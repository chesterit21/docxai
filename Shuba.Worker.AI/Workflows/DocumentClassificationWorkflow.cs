using System.Text.Json;
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
        string rawResponse;
        var needsChunking = NeedsChunking(fileSize, providerName);

        if (needsChunking)
        {
            rawResponse = await ExecuteChunkedUploadAsync(task, providerSlot, fileSize, sessionId, documentTypes, ct);
        }
        else
        {
            rawResponse = await ExecuteDirectUploadAsync(task, providerSlot, sessionId, documentTypes, ct);
        }

        // Parse JSON response
        var result = ParseClassificationResponse(rawResponse);
        if (result == null)
        {
            _logger.LogWarning("[Workflow] ⚠️ Could not parse classification JSON from response");
            throw new Exception("Failed to parse classification JSON from Web AI response.");
        }

        _logger.LogInformation("[Workflow] ✅ Parsed: Category={Cat}, SubCat={Sub}, DocType={Type}",
            result.Category, result.SubCategory, result.DocumentType);

        // Update database
        await UpdateDatabaseAsync(task.DocumentId, result, ct);
    }

    // ────────────────────────────────────────────────────────
    //  STRATEGY: DIRECT UPLOAD (< 20MB, atau DS/ZAI < 50MB)
    // ────────────────────────────────────────────────────────

    private async Task<string> ExecuteDirectUploadAsync(
        AgentPollingTaskDocument task,
        ProviderSlot providerSlot,
        string sessionId,
        List<TmDocumentType> documentTypes,
        CancellationToken ct)
    {
        _logger.LogInformation("[Workflow] 📤 Direct upload: {Path}", task.FullPath);

        var systemPrompt = ClassificationPrompt.Build("", documentTypes);
        var userMessage = ClassificationPrompt.UserMessage;

        return await _orchestrator.AskAsync(
            providerSlot.Provider,
            providerSlot.Selectors,
            systemPrompt,
            userMessage,
            sessionId,
            filePaths: new List<string> { task.FullPath! },
            ct: ct);
    }

    // ────────────────────────────────────────────────────────
    //  STRATEGY: CHUNKED UPLOAD (file besar, multi-part)
    // ────────────────────────────────────────────────────────

    private async Task<string> ExecuteChunkedUploadAsync(
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
                           + ClassificationPrompt.Build("", documentTypes);
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
                    ct: ct);

                if (!isLast)
                {
                    _logger.LogInformation("[Workflow] ⏳ Chunk {Part}/{Total} uploaded, waiting before next...",
                        partNumber, totalParts);
                    await Task.Delay(3000, ct); // jeda antar chunk
                }
            }

            return rawResponse;
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
                    return raw[start..(i + 1)];
            }
        }

        return null;
    }
}

public class ClassificationResult
{
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? DocumentType { get; set; }
    public string? Summary { get; set; }
    public string? Points { get; set; }
}
