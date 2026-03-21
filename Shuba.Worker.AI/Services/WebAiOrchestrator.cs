using Microsoft.Playwright;
using Shuba.Worker.AI.Models;
using Shuba.Worker.AI.WebAI;

namespace Shuba.Worker.AI.Services;

/// <summary>
/// Manages Playwright CDP connection, per-provider page caching,
/// session counter (reset every N requests), and retry logic.
/// </summary>
public class WebAiOrchestrator : IAsyncDisposable
{
    private readonly string _cdpUrl;
    private readonly WorkerSettings _settings;

    private IPlaywright? _playwright;
    private IBrowser? _browser;

    private readonly Dictionary<string, IPage> _pages = new();
    private readonly Dictionary<string, int> _requestCounters = new();

    public WebAiOrchestrator(string cdpUrl, WorkerSettings settings)
    {
        _cdpUrl = cdpUrl;
        _settings = settings;
    }

    // ────────────────────────────────────────────────────────
    //  CONNECT
    // ────────────────────────────────────────────────────────

    public async Task ConnectAsync()
    {
        Console.WriteLine($"[CDP] 🔌 Connecting to Chrome at {_cdpUrl}...");
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.ConnectOverCDPAsync(_cdpUrl);
        Console.WriteLine("[CDP] ✅ Connected!");
    }

    // ────────────────────────────────────────────────────────
    //  GET / CREATE PAGE PER PROVIDER
    // ────────────────────────────────────────────────────────

    public async Task<IPage> GetOrCreatePageAsync(WebAiProvider provider)
    {
        if (_browser == null)
            throw new InvalidOperationException("Call ConnectAsync first.");

        var context = _browser.Contexts.Count > 0
            ? _browser.Contexts[0]
            : await _browser.NewContextAsync();

        try { await context.GrantPermissionsAsync(new[] { "clipboard-read", "clipboard-write" }); }
        catch { /* CDP context may not support it */ }

        if (_pages.TryGetValue(provider.WebAiName, out var cached))
        {
            try
            {
                _ = cached.Url;
                await cached.BringToFrontAsync();
                return cached;
            }
            catch
            {
                _pages.Remove(provider.WebAiName);
            }
        }

        IPage? targetPage = null;
        if (!string.IsNullOrEmpty(provider.WebAiUrl))
        {
            var host = new Uri(provider.WebAiUrl).Host;
            foreach (var p in context.Pages)
            {
                if (p.Url.Contains(host, StringComparison.OrdinalIgnoreCase))
                {
                    targetPage = p;
                    break;
                }
            }
        }

        if (targetPage == null)
        {
            targetPage = await context.NewPageAsync();
            await SFCoreWebAIBrowser.NewSession(targetPage, provider);
        }

        await targetPage.BringToFrontAsync();
        _pages[provider.WebAiName] = targetPage;
        return targetPage;
    }

    // ────────────────────────────────────────────────────────
    //  MAIN: ASK
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Calculates dynamic delay (ms) based on file size.
    /// ≤5MB → 4s, ≤20MB → 8s, ≤50MB → 20s, >50MB → 40s
    /// </summary>
    public static int CalculateUploadDelay(long fileSizeBytes)
    {
        var sizeMb = fileSizeBytes / (1024.0 * 1024.0);
        return sizeMb switch
        {
            <= 5 => 4_000,
            <= 20 => 8_000,
            <= 50 => 20_000,
            _ => 40_000
        };
    }

    public async Task<string> AskAsync(
        WebAiProvider provider,
        IList<WebAiSelector> selectors,
        string systemPrompt,
        string userMessage,
        string sessionId,
        List<string>? filePaths = null,
        long fileSizeBytes = 0,
        bool isDataArray = false,
        CancellationToken ct = default)
    {
        var page = await GetOrCreatePageAsync(provider);

        _requestCounters.TryGetValue(provider.WebAiName, out int count);
        count++;
        _requestCounters[provider.WebAiName] = count;

        if (count > 1 && count % _settings.SessionResetAfterNRequests == 1)
        {
            Console.WriteLine($"[Session] 🔄 {provider.WebAiName} reached {_settings.SessionResetAfterNRequests} requests → NewSession");
            await SFCoreWebAIBrowser.NewSession(page, provider);
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            await SFCoreWebAIBrowser.WaitForPageReady(page, selectors);
            await SFCoreWebAIBrowser.CheckClearChat(page, selectors, "Clear-Chat");

            // Upload file(s) jika ada — route berdasarkan provider name
            if (filePaths != null && filePaths.Count > 0)
            {
                foreach (var filePath in filePaths)
                {
                    Console.WriteLine($"[{provider.WebAiName}] 📎 Uploading file: {Path.GetFileName(filePath)}");
                    await SFCoreWebAIBrowser.UploadFile(page, selectors, filePath, provider.WebAiName, fileSizeBytes);
                }
            }

            await SFCoreWebAIBrowser.SendMessage(page, selectors, systemPrompt, userMessage);
            await SFCoreWebAIBrowser.ScrollElementToBottom(page);
            var response = await SFCoreWebAIBrowser.WaitAndExtractResponse(page, provider.WebAiName, selectors, sessionId, isDataArray: isDataArray);

            if (!string.IsNullOrWhiteSpace(response))
            {
                if (response.Length > 1000)
                {
                    Console.WriteLine($"[{provider.WebAiName}] ✅ Got response ({response.Length} chars).");
                    return response;
                }
                else
                {
                    await Task.Delay(5_000);
                    response = await SFCoreWebAIBrowser.WaitAndExtractResponse(page, provider.WebAiName, selectors, sessionId, isDataArray: isDataArray);
                    if (response.Length > 1000)
                    {
                        Console.WriteLine($"[{provider.WebAiName}] ✅ Got response ({response.Length} chars).");
                        return response;
                    }
                }
            }

            Console.WriteLine($"[{provider.WebAiName}] ⚠️ Empty response.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{provider.WebAiName}] ✗  failed: {ex.Message}");
        }

        throw new Exception($"[{provider.WebAiName}] Failed for session: {sessionId}");
    }

    public async Task<string> GetResponseOnlyAsync(
        WebAiProvider provider,
        IList<WebAiSelector> selectors,
        string systemPrompt,
        string userMessage,
        string sessionId,
        List<string>? filePaths = null,
        long fileSizeBytes = 0,
        bool isDataArray = false,
        CancellationToken ct = default)
    {
        var page = await GetOrCreatePageAsync(provider);

        _requestCounters.TryGetValue(provider.WebAiName, out int count);
        count++;
        _requestCounters[provider.WebAiName] = count;

        if (count > 1 && count % _settings.SessionResetAfterNRequests == 1)
        {
            Console.WriteLine($"[Session] 🔄 {provider.WebAiName} reached {_settings.SessionResetAfterNRequests} requests → NewSession");
            await SFCoreWebAIBrowser.NewSession(page, provider);
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            await SFCoreWebAIBrowser.WaitForPageReady(page, selectors);
            await SFCoreWebAIBrowser.CheckClearChat(page, selectors, "Clear-Chat");
            var response = await SFCoreWebAIBrowser.WaitAndExtractResponse(page, provider.WebAiName, selectors, sessionId, isDataArray: isDataArray);

            if (!string.IsNullOrWhiteSpace(response))
            {
                if (response.Length > 1000)
                {
                    Console.WriteLine($"[{provider.WebAiName}] ✅ Got response ({response.Length} chars).");
                    return response;
                }
                else
                {
                    await Task.Delay(5_000);
                    response = await SFCoreWebAIBrowser.WaitAndExtractResponse(page, provider.WebAiName, selectors, sessionId, isDataArray: isDataArray);
                    if (response.Length > 1000)
                    {
                        Console.WriteLine($"[{provider.WebAiName}] ✅ Got response ({response.Length} chars).");
                        return response;
                    }
                }
            }

            Console.WriteLine($"[{provider.WebAiName}] ⚠️ Empty response.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{provider.WebAiName}] ✗  failed: {ex.Message}");
        }

        throw new Exception($"[{provider.WebAiName}] Failed for session: {sessionId}");
    }
    // ────────────────────────────────────────────────────────
    //  DISPOSE
    // ────────────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        if (_browser?.IsConnected == true)
        {
            try { await _browser.CloseAsync(); } catch { }
        }
        _playwright?.Dispose();
    }
}
