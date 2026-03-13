using Microsoft.Playwright;
using HumanCursor.Playwright;

namespace Shuba.Worker.AI.WebAI;

/// <summary>
/// Browser automation core for Web AI providers.
/// Ported from SFCore.AI.WebAI.Core.SFCoreWebAIBrowser.
/// </summary>
public static class SFCoreWebAIBrowser
{
    private static readonly Random _rng = new();

    // ────────────────────────────────────────────────────────
    //  PUBLIC API
    // ────────────────────────────────────────────────────────

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
            await Task.Delay(7500);
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

    public static async Task FocusInput(IPage page, IList<WebAiSelector> selectors, string selectorType = "Input-Chat")
    {
        Console.WriteLine($"[FocusInput] 📝 Focusing input ({selectorType})...");
        await ExecuteWithFallback(page, selectors, selectorType, async (locator, sel) =>
        {
            await HybridMoveAndClick(page, locator, "Input");
        });
        await NaturalDelay();
    }

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

    public static async Task TypeHumanLike(IPage page, string text)
    {
        Console.WriteLine($"[TypeHumanLike] ⌨️ Typing {text.Length} chars...");
        await page.Keyboard.TypeAsync(text, new KeyboardTypeOptions { Delay = _rng.Next(60, 120) });
        await NaturalDelay();
    }

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

    public static async Task PasteInput(IPage page, string text)
    {
        Console.WriteLine($"[PasteInput] 📋 Pasting {text.Length} chars...");
        await page.Keyboard.InsertTextAsync(text);
        await NaturalDelay();
    }

    public static async Task ClickSend(IPage page, IList<WebAiSelector> selectors, string selectorType = "Send")
    {
        Console.WriteLine($"[ClickSend] 🖱️ Looking for send button...");
        await ExecuteWithFallback(page, selectors, selectorType, async (locator, sel) =>
        {
            await HybridMoveAndClick(page, locator, "Send");
        });
        Console.WriteLine("[ClickSend] ✅ Clicked!");
    }

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
    //  UPLOAD FILE
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Upload file ke Web AI — route berdasarkan provider name:
    /// - DeepSeek: HumanCursor klik button upload (📎) → FileChooser langsung terbuka
    /// - Qwen: HumanCursor klik "+" → menu muncul → HumanCursor klik "Upload attachment" → FileChooser
    /// - ZAI: HumanCursor klik button#upload-file-button → FileChooser langsung terbuka
    /// </summary>
    public static async Task UploadFile(IPage page, IList<WebAiSelector> selectors, string filePath, string providerName)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        // Cek apakah perlu rename (file programming yang mungkin di-block)
        string finalFilePath = filePath;
        bool isRenamed = false;

        if (FileTypeHelper.NeedsRename(filePath))
        {
            Console.WriteLine($"[Upload-{providerName}] 🔄 Programming file detected, creating renamed copy...");
            finalFilePath = FileTypeHelper.CreateRenamedCopy(filePath);
            isRenamed = true;
        }

        Console.WriteLine($"[Upload-{providerName}] 📎 Uploading: {Path.GetFileName(finalFilePath)}");

        try
        {
            // Route berdasarkan provider name
            if (providerName.Contains("Qwen", StringComparison.OrdinalIgnoreCase))
            {
                // Qwen: klik "+" → menu → klik "Upload attachment" → FileChooser
                await UploadQwen(page, selectors, finalFilePath);
                await Task.Delay(30_000);
            }
            else
            {
                // DeepSeek & ZAI: klik button upload → langsung FileChooser
                await UploadDirectButton(page, selectors, finalFilePath, providerName);
                if (providerName.Contains("DeepSeek", StringComparison.OrdinalIgnoreCase)) await Task.Delay(50_000);
                else await Task.Delay(30_000);
            }

            Console.WriteLine($"[Upload-{providerName}] ✅ File uploaded: {Path.GetFileName(finalFilePath)}");
            await NaturalDelay();
        }
        finally
        {
            if (isRenamed && finalFilePath != filePath)
            {
                FileTypeHelper.CleanupRenamedFile(finalFilePath);
            }
        }
    }

    /// <summary>
    /// DeepSeek & ZAI: HumanCursor klik button upload → FileChooser langsung terbuka → set file.
    /// </summary>
    private static async Task UploadDirectButton(
        IPage page, IList<WebAiSelector> selectors, string filePath, string providerName)
    {
        Console.WriteLine($"[Upload-{providerName}] 📤 HumanCursor → klik upload button → FileChooser...");

        var candidates = selectors
            .Where(s => s.SelectorType == "Button-Upload")
            .OrderBy(s => s.SelectorIndex)
            .ToList();

        foreach (var candidate in candidates)
        {
            try
            {
                var locator = PlaywrightLocatorResolver.Resolve(page, candidate.LocatorStrategy, candidate.SelectorElement);
                var count = await locator.CountAsync();
                if (count == 0) continue;
                if (!await locator.IsVisibleAsync()) continue;

                // HumanCursor klik button dan tunggu FileChooser
                var fileChooser = await page.RunAndWaitForFileChooserAsync(async () =>
                {
                    await HybridMoveAndClick(page, locator, $"{providerName}-Upload");
                }, new PageRunAndWaitForFileChooserOptions { Timeout = 10_000 });

                await Task.Delay(1500);
                await fileChooser.SetFilesAsync(filePath);
                await Task.Delay(2000);

                Console.WriteLine($"[Upload-{providerName}] ✅ File set via FileChooser.");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Upload-{providerName}] ⏭️ Failed with '{candidate.SelectorElement}': {ex.Message}");
            }
        }

        // Fallback: hidden input
        await UploadViaHiddenInput(page, selectors, filePath);
    }

    /// <summary>
    /// Qwen: HumanCursor klik "+" (mode-select-open) → menu ant-dropdown muncul
    ///       → HumanCursor klik "Upload attachment" (li[data-menu-id*='upload'])
    ///       → set file via input#filesUpload
    /// </summary>
    private static async Task UploadQwen(
        IPage page, IList<WebAiSelector> selectors, string filePath)
    {
        Console.WriteLine("[Upload-Qwen] 📤 HumanCursor → klik '+' → menu → 'Upload attachment'...");

        // Step 1: HumanCursor klik tombol "+" (Button-Plus)
        var plusButtons = selectors
            .Where(s => s.SelectorType == "Button-Plus")
            .OrderBy(s => s.SelectorIndex)
            .ToList();

        bool menuOpened = false;
        foreach (var btn in plusButtons)
        {
            try
            {
                var locator = PlaywrightLocatorResolver.Resolve(page, btn.LocatorStrategy, btn.SelectorElement);
                if (await locator.CountAsync() > 0 && await locator.IsVisibleAsync())
                {
                    await HybridMoveAndClick(page, locator, "Qwen-Plus");
                    await Task.Delay(2000); // tunggu menu ant-dropdown muncul
                    menuOpened = true;
                    Console.WriteLine("[Upload-Qwen] ✅ '+' clicked, menu opened.");
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Upload-Qwen] ⏭️ Plus button failed: {ex.Message}");
            }
        }

        if (!menuOpened)
            throw new Exception("[Upload-Qwen] Could not find or click '+' button.");

        // Step 2: HumanCursor klik menu item "Upload attachment"
        var menuItems = selectors
            .Where(s => s.SelectorType == "Menu-Upload")
            .OrderBy(s => s.SelectorIndex)
            .ToList();

        foreach (var item in menuItems)
        {
            try
            {
                var locator = PlaywrightLocatorResolver.Resolve(page, item.LocatorStrategy, item.SelectorElement);
                if (await locator.CountAsync() > 0 && await locator.IsVisibleAsync())
                {
                    // Klik menu item — Qwen punya input#filesUpload yang hidden,
                    // jadi setelah klik menu "Upload attachment", FileChooser harus terbuka
                    try
                    {
                        var fileChooser = await page.RunAndWaitForFileChooserAsync(async () =>
                        {
                            await HybridMoveAndClick(page, locator, "Qwen-Upload-Menu");
                        }, new PageRunAndWaitForFileChooserOptions { Timeout = 10_000 });

                        await Task.Delay(1500);
                        await fileChooser.SetFilesAsync(filePath);
                        await Task.Delay(2000);
                        Console.WriteLine("[Upload-Qwen] ✅ File set via FileChooser.");
                        return;
                    }
                    catch (TimeoutException)
                    {
                        // Fallback: Qwen mungkin set file via hidden input#filesUpload langsung
                        Console.WriteLine("[Upload-Qwen] ⚠️ FileChooser timeout, trying input#filesUpload...");
                        await HybridMoveAndClick(page, locator, "Qwen-Upload-Menu");
                        await Task.Delay(1500);
                        await UploadViaHiddenInput(page, selectors, filePath);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Upload-Qwen] ⏭️ Menu item failed: {ex.Message}");
            }
        }

        // Fallback: langsung set di hidden input
        Console.WriteLine("[Upload-Qwen] ⚠️ Menu items all failed, trying hidden input...");
        await UploadViaHiddenInput(page, selectors, filePath);
    }



    /// <summary>
    /// Fallback: set file langsung via hidden input[type=file].
    /// </summary>
    private static async Task UploadViaHiddenInput(
        IPage page, IList<WebAiSelector> selectors, string filePath)
    {
        Console.WriteLine("[Upload-Fallback] 🔍 Trying hidden input[type=file]...");

        var fileInputs = selectors
            .Where(s => s.SelectorType == "File-Input")
            .OrderBy(s => s.SelectorIndex)
            .ToList();

        // Jika tidak ada File-Input selector, coba default
        if (fileInputs.Count == 0)
        {
            fileInputs.Add(new WebAiSelector
            {
                SelectorType = "File-Input",
                SelectorIndex = 1,
                LocatorStrategy = "css",
                SelectorElement = "input[type='file']"
            });
        }

        foreach (var fi in fileInputs)
        {
            try
            {
                var locator = PlaywrightLocatorResolver.Resolve(page, fi.LocatorStrategy, fi.SelectorElement);
                if (await locator.CountAsync() > 0)
                {
                    await locator.SetInputFilesAsync(filePath);
                    await Task.Delay(2000);
                    Console.WriteLine("[Upload-Fallback] ✅ File set via hidden input.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Upload-Fallback] ⏭️ Failed: {ex.Message}");
            }
        }

        throw new Exception("All upload methods failed. Could not upload file.");
    }

    // ────────────────────────────────────────────────────────
    //  SEND MESSAGE (combined)
    // ────────────────────────────────────────────────────────

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

    public static async Task<string> WaitAndExtractResponse(
        IPage page,
        string webAiName,
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
            var stopSelectors = selectors
                .Where(s => s.SelectorType == "Stop-Button")
                .OrderBy(s => s.SelectorIndex)
                .ToList();
            var inputSelectors = selectors
                .Where(s => s.SelectorType == "Input-Chat")
                .OrderBy(s => s.SelectorIndex)
                .ToList();
            var isRunning = true;

            Console.WriteLine("[WaitResponse] ⏳ Waiting for AI to finish generating...");
            await NaturalDelay();

            int pollCount = 0;
            await Task.Delay(20_000);
            while (isRunning)
            {
                pollCount++;

                try
                {
                    if (webAiName.ToLower().Contains("qwen", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var sel in stopSelectors)
                        {
                            try
                            {
                                var btn = PlaywrightLocatorResolver.Resolve(page, sel.LocatorStrategy, sel.SelectorElement);
                                if (await btn.CountAsync() == 0)
                                {

                                    isRunning = false;
                                    break;
                                }
                                await NaturalDelay();
                            }
                            catch { continue; }
                        }
                        foreach (var sel in sendSelectors)
                        {
                            try
                            {
                                var btn = PlaywrightLocatorResolver.Resolve(page, sel.LocatorStrategy, sel.SelectorElement);
                                if (await btn.CountAsync() > 0)
                                {
                                    var classAttr = await btn.GetAttributeAsync("class");
                                    var hasClassStopButton = classAttr?.Contains("stop-button") ?? true;
                                    if (hasClassStopButton)
                                    {
                                        isRunning = false;
                                        break;
                                    }
                                    await Task.Delay(1_000);
                                    var isDisabled = await btn.IsDisabledAsync();
                                    if (isDisabled)
                                    {
                                        isRunning = false;
                                        break;
                                    }

                                    await Task.Delay(1_000);
                                    classAttr = await btn.GetAttributeAsync("class");
                                    var hasDisabledClass = classAttr?.Split(' ').Contains("disabled") ?? true;
                                    if (hasDisabledClass)
                                    {
                                        isRunning = false;
                                        break;
                                    }
                                }
                                await NaturalDelay();
                            }
                            catch { continue; }
                        }

                    }
                    else
                    {

                        await FocusInput(page, inputSelectors);
                        await TypeHumanLikeWithNewlines(page, "terus kalau semisal nya,");
                        await Task.Delay(1_000);
                        Console.WriteLine($"[WaitResponse - {webAiName}] Check Button.");

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
                                Console.WriteLine($"[Button Send - {webAiName}] ✅ - AI Still Writing...");
                                await Task.Delay(1_000);
                            }
                            catch { continue; }
                        }

                        var random = new Random();
                        for (int i = 0; i < "terus kalau semisal nya,".Length; i++)
                        {
                            await page.Keyboard.PressAsync("Backspace");
                            await Task.Delay(random.Next(60, 150));
                        }
                    }
                }
                catch (Exception pollEx)
                {
                    Console.WriteLine($"[WaitResponse] ⚠️ Poll error: {pollEx.Message}");
                }
                await NaturalDelay();
            }

            Console.WriteLine("[WaitResponse] ⏰ Timeout! Extracting available content...");
            var fallbackBody = await page.InnerTextAsync("body");
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

    private static string? ExtractJsonBySessionId(string bodyText, string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(bodyText)) return null;

        var candidates = new List<string>();

        int searchFrom = 0;
        while (true)
        {
            // Look for JSON that contains Category (from our prompt template)
            int sidIdx = bodyText.IndexOf("\"Category\"", searchFrom, StringComparison.Ordinal);
            if (sidIdx < 0)
            {
                // Fallback: look for session_id
                sidIdx = bodyText.IndexOf("\"session_id\"", searchFrom, StringComparison.Ordinal);
                if (sidIdx < 0) break;
            }

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
            Console.WriteLine("[ExtractJson] ⚠️ No JSON with Category/session_id found.");
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

    private static bool IsTemplateResponse(string json)
        => json.Contains("\"...\"") || json.Contains(": \"...\"");

    private static string? ExtractBalancedJson(string text, int openPos)
    {
        int depth = 0;
        bool inString = false;
        bool escape = false;

        for (int i = openPos; i < text.Length; i++)
        {
            char c = text[i];

            if (escape) { escape = false; continue; }
            if (c == '\\' && inString) { escape = true; continue; }
            if (c == '"') { inString = !inString; continue; }
            if (inString) continue;

            if (c == '{') depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                    return text[openPos..(i + 1)];
            }
        }

        return null;
    }

    // ────────────────────────────────────────────────────────
    //  CORE: HYBRID CURSOR
    // ────────────────────────────────────────────────────────

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
        var x = (float)(box.X + box.Width * (0.3 + _rng.NextDouble() * 0.4));
        var y = (float)(box.Y + box.Height * (0.3 + _rng.NextDouble() * 0.4));
        return (x, y);
    }
}
