using System.Text.Json;
using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.Repository.Dms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shuba.Worker.AI.Services;
using Shuba.Worker.AI.Workflows.Prompts;

namespace Shuba.Worker.AI.Workflows;

/// <summary>
/// Workflow per-dokumen:
/// 1. Baca file content dari FullPath
/// 2. Kirim ke Web AI provider → parse JSON response
/// 3. Update TblDocuments (CategoryName, SubCategoryName, DocumentTypeName)
/// 4. Update TblDocumentFiles (DocumentSummary = Summary + Points)
/// </summary>
public class DocumentClassificationWorkflow
{
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
        _logger.LogInformation("[Workflow] 📄 Processing DocId={DocId} via {Provider}",
            task.DocumentId, providerSlot.Provider.WebAiName);

        // Step 1: Baca konten file
        var fileContent = await ReadFileContentAsync(task.FullPath);
        if (string.IsNullOrWhiteSpace(fileContent))
        {
            _logger.LogWarning("[Workflow] ⚠️ File kosong atau tidak bisa dibaca: {Path}", task.FullPath);
            throw new Exception($"Cannot read file content from: {task.FullPath}");
        }

        // Limit konten agar tidak terlalu panjang untuk Web AI
        if (fileContent.Length > 15000)
            fileContent = fileContent[..15000] + "\n\n[... konten terpotong ...]";

        // Step 2: Kirim ke Web AI
        var systemPrompt = ClassificationPrompt.Build(fileContent);
        var userMessage = ClassificationPrompt.UserMessage;

        var rawResponse = await _orchestrator.AskAsync(
            providerSlot.Provider,
            providerSlot.Selectors,
            systemPrompt,
            userMessage,
            sessionId,
            ct);

        _logger.LogInformation("[Workflow] 📩 Raw response length: {Len} chars", rawResponse?.Length ?? 0);

        // Step 3: Parse JSON response
        var result = ParseClassificationResponse(rawResponse);
        if (result == null)
        {
            _logger.LogWarning("[Workflow] ⚠️ Could not parse classification JSON from response");
            throw new Exception("Failed to parse classification JSON from Web AI response.");
        }

        _logger.LogInformation("[Workflow] ✅ Parsed: Category={Cat}, SubCat={Sub}, DocType={Type}",
            result.Category, result.SubCategory, result.DocumentType);

        // Step 4: Update database
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        // Update TblDocuments
        var document = await dbContext.Set<Documents>()
            .FirstOrDefaultAsync(d => d.Id == task.DocumentId, ct);

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
            .Where(df => df.DocumentID == task.DocumentId)
            .ToListAsync(ct);

        foreach (var docFile in docFiles)
        {
            docFile.DocumentSummary = summaryContent;
            dbContext.Update(docFile);
        }

        await dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("[Workflow] 💾 Database updated for DocId={DocId}", task.DocumentId);
    }

    // ────────────────────────────────────────────────────────
    //  HELPERS
    // ────────────────────────────────────────────────────────

    private static async Task<string?> ReadFileContentAsync(string? fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
            return null;

        try
        {
            return await File.ReadAllTextAsync(fullPath);
        }
        catch
        {
            return null;
        }
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
            // Try to find JSON object in the response
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
        // Find opening brace
        int start = raw.IndexOf('{');
        if (start < 0) return null;

        // Find matching closing brace
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
