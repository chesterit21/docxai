using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Repository.Dms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shuba.Worker.AI.Services;
using Shuba.Worker.AI.Workflows.Prompts;

namespace Shuba.Worker.AI.Workflows;

/// <summary>
/// Workflow per-dokumen:
/// 1. Cek file size → tentukan strategi (direct / split-chunked)
/// 2. Upload file ke Web AI provider (single atau multi-part)
/// 3. Parse JSON response → Update TblDocuments + TblDocumentFiles
/// </summary>
public class DocumentClassificationWorkflow
{
    private const long SIZE_20MB = 20L * 1024 * 1024;
    private const long SIZE_50MB = 50L * 1024 * 1024;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WebAiOrchestrator _orchestrator;
    private readonly ILogger<DocumentClassificationWorkflow> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DocumentClassificationWorkflow(
        IServiceScopeFactory scopeFactory,
        WebAiOrchestrator orchestrator,
        ILogger<DocumentClassificationWorkflow> logger)
    {
        _scopeFactory = scopeFactory;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        AgentPollingTaskDocument task,
        ProviderSlot providerSlot,
        CancellationToken ct)
    {
        var sessionId = task.Id.ToString();
        var providerName = providerSlot.Provider.WebAiName;

        _logger.LogInformation("[Workflow] 📄 Processing DocId={DocId} via {Provider}",
            task.DocumentId, providerName);

        // Validasi file
        if (string.IsNullOrEmpty(task.FullPath) || !File.Exists(task.FullPath))
        {
            _logger.LogWarning("[Workflow] ⚠️ File tidak ditemukan: {Path}", task.FullPath);
            throw new Exception($"File not found: {task.FullPath}");
        }

        var fileInfo = new FileInfo(task.FullPath);
        var fileSize = fileInfo.Length;
        _logger.LogInformation("[Workflow] 📏 File size: {Size}MB, Provider: {Provider}",
            fileSize / (1024 * 1024), providerName);

        // Ambil taksonomi dinamis
        var documentTypes = await GetDocumentTypesAsync(ct);

        // Tentukan strategi
        var needsChunking = NeedsChunking(fileSize, providerName);
        if (needsChunking)
        {
            await ExecuteChunkedUploadAsync(task, providerSlot, fileSize, sessionId, documentTypes, ct);
        }
        else
        {
            await ExecuteDirectUploadAsync(task, providerSlot, fileSize, sessionId, documentTypes, ct);
        }
    }

    // ────────────────────────────────────────────────────────
    //  STRATEGY: DIRECT UPLOAD (< 20MB, atau DS/ZAI < 50MB)
    // ────────────────────────────────────────────────────────

    private async Task ExecuteDirectUploadAsync(
        AgentPollingTaskDocument task,
        ProviderSlot providerSlot,
        long fileSize,
        string sessionId,
        List<TmDocumentType> documentTypes,
        CancellationToken ct)
    {
        _logger.LogInformation("[Workflow] 📤 Direct upload: {Path}", task.FullPath);

        var systemPrompt = ClassificationPrompt.Build(sessionId, documentTypes);
        var userMessage = ClassificationPrompt.UserMessage;

        var rawResponse = await _orchestrator.AskAsync(
            providerSlot.Provider,
            providerSlot.Selectors,
            systemPrompt,
            userMessage,
            sessionId,
            filePaths: new List<string> { task.FullPath! },
            fileSizeBytes: fileSize,
            isDataArray: false,
            ct: ct);

        var result = ParseClassificationResponse(rawResponse);
        if (result == null)
        {
            _logger.LogWarning("[Workflow] ⚠️ Could not parse classification JSON from response");
            throw new Exception("Failed to parse classification JSON from Web AI response.");
        }
        // Update database (Phase 1 result)
        await UpdateDatabaseAsync(task.DocumentId, result, ct);

        _logger.LogInformation("[Workflow] ✅ Parsed: Category={Cat}, SubCat={Sub}, DocType={Type}",
            result.Category, result.SubCategory, result.DocumentType);

        // ────────────────────────────────────────────────────────
        //  PHASE 2: EXTRACT DATA
        // ────────────────────────────────────────────────────────
        _logger.LogInformation("[Workflow] 🔍 Phase 2: Starting data extraction for {Type}", result.DocumentType);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        // 1. Ambil DocumentTypeId berdasarkan hasil klasifikasi
        var docType = await dbContext.Set<TmDocumentType>()
            .Include(t => t.Attributes)
            .FirstOrDefaultAsync(t => t.CategoryName.ToLower() == result.Category!.ToLower()
                                   && t.SubCategoryName.ToLower() == result.SubCategory!.ToLower()
                                   && t.DocumentType.ToLower() == result.DocumentType!.ToLower(), ct);
        string systemPrompt2;
        if (docType == null || docType.Attributes == null || !docType.Attributes.Any())
        {
            systemPrompt2 = ClassificationPrompt.SystemMessagePhase2EmptyAttribute("ext-attr-" + sessionId);
        }
        else
        {
            systemPrompt2 = ClassificationPrompt.SystemMessagePhase2("ext-attr-" + sessionId, docType.Attributes);
        }

        // 2. Build Phase 2 Prompts
        var userMessage2 = ClassificationPrompt.UserMessagePhase2;

        // 3. Ask AI Phase 2
        var rawResponse2 = await _orchestrator.AskAsync(
            providerSlot.Provider,
            providerSlot.Selectors,
            systemPrompt2,
            userMessage2,
            "ext-attr-" + sessionId,
            null,
            fileSizeBytes: fileSize,
            isDataArray: true, // Phase 2 returns array of entities
            ct: ct);

        // 4. Parse Results
        var extractedEntities = ParseExtractionResponse(rawResponse2);
        if (extractedEntities == null || !extractedEntities.Any())
        {
            _logger.LogWarning("[Workflow] ⚠️ Phase 2: Could not parse any extracted entities.");
            return;
        }

        // 5. Persist to Database
        await SaveExtractedEntitiesAsync(task.DocumentId, extractedEntities, ct);
        _logger.LogInformation("[Workflow] ✅ Phase 2: {Count} entities extracted and saved.", extractedEntities.Count);
    }

    // ────────────────────────────────────────────────────────
    //  STRATEGY: CHUNKED UPLOAD (file besar, multi-part)
    // ────────────────────────────────────────────────────────

    private async Task ExecuteChunkedUploadAsync(
        AgentPollingTaskDocument task,
        ProviderSlot providerSlot,
        long fileSize,
        string sessionId,
        List<TmDocumentType> documentTypes,
        CancellationToken ct)
    {
        var chunkSize = FileSplitter.GetChunkSizeForProvider(providerSlot.Provider.WebAiName);
        var taskGuid = task.Id;

        _logger.LogInformation("[Workflow] 📦 Chunked upload: FileSize={Size}MB, ChunkSize={Chunk}MB",
            fileSize / (1024 * 1024), chunkSize / (1024 * 1024));

        // Split file ke temp directory
        var chunks = await FileSplitter.SplitFileAsync(task.FullPath!, chunkSize, taskGuid);
        var totalParts = chunks.Count;

        _logger.LogInformation("[Workflow] 📦 Split into {Count} chunks", totalParts);

        try
        {
            string rawResponse = string.Empty;

            for (int i = 0; i < totalParts; i++)
            {
                var partNumber = i + 1;
                var chunkPath = chunks[i];
                var isLast = partNumber == totalParts;

                string prompt;
                string userMsg;

                if (isLast)
                {
                    // Chunk terakhir: prompt klasifikasi asli
                    prompt = ChunkedUploadPrompt.BuildFinalPrompt(partNumber, totalParts)
                           + ClassificationPrompt.Build(sessionId, documentTypes);
                    userMsg = ChunkedUploadPrompt.ChunkUserMessage(partNumber, totalParts);
                }
                else if (partNumber == 1)
                {
                    // Chunk pertama: intro
                    prompt = ChunkedUploadPrompt.BuildIntroPrompt(partNumber, totalParts);
                    userMsg = ChunkedUploadPrompt.ChunkUserMessage(partNumber, totalParts);
                }
                else
                {
                    // Chunk tengah
                    prompt = ChunkedUploadPrompt.BuildMiddlePrompt(partNumber, totalParts);
                    userMsg = ChunkedUploadPrompt.ChunkUserMessage(partNumber, totalParts);
                }

                _logger.LogInformation("[Workflow] 📤 Uploading chunk {Part}/{Total}: {Path}",
                    partNumber, totalParts, Path.GetFileName(chunkPath));

                rawResponse = await _orchestrator.AskAsync(
                    providerSlot.Provider,
                    providerSlot.Selectors,
                    prompt,
                    userMsg,
                    sessionId,
                    filePaths: new List<string> { chunkPath },
                    fileSizeBytes: new FileInfo(chunkPath).Length,
                    isDataArray: false,
                    ct: ct);

                if (!isLast)
                {
                    _logger.LogInformation("[Workflow] ⏳ Chunk {Part}/{Total} uploaded, waiting before next...",
                        partNumber, totalParts);
                    await Task.Delay(3000, ct); // jeda antar chunk
                }
            }

            // 1. Parse Phase 1 Result
            var result = ParseClassificationResponse(rawResponse);
            if (result == null)
            {
                _logger.LogWarning("[Workflow] ⚠️ Could not parse classification JSON from chunked response");
                throw new Exception("Failed to parse classification JSON from Web AI response.");
            }

            // 2. Update database (Phase 1)
            await UpdateDatabaseAsync(task.DocumentId, result, ct);
            _logger.LogInformation("[Workflow] ✅ Parsed: Category={Cat}, SubCat={Sub}, DocType={Type}",
                result.Category, result.SubCategory, result.DocumentType);

            // ────────────────────────────────────────────────────────
            //  PHASE 2: EXTRACT DATA
            // ────────────────────────────────────────────────────────
            _logger.LogInformation("[Workflow] 🔍 Phase 2: Starting data extraction for {Type}", result.DocumentType);

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

            // 3. Ambil DocumentTypeId berdasarkan hasil klasifikasi
            var docType = await dbContext.Set<TmDocumentType>()
                .Include(t => t.Attributes)
                .FirstOrDefaultAsync(t => t.CategoryName == result.Category
                                       && t.SubCategoryName == result.SubCategory
                                       && t.DocumentType == result.DocumentType, ct);

            if (docType == null || docType.Attributes == null || !docType.Attributes.Any())
            {
                _logger.LogInformation("[Workflow] ℹ️ No specific attributes found for {Type}. Phase 2 skipped.", result.DocumentType);
                return;
            }

            // 4. Build Phase 2 Prompts
            var systemPrompt2 = ClassificationPrompt.SystemMessagePhase2(sessionId, docType.Attributes);
            var userMessage2 = ClassificationPrompt.UserMessagePhase2;

            // 5. Ask AI Phase 2 (Upload ulang/lanjutan dengan prompt baru)
            // Note: Kita kirim file aslinya lagi untuk Phase 2 ekstraksi
            var rawResponse2 = await _orchestrator.AskAsync(
                providerSlot.Provider,
                providerSlot.Selectors,
                systemPrompt2,
                userMessage2,
                sessionId,
                filePaths: new List<string> { task.FullPath! },
                fileSizeBytes: fileSize,
                isDataArray: true,
                ct: ct);

            // 6. Parse Results
            var extractedEntities = ParseExtractionResponse(rawResponse2);
            if (extractedEntities == null || !extractedEntities.Any())
            {
                _logger.LogWarning("[Workflow] ⚠️ Phase 2: Could not parse any extracted entities.");
                return;
            }

            // 7. Persist to Database
            await SaveExtractedEntitiesAsync(task.DocumentId, extractedEntities, ct);
            _logger.LogInformation("[Workflow] ✅ Phase 2: {Count} entities extracted and saved.", extractedEntities.Count);
        }
        finally
        {
            // Cleanup temp files
            FileSplitter.CleanupTempFiles(taskGuid);
        }
    }

    // ────────────────────────────────────────────────────────
    //  DECISION: PERLU CHUNKING ATAU TIDAK?
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Menentukan apakah file perlu di-chunk:
    /// - Qwen: chunk jika > 20MB (per 19MB)
    /// - DS/ZAI: chunk jika > 50MB (per 45MB)
    /// </summary>
    private static bool NeedsChunking(long fileSize, string providerName)
    {
        var isQwen = providerName.Contains("Qwen", StringComparison.OrdinalIgnoreCase);

        if (isQwen)
            return fileSize > SIZE_20MB; // Qwen: chunk di atas 20MB

        return fileSize > SIZE_50MB; // DS/ZAI: chunk di atas 50MB
    }

    // ────────────────────────────────────────────────────────
    //  DATABASE UPDATE
    // ────────────────────────────────────────────────────────

    private async Task UpdateDatabaseAsync(int documentId, ClassificationResult result, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        // Update TblDocuments
        var document = await dbContext.Set<Documents>()
            .FirstOrDefaultAsync(d => d.Id == documentId, ct);

        if (document != null)
        {
            document.CategoryName = result.Category ?? document.CategoryName;
            document.SubCategoryName = result.SubCategory ?? document.SubCategoryName;
            document.DocumentTypeName = result.DocumentType ?? document.DocumentTypeName;
            document.DocumentDesc = result.Summary ?? document.DocumentDesc;
            dbContext.Update(document);
        }

        // Update TblDocumentFiles — set DocumentSummary
        var summaryContent = BuildSummaryContent(result.Summary, result.Points);
        var docFiles = await dbContext.Set<DocumentFiles>()
            .Where(df => df.DocumentID == documentId)
            .ToListAsync(ct);

        foreach (var docFile in docFiles)
        {
            docFile.DocumentSummary = summaryContent;
            dbContext.Update(docFile);
        }

        await dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("[Workflow] 💾 Database updated for DocId={DocId}", documentId);
    }

    private async Task SaveExtractedEntitiesAsync(int documentId, List<ExtractionResult> entities, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        // 1. Persist to New Structured Table (DocumentExtractedEntities)
        var existingEntities = await dbContext.Set<DocumentExtractedEntities>()
            .Where(e => e.DocumentId == documentId)
            .ToListAsync(ct);

        foreach (var entity in entities)
        {
            var attrName = entity.AttributeName ?? "Unknown";
            var existing = existingEntities.FirstOrDefault(e => e.AttributeName == attrName);

            if (existing != null)
            {
                // Update
                existing.ValueText = entity.ValueText;
                existing.ValueNumber = entity.ValueNumber;
                existing.ValueDecimal = entity.ValueDecimal;
                existing.ValueBoolean = entity.ValueBoolean;
                existing.ValueDate = entity.ValueDate;
                dbContext.Update(existing);
            }
            else
            {
                // Insert
                var newEntity = new DocumentExtractedEntities
                {
                    DocumentId = documentId,
                    AttributeName = attrName,
                    ValueText = entity.ValueText,
                    ValueNumber = entity.ValueNumber,
                    ValueDecimal = entity.ValueDecimal,
                    ValueBoolean = entity.ValueBoolean,
                    ValueDate = entity.ValueDate
                };
                dbContext.Add(newEntity);
            }
        }

        await dbContext.SaveChangesAsync(ct);

        // 2. Persist to Legacy Table (TblDocumentAttributes)
        var legacyList = new List<LegacyAttributeValue>();
        long baseId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        foreach (var entity in entities)
        {
            string valStr = entity.ValueText ??
                           entity.ValueNumber?.ToString() ??
                           entity.ValueDecimal?.ToString() ??
                           entity.ValueBoolean?.ToString() ??
                           entity.ValueDate?.ToString("yyyy-MM-dd HH:mm:ss") ??
                           "";

            var attrElement = new
            {
                type = "text-field",
                label = entity.AttributeName,
                placeholder = entity.AttributeName,
                helptext = entity.AttributeName,
                max = 500,
                name = entity.AttributeName?.ToLower().Replace(" ", "_")
            };

            legacyList.Add(new LegacyAttributeValue
            {
                attributeName = entity.AttributeName ?? "Unknown",
                attributeType = "text-field",
                attributeElement = JsonSerializer.Serialize(attrElement, jsonOptions),
                id = baseId++,
                value = valStr
            });
        }

        var legacyRecord = await dbContext.Set<DocumentAttributes>()
            .FirstOrDefaultAsync(a => a.DocumentID == documentId, ct);

        if (legacyRecord != null)
        {
            legacyRecord.AttributeValues = JsonSerializer.Serialize(legacyList, jsonOptions);
            dbContext.Update(legacyRecord);
        }
        else
        {
            legacyRecord = new DocumentAttributes
            {
                DocumentID = documentId,
                AttributeValues = JsonSerializer.Serialize(legacyList, jsonOptions)
            };
            dbContext.Add(legacyRecord);
        }

        await dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("[Workflow] 💾 All extraction data persisted for DocId={DocId}", documentId);
    }

    private List<ExtractionResult>? ParseExtractionResponse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        try
        {
            var json = ExtractJsonFromRaw(raw);
            if (json == null) return null;

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("data", out var dataElement))
            {
                return JsonSerializer.Deserialize<List<ExtractionResult>>(dataElement.GetRawText(), _jsonOptions);
            }

            return JsonSerializer.Deserialize<List<ExtractionResult>>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Workflow] Extraction parse error: {Msg}", ex.Message);
            return null;
        }
    }

    // ────────────────────────────────────────────────────────
    //  HELPERS
    // ────────────────────────────────────────────────────────

    private async Task<List<TmDocumentType>> GetDocumentTypesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        return await dbContext.Set<TmDocumentType>().ToListAsync(ct);
    }

    private static string BuildSummaryContent(string? summary, string? points)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(summary))
            parts.Add(summary);
        if (!string.IsNullOrWhiteSpace(points))
            parts.Add("## Poin-poin Penting\n\n" + points);

        return string.Join("\n\n", parts);
    }

    private ClassificationResult? ParseClassificationResponse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        try
        {
            var json = ExtractJsonFromRaw(raw);
            if (json == null) return null;

            // Handle wrapper { "session_id": "...", "data": { ... } }
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("data", out var dataElement))
            {
                return JsonSerializer.Deserialize<ClassificationResult>(dataElement.GetRawText(), _jsonOptions);
            }

            // Fallback for old format or direct object
            return JsonSerializer.Deserialize<ClassificationResult>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Workflow] Parse error: {Msg}", ex.Message);
            return null;
        }
    }

    private static string? ExtractJsonFromRaw(string raw)
    {
        int start = raw.IndexOf('{');
        if (start < 0) return null;

        int depth = 0;
        bool inString = false;
        bool escape = false;

        for (int i = start; i < raw.Length; i++)
        {
            char c = raw[i];
            if (escape) { escape = false; continue; }
            if (c == '\\' && inString) { escape = true; continue; }
            if (c == '"') { inString = !inString; continue; }
            if (inString) continue;

            if (c == '{') depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    var json = raw[start..(i + 1)];
                    // Replace NBSP (\u00A0) with a regular space to avoid JsonDocument parse errors (0xC2 byte)
                    return json.Replace('\u00A0', ' ');
                }
            }
        }

        return null;
    }
}

public class LegacyAttributeValue
{
    public string attributeName { get; set; }
    public string attributeType { get; set; } = "text-field";
    public string attributeElement { get; set; }
    public long id { get; set; }
    public string value { get; set; }
}

public class ExtractionResult
{
    public string AttributeName { get; set; }
    public string? ValueText { get; set; }
    public int? ValueNumber { get; set; }
    public decimal? ValueDecimal { get; set; }
    public bool? ValueBoolean { get; set; }
    public DateTime? ValueDate { get; set; }
}

public class ClassificationResult
{
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? DocumentType { get; set; }
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Summary { get; set; }

    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Points { get; set; }
}

