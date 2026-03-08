
appsettings.json
540 B • 26 lines

{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=dms;Username=postgres;Password=P@ssw0rd#2026"
  },
  "Cdp": {
    "Url": "<http://localhost:9222>"
  },
  "Providers": {
    "DeepSeek": {
      "Name": "DeepSeek",
      "Url": "<https://chat.deepseek.com/>"
    },
    "Qwen": {
      "Name": "Qwen",
      "Url": "<https://chat.qwen.ai/>"
    },
    "ZAi": {
      "Name": "Z-AI",
      "Url": "<https://chat.z.ai/>"
    }
  },
  "Settings": {
    "SessionResetAfterNRequests": 10,
    "MaxRetryPerExtraction": 3
  }
}
---

// ── Config Models ─────────────────────────────────────────

public class AppSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string CdpUrl { get; set; } = string.Empty;
    public ProviderConfig DeepSeek { get; set; } = new();
    public ProviderConfig Qwen { get; set; } = new();
    public ProviderConfig ZAi { get; set; } = new();
    public int SessionResetAfterNRequests { get; set; } = 10;
    public int MaxRetryPerExtraction { get; set; } = 3;
}

public class ProviderConfig
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
---

using Microsoft.Playwright;
using DocTypeSeeder.Models;
using DocTypeSeeder.WebAI;

namespace DocTypeSeeder.Orchestration;

/// <summary>
/// Manages Playwright CDP connection, per-provider page caching,
/// session counter (reset every N requests), and retry-with-NewSession logic.
/// </summary>
public class WebAiOrchestrator : IAsyncDisposable
{
    private readonly string _cdpUrl;
    private readonly AppSettings_settings;

    private IPlaywright? _playwright;
    private IBrowser? _browser;

    // Per-provider page cache
    private readonly Dictionary<string, IPage> _pages = new();

    // Per-provider request counters (for session reset)
    private readonly Dictionary<string, int> _requestCounters = new();

    public WebAiOrchestrator(string cdpUrl, AppSettings settings)
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

        // Grant clipboard permissions (best effort)
        try { await context.GrantPermissionsAsync(new[] { "clipboard-read", "clipboard-write" }); }
        catch { /* CDP context may not support it */ }

        // Try reuse existing page for this provider URL
        if (_pages.TryGetValue(provider.WebAiName, out var cached))
        {
            // Check if still alive
            try
            {
                _ = cached.Url; // throws if closed
                await cached.BringToFrontAsync();
                return cached;
            }
            catch
            {
                _pages.Remove(provider.WebAiName);
            }
        }

        // Find matching page in context by URL host
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
    //  MAIN: ASK WITH RETRY + SESSION RESET
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Send a message to the AI provider and extract the response.
    ///
    /// Session reset: every N requests, navigates to a fresh chat.
    /// Retry logic: on extraction failure → NewSession → retry (max MaxRetry times).
    /// </summary>
    public async Task<string> AskAsync(
        WebAiProvider provider,
        IList<WebAiSelector> selectors,
        string systemPrompt,
        string userMessage,
        string sessionId,
        CancellationToken ct = default)
    {
        var page = await GetOrCreatePageAsync(provider);

        // Increment counter and check if session reset is needed
        _requestCounters.TryGetValue(provider.WebAiName, out int count);
        count++;
        _requestCounters[provider.WebAiName] = count;

        if (count > 1 && count % _settings.SessionResetAfterNRequests == 1)
        {
            Console.WriteLine($"[Session] 🔄 {provider.WebAiName} reached {_settings.SessionResetAfterNRequests} requests → NewSession");
            await SFCoreWebAIBrowser.NewSession(page, provider);
        }

        // Retry loop
        //while (attempt < _settings.MaxRetryPerExtraction)
        //{

            try
            {
                ct.ThrowIfCancellationRequested();

                await SFCoreWebAIBrowser.WaitForPageReady(page, selectors);
                await SFCoreWebAIBrowser.CheckClearChat(page, selectors, "Clear-Chat");
                await SFCoreWebAIBrowser.SendMessage(page, selectors, systemPrompt, userMessage);
                await SFCoreWebAIBrowser.ScrollElementToBottom(page);
                var response = await SFCoreWebAIBrowser.WaitAndExtractResponse(page,provider.WebAiName, selectors, sessionId);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    if(response.Length > 300)
                    {
                        Console.WriteLine($"[{provider.WebAiName}] ✅ Got response ({response.Length} chars).");
                        return response;
                    }
                    else
                    {
                        await Task.Delay(10_000);
                        response = await SFCoreWebAIBrowser.WaitAndExtractResponse(page,provider.WebAiName, selectors, sessionId);
                        if(response.Length > 300)
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

        //}

        throw new Exception($"[{provider.WebAiName}] All {_settings.MaxRetryPerExtraction} attempts failed for session: {sessionId}");
    }


    public async Task<string> GetResponseAgainAsync(
        WebAiProvider provider,
        IList<WebAiSelector> selectors,
         string sessionId,
        CancellationToken ct = default)
    {
        var page = await GetOrCreatePageAsync(provider);

        // Retry loop
        int attempt = 0;
        while (attempt < _settings.MaxRetryPerExtraction)
        {
            attempt++;
            Console.WriteLine($"\n[{provider.WebAiName}] 📤 Attempt {attempt}/{_settings.MaxRetryPerExtraction} | Session: {sessionId}");

            try
            {
                ct.ThrowIfCancellationRequested();

                await SFCoreWebAIBrowser.WaitForPageReady(page, selectors);
                await SFCoreWebAIBrowser.ScrollElementToBottom(page);
                await Task.Delay(5_000);
                var response = await SFCoreWebAIBrowser.WaitAndExtractResponsePhaseTwo(page,provider.WebAiName, selectors, sessionId);
                Console.WriteLine($"================================== PRINT RESPONSE -========================================.");
                Console.WriteLine("");
                Console.WriteLine(response);
                Console.WriteLine("");
                Console.WriteLine($"================================ END PRINT RESPONSE -=======================================.");

                if (!string.IsNullOrWhiteSpace(response))
                {
                    if(response.Length > 300)
                    {
                        Console.WriteLine($"[{provider.WebAiName}] ✅ Got response ({response.Length} chars).");
                        return response;
                    }
                }

                Console.WriteLine($"[{provider.WebAiName}] ⚠️ Empty response on attempt {attempt}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{provider.WebAiName}] ✗ Attempt {attempt} failed: {ex.Message}");
            }
        }

        throw new Exception($"[{provider.WebAiName}] All {_settings.MaxRetryPerExtraction} attempts failed for session: {sessionId}");
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
---

namespace DocTypeSeeder.WebAI;

public class WebAiProvider
{
    public string WebAiName { get; set; } = string.Empty;
    public string WebAiUrl { get; set; } = string.Empty;
}

public class WebAiSelector
{
    public string SelectorType { get; set; } = string.Empty;       // Input-Chat, Send, Response, Clear-Chat
    public string SelectorElement { get; set; } = string.Empty;    // actual selector value
    public string? LocatorStrategy { get; set; } = "css";          // css, xpath, placeholder
    public int SelectorIndex { get; set; } = 0;                    // priority order
}
---

using Microsoft.Playwright;
using HumanCursor.Playwright;

namespace DocTypeSeeder.WebAI;

/// <summary>
/// Browser automation core for Web AI providers.
/// Ported from SFCore.AI.WebAI.Core.SFCoreWebAIBrowser.
///
/// Design principles:
///   1. Multi-Selector Fallback — tries each selector ordered by SelectorIndex
///   2. Hybrid Cursor — ScrollIntoView → BoundingBox → HumanMoveTo → Click
///   3. Natural Delays — random 1.5-4s between steps
/// </summary>
public static class SFCoreWebAIBrowser
{
    private static readonly Random _rng = new();

    // ────────────────────────────────────────────────────────
    //  PUBLIC API
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Navigate to provider URL to start a fresh session (new chat).
    /// </summary>
    public static async Task NewSession(IPage page, WebAiProvider provider)
    {
        try
        {
            Console.WriteLine($"[{provider.WebAiName}] 🌐 Starting new session → {provider.WebAiUrl}");
            await page.ShowCursorAsync();

            if (!string.IsNullOrEmpty(provider.WebAiUrl))
            {
                await page.GotoAsync(provider.WebAiUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30_000
                });
            }

            await NaturalDelay();
            Console.WriteLine($"[{provider.WebAiName}] ✅ New session ready.");
            await Task.Delay(7500); // extra wait for SPA to fully load
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{provider.WebAiName}] ✗ NewSession failed: {ex.Message}");
            throw;
        }
    }

    // ────────────────────────────────────────────────────────
    //  PAGE READINESS
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Wait until the chatbot input is visible and ready.
    /// Tries CSS Input-Chat selectors, waiting up to timeoutMs for any one to appear.
    /// </summary>
    public static async Task WaitForPageReady(IPage page, IList<WebAiSelector> selectors, int timeoutMs = 30_000)
    {
        var inputSelectors = selectors
            .Where(s => s.SelectorType == "Input-Chat"
                     && (s.LocatorStrategy ?? "css").Trim().ToLowerInvariant() == "css")
            .OrderBy(s => s.SelectorIndex)
            .ToList();

        if (inputSelectors.Count == 0)
        {
            Console.WriteLine("[PageReady] ⚠️ No CSS Input-Chat selectors — waiting 5s fallback...");
            await Task.Delay(5000);
            return;
        }

        foreach (var sel in inputSelectors)
        {
            try
            {
                await page.WaitForSelectorAsync(sel.SelectorElement, new PageWaitForSelectorOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = timeoutMs
                });
                Console.WriteLine($"[PageReady] ✅ Ready! Found: {sel.SelectorElement}");
                await NaturalDelay();
                return;
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"[PageReady] ⏭️ Timeout for '{sel.SelectorElement}', trying next...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PageReady] ⏭️ Error for '{sel.SelectorElement}': {ex.Message}");
            }
        }

        Console.WriteLine("[PageReady] ⚠️ No Input-Chat visible. Proceeding anyway...");
    }

    // ────────────────────────────────────────────────────────
    //  INTERACTION
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Focus the chat input element using human cursor.
    /// </summary>
    public static async Task FocusInput(IPage page, IList<WebAiSelector> selectors, string selectorType = "Input-Chat")
    {
        Console.WriteLine($"[FocusInput] 📝 Focusing input ({selectorType})...");
        await ExecuteWithFallback(page, selectors, selectorType, async (locator, sel) =>
        {
            await HybridMoveAndClick(page, locator, "Input");
        });
        await NaturalDelay();
    }

    /// <summary>
    /// Clear any leftover text in the input field via sequential Backspace presses.
    /// </summary>
    public static async Task ClearFocusedInput(IPage page, IList<WebAiSelector> selectors, string selectorType = "Input-Chat")
    {
        Console.WriteLine($"[ClearInput] 🧹 Clearing '{selectorType}'...");
        await ExecuteWithFallback(page, selectors, selectorType, async (locator, sel) =>
        {
            try
            {
                var text = await locator.EvaluateAsync<string>(@"el => {
                    if (el.tagName === 'TEXTAREA' || el.tagName === 'INPUT') return el.value || '';
                    if (el.isContentEditable) return el.innerText || '';
                    return '';
                }");

                if (!string.IsNullOrWhiteSpace(text?.Trim()))
                {
                    Console.WriteLine($"[ClearInput] Found {text.Length} chars, clearing...");
                    await locator.FocusAsync();
                    await page.Keyboard.PressAsync("End");
                    await Task.Delay(_rng.Next(50, 150));
                    for (int i = 0; i < text.Length; i++)
                    {
                        await page.Keyboard.PressAsync("Backspace");
                        await Task.Delay(_rng.Next(30, 80));
                    }
                }
                else
                {
                    Console.WriteLine("[ClearInput] Already empty.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClearInput] ⚠️ Could not clear: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// Type text human-like (char by char, 60-120ms per char).
    /// </summary>
    public static async Task TypeHumanLike(IPage page, string text)
    {
        Console.WriteLine($"[TypeHumanLike] ⌨️ Typing {text.Length} chars...");
        await page.Keyboard.TypeAsync(text, new KeyboardTypeOptions { Delay = _rng.Next(60, 120) });
        await NaturalDelay();
    }

    /// <summary>
    /// Type text with Shift+Enter for newlines (avoids accidental submit).
    /// </summary>
    public static async Task TypeHumanLikeWithNewlines(IPage page, string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = normalized.Split('\n');
        Console.WriteLine($"[TypeHumanLike] ⌨️ Typing {text.Length} chars, {lines.Length} lines...");

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length > 0)
                await page.Keyboard.TypeAsync(lines[i], new KeyboardTypeOptions { Delay = _rng.Next(60, 120) });

            if (i < lines.Length - 1)
            {
                await page.Keyboard.DownAsync("Shift");
                await page.Keyboard.PressAsync("Enter");
                await page.Keyboard.UpAsync("Shift");
                await Task.Delay(_rng.Next(100, 300));
            }
        }
        await NaturalDelay();
    }

    /// <summary>
    /// Paste text via InsertText (fast, bulk insert).
    /// </summary>
    public static async Task PasteInput(IPage page, string text)
    {
        Console.WriteLine($"[PasteInput] 📋 Pasting {text.Length} chars...");
        await page.Keyboard.InsertTextAsync(text);
        await NaturalDelay();
    }

    /// <summary>
    /// Click the send button using human cursor movement.
    /// </summary>
    public static async Task ClickSend(IPage page, IList<WebAiSelector> selectors, string selectorType = "Send")
    {
        Console.WriteLine($"[ClickSend] 🖱️ Looking for send button...");
        await ExecuteWithFallback(page, selectors, selectorType, async (locator, sel) =>
        {
            await HybridMoveAndClick(page, locator, "Send");
        });
        Console.WriteLine("[ClickSend] ✅ Clicked!");
    }

    /// <summary>
    /// Check if a Clear-Chat button exists and click it if visible.
    /// Silently skips if not found.
    /// </summary>
    public static async Task CheckClearChat(IPage page, IList<WebAiSelector> selectors, string selectorType = "Clear-Chat")
    {
        var candidates = selectors
            .Where(s => s.SelectorType == selectorType)
            .OrderBy(s => s.SelectorIndex)
            .ToList();

        if (candidates.Count == 0)
        {
            Console.WriteLine($"[ClearChat] ℹ️ No '{selectorType}' selectors configured — skipping.");
            return;
        }

        foreach (var candidate in candidates)
        {
            try
            {
                var locator = PlaywrightLocatorResolver.Resolve(page, candidate.LocatorStrategy, candidate.SelectorElement);
                if (await locator.CountAsync() > 0 && await locator.IsVisibleAsync())
                {
                    Console.WriteLine($"[ClearChat] 🧹 Found! Clicking '{candidate.SelectorElement}'...");
                    await HybridMoveAndClick(page, locator, selectorType);
                    Console.WriteLine("[ClearChat] ✅ Chat cleared!");
                    await NaturalDelay();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClearChat] ⏭️ Skip '{candidate.SelectorElement}' — {ex.Message}");
            }
        }

        Console.WriteLine($"[ClearChat] ℹ️ No visible '{selectorType}' found — skipping.");
    }

    // ────────────────────────────────────────────────────────
    //  SEND MESSAGE (combined)
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Full send: FocusInput → Clear → Paste systemPrompt → Type userMessage → ClickSend.
    /// systemPrompt is always pasted (bulk). userMessage typed human-like with newline handling.
    /// </summary>
    public static async Task SendMessage(IPage page, IList<WebAiSelector> selectors, string systemMessage, string userMessage)
    {
        await FocusInput(page, selectors);
        await ClearFocusedInput(page, selectors);
        await PasteInput(page, systemMessage);
        await Task.Delay(2000);
        await TypeHumanLikeWithNewlines(page, userMessage);
        await Task.Delay(3000);
        await ClickSend(page, selectors);
        await Task.Delay(3000);
    }

    /// <summary>
    /// Scroll page to bottom (uses window.scrollTo).
    /// </summary>
    public static async Task ScrollElementToBottom(IPage page)
    {
        Console.WriteLine("[ScrollToBottom] ⬇️ Scrolling...");
        await page.EvaluateAsync("window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' })");
        await Task.Delay(_rng.Next(800, 1500));
        Console.WriteLine("[ScrollToBottom] ✅ Done.");
    }

    // ────────────────────────────────────────────────────────
    //  WAIT & EXTRACT RESPONSE
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Wait for AI to finish generating then extract the response.
    ///
    /// Detection: polls send button — when it becomes enabled again = AI done.
    /// Extraction: scans body text for JSON containing "session_id".
    ///
    /// Timeout: 180s default.
    /// </summary>
    public static async Task<string> WaitAndExtractResponse(
        IPage page,
        string WebAiName,
        IList<WebAiSelector> selectors,
        string? sessionId = null,
        int timeoutMs = 180_000,
        int pollIntervalMs = 2_000)
    {
        try
        {
            var sendSelectors = selectors
                .Where(s => s.SelectorType == "Send")
                .OrderBy(s => s.SelectorIndex)
                .ToList();
            var inputSelectors = selectors
                .Where(s => s.SelectorType == "Input-Chat")
                .OrderBy(s => s.SelectorIndex)
                .ToList();
            var isRunning = true;
            var countLoop = 0;

            Console.WriteLine("[WaitResponse] ⏳ Waiting for AI to finish generating...");
            Console.WriteLine($"[WaitResponse]   Send selectors: {sendSelectors.Count}");

            await NaturalDelay();

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            int pollCount = 0;
            await Task.Delay(20_000);
            while (isRunning)
            {
                pollCount++;

                try
                {
                    await FocusInput(page, inputSelectors);
                    await TypeHumanLikeWithNewlines(page, "terus kalau semisal nya,");
                    await Task.Delay(1_000);
                    Console.WriteLine($"[WaitResponse - {WebAiName}] Check Button).");
                    foreach (var sel in sendSelectors)
                    {
                        try
                        {
                            var btn = PlaywrightLocatorResolver.Resolve(page, sel.LocatorStrategy, sel.SelectorElement);
                            if (await btn.CountAsync() > 0)
                            {
                                isRunning = false;
                                break;
                            }
                            Console.WriteLine($"[Button Send - {WebAiName}] ✅ - AI Still Write The Answare).");
                            await NaturalDelay();
                        }
                        catch { continue; }
                    }
                    if (WebAiName.ToLower().Contains("qwen"))
                    {
                        foreach (var sel in sendSelectors)
                        {
                            try
                            {
                                var btn = PlaywrightLocatorResolver.Resolve(page, sel.LocatorStrategy, sel.SelectorElement);
                                if (await btn.CountAsync() > 0)
                                {
                                    isRunning = false;
                                    break;
                                }
                                await Task.Delay(650);
                            }
                            catch { continue; }
                        }
                        if (pollCount > 8)
                        {
                            isRunning = false;
                            break;
                        }
                    }
                    else
                    {
                        if (pollCount > 10)
                        {
                            isRunning = false;
                        }
                    }


                    var random = new Random();
                    for (int i = 0; i < "terus kalau semisal nya,".Length; i++)
                    {
                        await page.Keyboard.PressAsync("Backspace");
                        await Task.Delay(random.Next(60, 150)); // jeda antar hapus
                    }
                    
                    // Send button re-enabled = generation done
                    if (pollCount > 10)
                    {
                        await Task.Delay(1500); // settle delay
                        var bodyText = await page.InnerTextAsync("body");
                        var extracted = ExtractJsonBySessionId(bodyText, sessionId);
                        if (!string.IsNullOrEmpty(extracted))
                        {
                            Console.WriteLine($"[WaitResponse] ✅ Response extracted ({extracted.Length} chars).");
                            return extracted;
                        }
                        // Fallback: raw body
                        Console.WriteLine("[WaitResponse] ⚠️ No JSON found, returning raw body text.");
                        return bodyText;
                    }
                }
                catch (Exception pollEx)
                {
                    Console.WriteLine($"[WaitResponse] ⚠️ Poll error: {pollEx.Message}");
                }

                await Task.Delay(pollIntervalMs);
            }

            // Timeout — extract whatever we have
            Console.WriteLine("[WaitResponse] ⏰ Timeout! Extracting available content...");
            var fallbackBody = await page.InnerTextAsync("body");
                Console.WriteLine($"================================== PRINT fallbackBody -========================================.");
                Console.WriteLine("");
                Console.WriteLine(fallbackBody);
                Console.WriteLine("");
                Console.WriteLine($"================================ END OFF fallbackBody -=======================================.");
            var fallbackExtracted = ExtractJsonBySessionId(fallbackBody, sessionId);
            if (!string.IsNullOrEmpty(fallbackExtracted)) return fallbackExtracted;
            if (!string.IsNullOrEmpty(fallbackBody)) return fallbackBody;

            throw new TimeoutException("Web AI did not produce a response within the timeout period.");
        }
        catch (Exception ex) when (ex is not TimeoutException)
        {
            Console.WriteLine($"[WaitResponse] ✗ Failed: {ex.Message}");
            throw;
        }
    }


    public static async Task<string> WaitAndExtractResponsePhaseTwo(
        IPage page,
        string WebAiName,
        IList<WebAiSelector> selectors,
        string? sessionId = null,
        int timeoutMs = 180_000,
        int pollIntervalMs = 2_000)
    {
        try
        {
            var sendSelectors = selectors
                .Where(s => s.SelectorType == "Send")
                .OrderBy(s => s.SelectorIndex)
                .ToList();
            var inputSelectors = selectors
                .Where(s => s.SelectorType == "Input-Chat")
                .OrderBy(s => s.SelectorIndex)
                .ToList();
            var isRunning = true;
            var countLoop = 0;

            Console.WriteLine("[WaitResponse] ⏳ Waiting for AI to finish generating...");
            Console.WriteLine($"[WaitResponse]   Send selectors: {sendSelectors.Count}");

            await NaturalDelay();

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            int pollCount = 0;
            while (isRunning)
            {
                pollCount++;

                try
                {
                    await FocusInput(page, inputSelectors);
                    await TypeHumanLikeWithNewlines(page, "terus kalau semisal nya,");
                    await Task.Delay(1_000);
                    Console.WriteLine($"[WaitResponse - {WebAiName}] Check Button).");
                    foreach (var sel in sendSelectors)
                    {
                        try
                        {
                            var btn = PlaywrightLocatorResolver.Resolve(page, sel.LocatorStrategy, sel.SelectorElement);
                            if (await btn.CountAsync() > 0)
                            {
                                isRunning = false;
                                break;
                            }
                            Console.WriteLine($"[Button Send - {WebAiName}] ✅ - AI Still Write The Answare).");
                            await NaturalDelay();
                        }
                        catch { continue; }
                    }

                    if (WebAiName.ToLower().Contains("qwen"))
                    {
                        foreach (var sel in sendSelectors)
                        {
                            try
                            {
                                var btn = PlaywrightLocatorResolver.Resolve(page, sel.LocatorStrategy, sel.SelectorElement);
                                if (await btn.CountAsync() > 0)
                                {
                                    isRunning = false;
                                    break;
                                }
                                await Task.Delay(650);
                            }
                            catch { continue; }
                        }
                        if (pollCount > 8)
                        {
                            isRunning = false;
                            break;
                        }
                    }
                    else
                    {
                        if (pollCount > 10)
                        {
                            isRunning = false;
                            break;
                        }
                    }

                    var random = new Random();
                    for (int i = 0; i < "terus kalau semisal nya,".Length; i++)
                    {
                        await page.Keyboard.PressAsync("Backspace");
                        await Task.Delay(random.Next(60, 150)); // jeda antar hapus
                    }
                    
                    // Send button re-enabled = generation done
                    if (pollCount > 10)
                    {
                        await Task.Delay(1500); // settle delay
                        var bodyText = await page.InnerTextAsync("body");
                        var extracted = ExtractJsonBySessionId(bodyText, sessionId);
                        if (!string.IsNullOrEmpty(extracted))
                        {
                            Console.WriteLine($"[WaitResponse] ✅ Response extracted ({extracted.Length} chars).");
                            return extracted;
                        }
                        // Fallback: raw body
                        Console.WriteLine("[WaitResponse] ⚠️ No JSON found, returning raw body text.");
                        return bodyText;
                    }
                }
                catch (Exception pollEx)
                {
                    Console.WriteLine($"[WaitResponse] ⚠️ Poll error: {pollEx.Message}");
                }
                countLoop++;
            }

            // Timeout — extract whatever we have
            Console.WriteLine("[WaitResponse] ⏰ Timeout! Extracting available content...");
            var fallbackBody = await page.InnerTextAsync("body");
                Console.WriteLine($"================================== PRINT fallbackBody -========================================.");
                Console.WriteLine("");
                Console.WriteLine(fallbackBody);
                Console.WriteLine("");
                Console.WriteLine($"================================ END OFF fallbackBody -=======================================.");
            var fallbackExtracted = ExtractJsonBySessionId(fallbackBody, sessionId);
            if (!string.IsNullOrEmpty(fallbackExtracted)) return fallbackExtracted;
            if (!string.IsNullOrEmpty(fallbackBody)) return fallbackBody;

            throw new TimeoutException("Web AI did not produce a response within the timeout period.");
        }
        catch (Exception ex) when (ex is not TimeoutException)
        {
            Console.WriteLine($"[WaitResponse] ✗ Failed: {ex.Message}");
            throw;
        }
    }
    // ────────────────────────────────────────────────────────
    //  RESPONSE EXTRACTION HELPERS
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Scan body text for JSON objects containing "session_id".
    /// Returns the one matching the given sessionId, or the last found.
    /// </summary>
// ── GANTI method ExtractJsonBySessionId yang lama dengan ini ──

private static string? ExtractJsonBySessionId(string bodyText, string? sessionId)
{
    if (string.IsNullOrWhiteSpace(bodyText)) return null;

    var candidates = new List<string>();

    int searchFrom = 0;
    while (true)
    {
        int sidIdx = bodyText.IndexOf("\"session_id\"", searchFrom, StringComparison.Ordinal);
        if (sidIdx < 0) break;

        // Walk BACKWARD dari sidIdx → cari root '{' pembuka
        int rootOpen = -1;
        int depth = 0;
        for (int i = sidIdx; i >= 0; i--)
        {
            if (bodyText[i] == '}') depth++;
            else if (bodyText[i] == '{')
            {
                if (depth == 0) { rootOpen = i; break; }
                depth--;
            }
        }

        if (rootOpen >= 0)
        {
            var extracted = ExtractBalancedJson(bodyText, rootOpen);
            if (extracted != null && !candidates.Contains(extracted))
                candidates.Add(extracted);
        }

        searchFrom = sidIdx + 1;
    }

    if (candidates.Count == 0)
    {
        Console.WriteLine("[ExtractJson] ⚠️ No JSON with session_id found.");
        return null;
    }

    Console.WriteLine($"[ExtractJson] Found {candidates.Count} JSON candidate(s).");

    var realCandidates = candidates.Where(c => !IsTemplateResponse(c)).ToList();
    var pool = realCandidates.Count > 0 ? realCandidates : candidates;

    if (!string.IsNullOrEmpty(sessionId))
    {
        var match = pool.LastOrDefault(c => c.Contains(sessionId));
        if (match != null)
        {
            Console.WriteLine($"[ExtractJson] ✅ Matched session_id ({match.Length} chars).");
            return match;
        }
    }

    var last = pool.Last();
    Console.WriteLine($"[ExtractJson] ℹ️ Using last candidate ({last.Length} chars).");
    return last;
}

   /// <summary>
    /// Returns true if JSON still contains "..." placeholder (template/prompt echo).
    /// </summary>
    private static bool IsTemplateResponse(string json)
        => json.Contains("\"...\"") || json.Contains(": \"...\"");
// ── TAMBAH method baru ini (helper untuk ExtractJsonBySessionId) ──

private static string? ExtractBalancedJson(string text, int openPos)
{
    int depth = 0;
    bool inString = false;
    bool escape = false;

    for (int i = openPos; i < text.Length; i++)
    {
        char c = text[i];

        if (escape)              { escape = false; continue; }
        if (c == '\\' && inString) { escape = true; continue; }
        if (c == '"')            { inString = !inString; continue; }
        if (inString)            continue;

        if      (c == '{') depth++;
        else if (c == '}')
        {
            depth--;
            if (depth == 0)
                return text[openPos..(i + 1)];
        }
    }

    return null; // unbalanced
}
    // ────────────────────────────────────────────────────────
    //  CORE: HYBRID CURSOR
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// ScrollIntoView → BoundingBox → RandomPointInBox → HumanMoveToAsync → Mouse.ClickAsync
    /// </summary>
    private static async Task HybridMoveAndClick(IPage page, ILocator locator, string label)
    {
        await locator.ScrollIntoViewIfNeededAsync();
        await Task.Delay(_rng.Next(500, 1500));

        var box = await locator.BoundingBoxAsync();
        if (box == null)
            throw new Exception($"Could not get bounding box for '{label}'.");

        var (x, y) = RandomPointInBox(box);
        Console.WriteLine($"[{label}] Coords: ({x:F0}, {y:F0}) — box [{box.X:F0},{box.Y:F0} {box.Width:F0}x{box.Height:F0}]");

        await page.HumanMoveToAsync(x, y);
        await NaturalDelay();
        await page.Mouse.ClickAsync(x, y);
    }

    // ────────────────────────────────────────────────────────
    //  CORE: MULTI-SELECTOR FALLBACK
    // ────────────────────────────────────────────────────────

    private static async Task ExecuteWithFallback(
        IPage page,
        IList<WebAiSelector> selectors,
        string selectorType,
        Func<ILocator, WebAiSelector, Task> action)
    {
        var candidates = selectors
            .Where(s => s.SelectorType == selectorType)
            .OrderBy(s => s.SelectorIndex)
            .ToList();

        if (candidates.Count == 0)
            throw new Exception($"No selectors found for type '{selectorType}'.");

        var errors = new List<string>();

        // Primary: main page
        foreach (var candidate in candidates)
        {
            try
            {
                var locator = PlaywrightLocatorResolver.Resolve(page, candidate.LocatorStrategy, candidate.SelectorElement);
                var count = await locator.CountAsync();
                if (count == 0) { errors.Add($"'{candidate.SelectorElement}' count=0"); continue; }
                if (!await locator.IsVisibleAsync()) { errors.Add($"'{candidate.SelectorElement}' not visible"); continue; }
                await action(locator, candidate);
                return;
            }
            catch (Exception ex)
            {
                errors.Add($"'{candidate.SelectorElement}' error: {ex.Message}");
            }
        }

        // Fallback: try iframes
        if (page.Frames.Count > 1)
        {
            foreach (var frame in page.Frames)
            {
                if (frame == page.MainFrame) continue;
                foreach (var candidate in candidates)
                {
                    try
                    {
                        var locator = frame.Locator(candidate.SelectorElement);
                        if (await locator.CountAsync() > 0 && await locator.IsVisibleAsync())
                        {
                            await action(locator, candidate);
                            return;
                        }
                    }
                    catch { /* skip */ }
                }
            }
        }

        throw new Exception(
            $"All {candidates.Count} selector(s) failed for '{selectorType}':\n" +
            string.Join("\n", errors.Select((e, i) => $"  [{i + 1}] {e}")));
    }

    // ────────────────────────────────────────────────────────
    //  HELPERS
    // ────────────────────────────────────────────────────────

    private static async Task NaturalDelay()
    {
        var delayMs = _rng.Next(1500, 4000);
        Console.WriteLine($"[Delay] ⏱️ {delayMs / 1000.0:F1}s...");
        await Task.Delay(delayMs);
    }

    private static (float x, float y) RandomPointInBox(LocatorBoundingBoxResult box)
    {
        var x = (float)(box.X + box.Width  * (0.3 + _rng.NextDouble() * 0.4));
        var y = (float)(box.Y + box.Height * (0.3 + _rng.NextDouble() * 0.4));
        return (x, y);
    }

}
---

namespace DocTypeSeeder.WebAI.Selectors;

public static class DeepSeekSelectors
{
    public static IList<WebAiSelector> Get() => new List<WebAiSelector>
    {
        // ── Input-Chat ─────────────────────────────────────────────────────────
        new() { SelectorType = "Input-Chat", SelectorIndex = 1,  LocatorStrategy = "css",
            SelectorElement = "#root > div > div > div.c3ecdb44 > div._7780f2e > div > div > div._9a2f8e4 > div.aaff8b8f > div > div > div._24fad49 > textarea" },
        new() { SelectorType = "Input-Chat", SelectorIndex = 2,  LocatorStrategy = "css",
            SelectorElement = "textarea[class='_27c9245 ds-scroll-area d96f2d2a']" },
        new() { SelectorType = "Input-Chat", SelectorIndex = 3,  LocatorStrategy = "css",
            SelectorElement = "textarea[class='_27c9245']" },
        new() { SelectorType = "Input-Chat", SelectorIndex = 4,  LocatorStrategy = "css",
            SelectorElement = "textarea[class='ds-scroll-area']" },
        new() { SelectorType = "Input-Chat", SelectorIndex = 5,  LocatorStrategy = "xpath",
            SelectorElement = "//*[@id=\"root\"]/div/div/div[2]/div[3]/div/div/div[2]/div[2]/div/div/div[1]/textarea" },
        new() { SelectorType = "Input-Chat", SelectorIndex = 6,  LocatorStrategy = "placeholder",
            SelectorElement = "Pesan DeepSeek" },

        // ── Send ───────────────────────────────────────────────────────────────
        new() { SelectorType = "Send", SelectorIndex = 1, LocatorStrategy = "css",
            SelectorElement = "#root > div > div > div.c3ecdb44 > div._7780f2e > div > div > div._9a2f8e4 > div.aaff8b8f > div > div > div.ec4f5d61 > div.bf38813a > div:nth-child(3) > div" },
        new() { SelectorType = "Send", SelectorIndex = 2, LocatorStrategy = "css",
            SelectorElement = "._7436101.ds-icon-button.ds-icon-button--l.ds-icon-button--sizing-container" },
        new() { SelectorType = "Send", SelectorIndex = 3, LocatorStrategy = "xpath",
            SelectorElement = "//*[@id=\"root\"]/div/div/div[2]/div[3]/div/div/div[2]/div[2]/div/div/div[2]/div[3]/div[2]/div" },
    };
}

public static class QwenSelectors
{
    public static IList<WebAiSelector> Get() => new List<WebAiSelector>
    {
        // ── Input-Chat ─────────────────────────────────────────────────────────
        new() { SelectorType = "Input-Chat", SelectorIndex = 1, LocatorStrategy = "placeholder",
            SelectorElement = "How can I help you today?" },
        new() { SelectorType = "Input-Chat", SelectorIndex = 2, LocatorStrategy = "css",
            SelectorElement = "#dropzone-container > div.message-input > div > div.message-input-container > div > div > textarea" },
        new() { SelectorType = "Input-Chat", SelectorIndex = 3, LocatorStrategy = "xpath",
            SelectorElement = "//*[@id=\"dropzone-container\"]/div[2]/div/div[2]/div/div/textarea" },

        // ── Send ───────────────────────────────────────────────────────────────
        new() { SelectorType = "Send", SelectorIndex = 1, LocatorStrategy = "css",
            SelectorElement = "div.message-input-right-button-send > div > button" },
        new() { SelectorType = "Send", SelectorIndex = 2, LocatorStrategy = "css",
            SelectorElement = "div.message-input-container > div > div > div.message-input-right-button > div.message-input-right-button-send > div > button" },
        new() { SelectorType = "Send", SelectorIndex = 3, LocatorStrategy = "css",
            SelectorElement = "#dropzone-container > div.message-input > div > div.message-input-container > div > div > div.message-input-right-button > div.message-input-right-button-send > div > button" },
        new() { SelectorType = "Send", SelectorIndex = 4, LocatorStrategy = "css",
            SelectorElement = "button:has(use[href*='icon-line-arrow-up'])" },
        new() { SelectorType = "Send", SelectorIndex = 5, LocatorStrategy = "xpath",
            SelectorElement = "//*[@id=\"dropzone-container\"]/div[2]/div/div[2]/div/div/div[2]/div[2]/div/button" },
        new() { SelectorType = "Send", SelectorIndex = 6, LocatorStrategy = "css",
            SelectorElement = "div.chat-layout-input-container > div > div > div.message-input-container > div > div > div.message-input-right-button > div.message-input-right-button-send > div > button" },
    };
}

public static class ZAiSelectors
{
    public static IList<WebAiSelector> Get() => new List<WebAiSelector>
    {
        // ── Input-Chat ─────────────────────────────────────────────────────────
        new() { SelectorType = "Input-Chat", SelectorIndex = 1, LocatorStrategy = "css",
            SelectorElement = "#chat-input" },
        new() { SelectorType = "Input-Chat", SelectorIndex = 2, LocatorStrategy = "xpath",
            SelectorElement = "//*[@id=\"chat-input\"]" },

        // ── Send ───────────────────────────────────────────────────────────────
        new() { SelectorType = "Send", SelectorIndex = 1, LocatorStrategy = "css",
            SelectorElement = "#send-message-button" },
        new() { SelectorType = "Send", SelectorIndex = 2, LocatorStrategy = "xpath",
            SelectorElement = "//*[@id=\"send-message-button\"]" },
    };

}
---

namespace DocTypeSeeder.Prompts;

public static class AttributePrompts
{
    public static string DeepSeekSystemPrompt(string sessionId, string categoryName, string subCategoryName, string documentType) =>
        "You are a document management expert. Given a document type, generate a comprehensive list of attributes (fields/metadata) that this document typically contains.\n\n" +
        "Rules:\n" +
        "- Return ONLY valid JSON, no explanation, no markdown code blocks\n" +
        "- DataType must be one of: Text, Int, Decimal, Bool, Date\n" +
        "- Be thorough and practical for real-world document management\n\n" +
        "Example :\n"+
       "| DocumentType  | AttributeName   | Type     |\n"+
       "| ------------- | --------------- | -------- |\n"+
       "| Kontrak Kerja | Nama Karyawan   | Text     |\n"+
       "| Kontrak Kerja | Nilai Kontrak   | Decimal |\n"+
       "| Kontrak Kerja | Tanggal Mulai   | Date     |\n"+
       "| Kontrak Kerja | Tanggal Selesai | Date     |\n"+
       "...etc\n\n"+

        $"Document Information:\n" +
        $"- Category: {categoryName}\n" +
        $"- SubCategory: {subCategoryName}\n" +
        $"- DocumentType: {documentType}\n\n" +
        "Required JSON format (respond ONLY with this JSON):\n" +
        "{\n" +
        $"    \"session_id\": \"{sessionId}\",\n" +
        "    \"data\": [\n" +
        "        {\n" +
        "            \"AttributeName\": \"...\",\n" +
        "            \"DataType\": \"...\"\n" +
        "        }\n" +
        "    ]\n" +
        "}\nPlease Deep Dive bro, Ok.\n";

    public static string QwenSystemPrompt(string sessionId, string categoryName, string subCategoryName, string documentType, string deepSeekAnswer) =>
        "You are a document management expert. Review the following attribute list for a document type, validate it, add any missing important attributes, remove duplicates, and rewrite the complete list.\n\n" +
        $"Document Information:\n" +
        $"- Category: {categoryName}\n" +
        $"- SubCategory: {subCategoryName}\n" +
        $"- DocumentType: {documentType}\n\n" +
        $"Previous answer from DeepSeek (validate and improve this):\n{deepSeekAnswer}\n\n" +
        "Rules:\n" +
        "- Return ONLY valid JSON, no explanation, no markdown code blocks\n" +
        "- DataType must be one of: Text, Int, Decimal, Bool, Date\n" +
        "- Rewrite the complete final list (not just additions)\n" +
        "- Fix any mistakes from the previous answer\n\n" +
        "Example :\n"+
       "| DocumentType  | AttributeName   | Type     |\n"+
       "| ------------- | --------------- | -------- |\n"+
       "| Kontrak Kerja | Nama Karyawan   | Text     |\n"+
       "| Kontrak Kerja | Nilai Kontrak   | Decimal |\n"+
       "| Kontrak Kerja | Tanggal Mulai   | Date     |\n"+
       "| Kontrak Kerja | Tanggal Selesai | Date     |\n"+
       "...etc\n\n"+
        "Required JSON format (respond ONLY with this JSON):\n" +
        "{\n" +
        $"    \"session_id\": \"{sessionId}\",\n" +
        "    \"data\": [\n" +
        "        {\n" +
        "            \"AttributeName\": \"...\",\n" +
        "            \"DataType\": \"...\"\n" +
        "        }\n" +
        "    ]\n" +
        "}\nPlease Deep Dive bro, Ok.\n";

    public static string ZAiSystemPrompt(string sessionId, string categoryName, string subCategoryName, string documentType, string qwenAnswer) =>
        "You are a document management expert performing final validation. Review the following attribute list, ensure it is complete, accurate, and add anything critical that may have been missed. This is the FINAL answer that will be saved to the database.\n\n" +
        $"Document Information:\n" +
        $"- Category: {categoryName}\n" +
        $"- SubCategory: {subCategoryName}\n" +
        $"- DocumentType: {documentType}\n\n" +
        $"Validated answer from Qwen (review and finalize this):\n{qwenAnswer}\n\n" +
        "Rules:\n" +
        "- Return ONLY valid JSON, no explanation, no markdown code blocks\n" +
        "- DataType must be one of: Text, Int, Decimal, Bool, Date\n" +
        "- This is the FINAL list -- make it definitive and complete\n" +
        "- Rewrite the complete final list\n\n" +
        "Example :\n"+
       "| DocumentType  | AttributeName   | Type     |\n"+
       "| ------------- | --------------- | -------- |\n"+
       "| Kontrak Kerja | Nama Karyawan   | Text     |\n"+
       "| Kontrak Kerja | Nilai Kontrak   | Decimal |\n"+
       "| Kontrak Kerja | Tanggal Mulai   | Date     |\n"+
       "| Kontrak Kerja | Tanggal Selesai | Date     |\n"+
       "...etc\n\n"+
        "Required JSON format (respond ONLY with this JSON):\n" +
        "{\n" +
        $"    \"session_id\": \"{sessionId}\",\n" +
        "    \"data\": [\n" +
        "        {\n" +
        "            \"AttributeName\": \"...\",\n" +
        "            \"DataType\": \"...\"\n" +
        "        }\n" +
        "    ]\n" +
        "}\nPlease Deep Dive bro, Ok.\n";
}

public static class SynonymPrompts
{
    public static string DeepSeekSystemPrompt(string sessionId, string attributeNamesJson) =>
        "You are a multilingual document attribute expert. For each attribute name below, generate synonyms in both English (eng) and Indonesian (ind).\n\n" +
        "Rules:\n" +
        "- Return ONLY valid JSON, no explanation, no markdown code blocks\n" +
        "- Generate >50 synonyms per attribute per language\n" +
        "- Synonyms should reflect how the field might appear in real documents\n" +
        "- AttributeName in the response must exactly match the input\n\n" +
        $"Attributes:\n{attributeNamesJson}\n\n" +
        "Required JSON format (respond ONLY with this JSON):\n" +
        "{\n" +
        $"    \"session_id\": \"{sessionId}\",\n" +
        "    \"data\": [\n" +
        "        {\n" +
        "            \"AttributeName\": \"...\",\n" +
        "            \"Synonyms\": [\n" +
        "                { \"Synonym\": \"...\", \"Language\": \"eng\" },\n" +
        "                { \"Synonym\": \"...\", \"Language\": \"ind\" }\n" +
        "            ]\n" +
        "        }\n" +
        "    ]\n" +
        "}\nPlease Deep Dive Bro.\n";

    public static string QwenSystemPrompt(string sessionId, string deepSeekAnswer) =>
        "You are a multilingual document expert. Review the following attribute synonyms list, validate it, add better or missing synonyms, and rewrite the complete list.\n\n" +
        $"Previous answer from DeepSeek:\n{deepSeekAnswer}\n\n" +
        "Rules:\n" +
        "- Return ONLY valid JSON, no explanation, no markdown code blocks\n" +
        "- Language must be either \"eng\" or \"ind\"\n" +
        "- Rewrite the complete final list\n" +
        "- Ensure synonyms are realistic and useful for document field matching\n\n" +
        "Required JSON format (respond ONLY with this JSON):\n" +
        "{\n" +
        $"    \"session_id\": \"{sessionId}\",\n" +
        "    \"data\": [\n" +
        "        {\n" +
        "            \"AttributeName\": \"...\",\n" +
        "            \"Synonyms\": [\n" +
        "                { \"Synonym\": \"...\", \"Language\": \"eng\" },\n" +
        "                { \"Synonym\": \"...\", \"Language\": \"ind\" }\n" +
        "            ]\n" +
        "        }\n" +
        "    ]\n" +
        "}\n\n";

    public static string ZAiSystemPrompt(string sessionId, string qwenAnswer) =>
        "You are a multilingual document expert performing final validation. Review the following synonym list, ensure it is complete and accurate. This is the FINAL answer that will be saved to the database.\n\n" +
        $"Validated answer from Qwen:\n{qwenAnswer}\n\n" +
        "Rules:\n" +
        "- Return ONLY valid JSON, no explanation, no markdown code blocks\n" +
        "- Language must be either \"eng\" or \"ind\"\n" +
        "- This is the FINAL list -- make it definitive\n" +
        "- Rewrite the complete final list\n\n" +
        "Required JSON format (respond ONLY with this JSON):\n" +
        "{\n" +
        $"    \"session_id\": \"{sessionId}\",\n" +
        "    \"data\": [\n" +
        "        {\n" +
        "            \"AttributeName\": \"...\",\n" +
        "            \"Synonyms\": [\n" +
        "                { \"Synonym\": \"...\", \"Language\": \"eng\" },\n" +
        "                { \"Synonym\": \"...\", \"Language\": \"ind\" }\n" +
        "            ]\n" +
        "        }\n" +
        "    ]\n" +
        "}\n\n";

}
---

using Microsoft.Playwright;

namespace DocTypeSeeder.WebAI;

/// <summary>
/// Resolves a Playwright ILocator from a strategy string + selector element.
/// Strategies: css, xpath, placeholder, text, role
/// </summary>
public static class PlaywrightLocatorResolver
{
    public static ILocator Resolve(IPage page, string? strategy, string element)
    {
        var s = (strategy ?? "css").Trim().ToLowerInvariant();
        return s switch
        {
            "xpath"       => page.Locator($"xpath={element}"),
            "placeholder" => page.GetByPlaceholder(element),
            "text"        => page.GetByText(element),
            "role"        => page.GetByRole(AriaRole.Button, new() { Name = element }),
            _             => page.Locator(element) // default css
        };
    }

    public static ILocator Resolve(IFrame frame, string? strategy, string element)
    {
        var s = (strategy ?? "css").Trim().ToLowerInvariant();
        return s switch
        {
            "xpath"       => frame.Locator($"xpath={element}"),
            "placeholder" => frame.GetByPlaceholder(element),
            "text"        => frame.GetByText(element),
            _             => frame.Locator(element)
        };
    }

}
---

using System.Text.Json;
using DocTypeSeeder.Database;
using DocTypeSeeder.Models;
using DocTypeSeeder.Prompts;
using DocTypeSeeder.WebAI;
using DocTypeSeeder.WebAI.Selectors;

namespace DocTypeSeeder.Orchestration;

/// <summary>
/// Phase 1: For each TmDocumentType without attributes,
/// ask DeepSeek → Qwen → Z AI in sequence, then INSERT to TmDocumentTypeAttributes.
///
/// Flow per document type:
///   1. DeepSeek  → generate attributes
///   2. Qwen      → validate & rewrite DeepSeek's answer
///   3. Z AI      → final validation → INSERT to DB
/// </summary>
public class Phase1AttributesOrchestrator
{
    private readonly WebAiOrchestrator _webAi;
    private readonly PostgresRepository_db;
    private readonly AppSettings _settings;

    // Provider definitions
    private readonly WebAiProvider _deepSeek;
    private readonly WebAiProvider _qwen;
    private readonly WebAiProvider _zAi;

    // Selector lists per provider
    private readonly IList<WebAiSelector> _deepSeekSelectors;
    private readonly IList<WebAiSelector> _qwenSelectors;
    private readonly IList<WebAiSelector> _zAiSelectors;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Phase1AttributesOrchestrator(
        WebAiOrchestrator webAi,
        PostgresRepository db,
        AppSettings settings)
    {
        _webAi = webAi;
        _db = db;
        _settings = settings;

        _deepSeek = new WebAiProvider { WebAiName = settings.DeepSeek.Name, WebAiUrl = settings.DeepSeek.Url };
        _qwen     = new WebAiProvider { WebAiName = settings.Qwen.Name,     WebAiUrl = settings.Qwen.Url };
        _zAi      = new WebAiProvider { WebAiName = settings.ZAi.Name,      WebAiUrl = settings.ZAi.Url };

        _deepSeekSelectors = DeepSeekSelectors.Get();
        _qwenSelectors     = QwenSelectors.Get();
        _zAiSelectors      = ZAiSelectors.Get();
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        Console.WriteLine("\n╔══════════════════════════════════════════════════════╗");
        Console.WriteLine("║       PHASE 1 — Generate Document Type Attributes    ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════╝\n");

        var docTypes = await _db.GetDocumentTypesWithoutAttributesAsync();

        if (docTypes.Count == 0)
        {
            Console.WriteLine("✅ All document types already have attributes. Phase 1 complete.");
            return;
        }

        Console.WriteLine($"📋 Found {docTypes.Count} document type(s) without attributes.\n");

        int processed = 0;
        int failed = 0;

        foreach (var docType in docTypes)
        {
            ct.ThrowIfCancellationRequested();
            processed++;

            Console.WriteLine($"\n──────────────────────────────────────────────────────");
            Console.WriteLine($"[{processed}/{docTypes.Count}] {docType.CategoryName} > {docType.SubCategoryName} > {docType.DocumentType}");
            Console.WriteLine($"──────────────────────────────────────────────────────");

            try
            {
                var sessionId = docType.Id.ToString();
                List<AttributeItem>? finalAttributes = null;

                var deepSeekJson = string.Empty;
                var deepSeekRaw = "";
                AttributeAiResponse? deepSeekParsed = new();
                // ── Step 1: DeepSeek ──────────────────────────────────────
                    Console.WriteLine("\n[STEP 1] 🤖 DeepSeek — Generating attributes...");
                if(processed > 1)
                {
                    var deepSeekSystemPrompt = AttributePrompts.DeepSeekSystemPrompt(
                        sessionId, docType.CategoryName, docType.SubCategoryName, docType.DocumentType);

                    deepSeekRaw = await _webAi.AskAsync(
                        _deepSeek, _deepSeekSelectors,
                        deepSeekSystemPrompt, "Generate the attributes now.",
                        sessionId, ct);

                    deepSeekParsed = ParseAttributeResponse(deepSeekRaw, sessionId, "DeepSeek");
                    if(deepSeekParsed is null)
                    {
                        var isUnComleted = true;
                        while(isUnComleted)
                        {
                            deepSeekRaw = await _webAi.GetResponseAgainAsync(_deepSeek, _deepSeekSelectors,sessionId, ct);
                            deepSeekParsed = ParseAttributeResponse(deepSeekRaw, sessionId, "DeepSeek");
                            if(deepSeekParsed is not null) isUnComleted = false;
                        }
                    }
                    
                }
                else
                {
                    Console.WriteLine("\n[GET RESPONSE] 🤖 DeepSeek — Generating Response in Else Block...");
                        var isUnComleted = true;
                        while(isUnComleted)
                        {
                            deepSeekRaw = await _webAi.GetResponseAgainAsync(_deepSeek, _deepSeekSelectors,sessionId, ct);
                            deepSeekParsed = ParseAttributeResponse(deepSeekRaw, sessionId, "DeepSeek");
                            if(deepSeekParsed is not null) isUnComleted = false;
                        }
                    
                }
                deepSeekJson = deepSeekParsed != null
                    ? JsonSerializer.Serialize(deepSeekParsed, _jsonOptions)
                    : deepSeekRaw;

                Console.WriteLine($"[STEP 1] ✅ DeepSeek returned {deepSeekParsed?.Data.Count ?? 0} attributes.");

                // ── Step 2: Qwen ──────────────────────────────────────────
                Console.WriteLine("\n[STEP 2] 🤖 Qwen — Validating DeepSeek's answer...");
                var qwenSystemPrompt = AttributePrompts.QwenSystemPrompt(
                    sessionId, docType.CategoryName, docType.SubCategoryName, docType.DocumentType, deepSeekJson);

                var qwenRaw = await _webAi.AskAsync(
                    _qwen, _qwenSelectors,
                    qwenSystemPrompt, "Validate and rewrite the attribute list now.",
                    sessionId, ct);

                var qwenParsed = ParseAttributeResponse(qwenRaw, sessionId, "Qwen");
                if(qwenParsed is null)
                {
                    var isUnComleted = true;
                    while(isUnComleted)
                    {
                        qwenRaw = await _webAi.GetResponseAgainAsync(_qwen, _qwenSelectors,sessionId, ct);
                        qwenParsed = ParseAttributeResponse(qwenRaw, sessionId, "Qwen");
                        if(qwenParsed is not null) isUnComleted = false;
                    }
                }

                var qwenJson = qwenParsed != null
                    ? JsonSerializer.Serialize(qwenParsed, _jsonOptions)
                    : qwenRaw;

                Console.WriteLine($"[STEP 2] ✅ Qwen returned {qwenParsed?.Data.Count ?? 0} attributes.");

                // ── Step 3: Z AI (Final) ──────────────────────────────────
                Console.WriteLine("\n[STEP 3] 🤖 Z AI — Final validation...");
                var zAiSystemPrompt = AttributePrompts.ZAiSystemPrompt(
                    sessionId, docType.CategoryName, docType.SubCategoryName, docType.DocumentType, qwenJson);

                var zAiRaw = await _webAi.AskAsync(
                    _zAi, _zAiSelectors,
                    zAiSystemPrompt, "Provide the final definitive attribute list now.",
                    sessionId, ct);

                var zAiParsed = ParseAttributeResponse(zAiRaw, sessionId, "Z-AI");
                if(zAiParsed is null)
                {
                    var isUnComleted = true;
                    while(isUnComleted)
                    {
                        zAiRaw = await _webAi.GetResponseAgainAsync(_zAi, _zAiSelectors,sessionId, ct);
                        zAiParsed = ParseAttributeResponse(zAiRaw, sessionId, "Z-AI");
                        if(zAiParsed is not null) isUnComleted = false;
                    }
                }

                Console.WriteLine($"[STEP 3] ✅ Z AI returned {zAiParsed?.Data.Count ?? 0} attributes.");
                await _db.UpsertAttributesAsync(docType.Id, deepSeekParsed!.Data);
                await _db.UpsertAttributesAsync(docType.Id, qwenParsed!.Data);
                await _db.UpsertAttributesAsync(docType.Id, zAiParsed!.Data);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR] ✗ Failed for '{docType.DocumentType}': {ex.Message}");
                failed++;
            }

            // Brief pause between document types
            await Task.Delay(2000, ct);
        }

        Console.WriteLine($"\n╔══════════════════════════════════════════════════════╗");
        Console.WriteLine($"║ PHASE 1 DONE — Processed: {processed}, Failed: {failed,-3}         ║");
        Console.WriteLine($"╚══════════════════════════════════════════════════════╝\n");
    }

    // ────────────────────────────────────────────────────────
    //  HELPERS
    // ────────────────────────────────────────────────────────

    private AttributeAiResponse? ParseAttributeResponse(string raw, string sessionId, string providerName)
    {
        try
        {
            var cleanJson = CleanJson(raw);
            var parsed = JsonSerializer.Deserialize<AttributeAiResponse>(cleanJson, _jsonOptions);

            if (parsed?.Data?.Count > 0)
                return parsed;

            Console.WriteLine($"[Parse-{providerName}] ⚠️ Parsed OK but data is empty. Raw snippet: {raw[..Math.Min(200, raw.Length)]}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Parse-{providerName}] ✗ JSON parse error: {ex.Message}");
            Console.WriteLine($"[Parse-{providerName}]   Raw snippet: {raw[..Math.Min(300, raw.Length)]}");
            return null;
        }
    }

private static string CleanJson(string raw)
{
    if (string.IsNullOrWhiteSpace(raw)) return "{}";

    // Strip markdown fences
    var clean = System.Text.RegularExpressions.Regex
        .Replace(raw.Trim(), @"```json|```", "").Trim();

    // Cari '{' PERTAMA (root opener) — bukan LastIndexOf!
    int start = clean.IndexOf('{');
    if (start < 0) return clean;

    // Balanced-brace walk forward dari root '{'
    int depth = 0;
    bool inString = false;
    bool escape = false;

    for (int i = start; i < clean.Length; i++)
    {
        char c = clean[i];
        if (escape)                { escape = false; continue; }
        if (c == '\\' && inString) { escape = true;  continue; }
        if (c == '"')              { inString = !inString; continue; }
        if (inString)              continue;

        if      (c == '{') depth++;
        else if (c == '}')
        {
            depth--;
            if (depth == 0) return clean[start..(i + 1)];
        }
    }

    return clean;
}

}
---

Template Prompt 1 :

Anda adalah asisten AI khusus untuk Document Management System (DMS) yang bertugas mengklasifikasikan dokumen ke dalam taksonomi yang telah ditentukan.

## 📋 TAKSONOMI DOKUMEN (14 KATEGORI)

### 1. Legal & Contractual

#### Kontrak & Perjanjian

- Kontrak Kerja
- Perjanjian Kerjasama (PKS)
- MoU
- NDA
- Kontrak Jual-Beli
- Perjanjian Sewa
- Amandemen Kontrak
- Addendum

#### Regulatory & Compliance

- Undang-Undang (UU)
- Peraturan Pemerintah (PP)
- Peraturan Daerah (Perda)
- Surat Edaran (SE)
- SOP Legal
- Izin Usaha (SIUP)
- Izin Usaha (NIB)
- Lisensi
- Sertifikasi

#### Dokumen Hukum Litigasi

- Gugatan
- Putusan Pengadilan
- Berita Acara Pemeriksaan (BAP)
- Surat Kuasa
- Pernyataan di Bawah Tangan

### 2. Human Resources

#### Data Pribadi & Rekrutmen

- CV / Resume
- Surat Lamaran
- Ijazah
- Transkrip Nilai
- KTP
- KK
- SKCK
- Hasil Psikotes

#### Administrasi Karyawan

- Surat Pengangkatan Karyawan (SPK)
- PKWT
- PKWTT
- Surat Peringatan (SP)
- Evaluasi Kinerja
- Surat Pengunduran Diri
- Surat Keterangan Kerja

#### Penggajian & Tunjangan

- Slip Gaji (Payroll)
- BPJS Kesehatan
- BPJS Ketenagakerjaan
- Daftar Gaji
- Perhitungan PPh 21

### 3. Financial & Accounting

#### Transaksi

- Invoice
- Faktur Pajak
- Kwitansi
- Nota
- Surat Jalan
- Bukti Transfer
- Cek
- Bilyet Giro

#### Laporan Keuangan

- Laporan Laba Rugi
- Neraca
- Arus Kas
- Laporan Anggaran
- Laporan Audit
- SPT
- SSP

#### Perbankan & Investasi

- Rekening Koran
- Buku Tabungan
- Laporan Kartu Kredit
- Laporan Investasi
- Surat Berharga

### 4. Corporate Governance

#### Perencanaan & Strategi

- Business Plan
- RKAP
- Proposal Proyek
- Feasibility Study
- Rencana Strategis (Renstra)

#### Rapat & Notulensi

- Notulen Rapat
- Berita Acara RUPS
- Berita Acara Rapat Direksi
- Materi Presentasi Rapat

#### Laporan Perusahaan

- Annual Report
- Sustainability Report
- Laporan Manajemen

### 5. Project & Technical

#### Manajemen Proyek

- Project Charter
- Project Plan
- WBS
- Gantt Chart
- Laporan Progress Proyek
- Project Closure Report

#### Teknis & Spesifikasi

- Spesifikasi Teknis
- Blueprint
- Gambar CAD
- SOP Teknis
- User Manual
- Buku Pedoman

#### Kualitas & Layanan

- SLA
- Laporan QC
- Checklist Inspeksi
- Laporan Pengujian (Test Report)

#### Dokumen Konstruksi

- RAB
- Shop Drawing
- As Built Drawing
- BAST Pekerjaan
- Laporan Harian/Mingguan Proyek

### 6. Marketing & Communication

#### Materi Pemasaran

- Brosur
- Katalog Produk
- Company Profile
- Materi Iklan
- Sales Deck

#### Komunikasi Eksternal

- Press Release
- Surat Penawaran
- Artikel Blog
- Newsletter
- Media Kit

#### Riset Pasar

- Market Research Report
- Competitor Analysis
- Survey Kepuasan Pelanggan

### 7. Procurement & Supply Chain

#### Procurement Process

- Request For Proposal (RFP)
- Request For Quotation (RFQ)
- Request For Information (RFI)
- Tender Document
- Purchase Order (PO)

#### Vendor Management

- Vendor Registration Form
- Vendor Contract
- Vendor Evaluation

#### Delivery & Logistics

- Delivery Order (DO)
- Bill of Lading (B/L)
- Packing List
- Shipping Manifest

#### Inventory

- Stock Report
- Warehouse Report
- Goods Receipt Note (GRN)
- Goods Issue Note

### 8. Operations & Administration

#### Internal Memo & Correspondence

- Memo Internal
- Surat Edaran Internal
- Email Resmi

#### Operational Reports

- Laporan Operasional Harian
- Laporan Produksi

#### Administrative Documents

- Form Permintaan Barang
- Form Perjalanan Dinas
- Form Cuti
- Surat Izin Keluar/Masuk
- Surat Tugas (SPPD)

#### Asset Management

- Inventaris Asset
- Maintenance Report
- Asset Register

### 9. IT & System Documentation

#### System Architecture

- System Architecture Diagram
- Solution Architecture

#### Technical Documentation

- API Documentation
- Software Design Document (SDD)
- Technical Design Document

#### DevOps & Infrastructure

- Deployment Guide
- Runbook
- Incident Report

#### Security Documentation

- Security Policy
- Risk Assessment
- Vulnerability Report

### 10. Research & Analysis

#### Academic / Research

- Whitepaper
- Research Paper

#### Internal Analysis

- Business Analysis Report

#### Data Analysis

- Data Insight Report

#### Benchmark Study

- Industry Benchmark Report

### 11. Training & Education

#### Training Materials

- Modul Training

#### Learning Guides

- Handbook

#### Certification

- Sertifikat Training

#### Assessment

- Soal Ujian
- Lembar Penilaian

### 12. Personal / Identity Documents

#### Identity Documents

- KTP
- Paspor
- SIM

#### Civil Documents

- Akta Kelahiran
- Akta Nikah

#### Educational Documents

- Ijazah
- Sertifikat Pendidikan

#### Licenses

- Lisensi Profesi

### 13. Health, Safety & Environment (HSE)

#### Incident Report

- Laporan Kecelakaan Kerja
- Near Miss Report

#### Safety Permit

- Hot Work Permit
- Confined Space Entry Permit

#### Inspection

- Checklist Inspeksi Safety
- Laporan Pemeriksaan P3K

#### Environment

- AMDAL
- Laporan Emisi
- Waste Management Report
- Environmental Compliance

#### Health

- Medical Check-up Report
- Health Surveillance

### 14. Insurance & Risk

#### Policy

- Polis Asuransi
- Endorsement Polis

#### Claims

- Formulir Klaim
- Laporan Kerugian (Loss Report)
- Bukti Kerugian

#### Risk Management

- Risk Register
- Business Continuity Plan
- Disaster Recovery Plan

#### Compliance Audit

- Internal Audit Report
- External Audit Finding
- Corrective Action Plan

## 🎯 TUGAS ANDA

1. Analisis konten dokumen yang diberikan oleh user.
2. Cocokkan dengan taksonomi di atas untuk menentukan **DocumentType** yang paling sesuai.
3. Keluarkan output dalam format JSON dengan struktur berikut:

```json
{
  "Category": "Nama Kategori Utama",
  "SubCategory": "Nama Subkategori",
  "DocumentType": "Nama Jenis Dokumen",
  "Summary": "Summary Dokumen minimal 1 paragraph jika dokumen lebih dari 1MB dan boleh lebih dari 1 paragraph, jika kurang dari 1MB maka summary dokumen minimal 3-4 kalimat. Dan harus berupa markdown isi summary di sini karena system sudah support markdown, jika ada bentuk table, list, atau format lainnya maka harus diubah ke markdown",
  "Points": "Poin-poin penting dalam dokumen, harus berupa markdown isi poin-poin penting di sini karena system sudah support markdown, jika ada bentuk table, list, atau format lainnya maka harus diubah ke markdown"
}
