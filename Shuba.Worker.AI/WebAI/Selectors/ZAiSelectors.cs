namespace Shuba.Worker.AI.WebAI.Selectors;

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

        // ── Button-Upload (📎 langsung buka FileChooser, no menu) ──────────
        new() { SelectorType = "Button-Upload", SelectorIndex = 1, LocatorStrategy = "css",
            SelectorElement = "button#upload-file-button" },
        new() { SelectorType = "Button-Upload", SelectorIndex = 2, LocatorStrategy = "css",
            SelectorElement = "button[id='upload-file-button']" },
        new() { SelectorType = "Button-Upload", SelectorIndex = 3, LocatorStrategy = "css",
            SelectorElement = "button[aria-label='More']:has(svg path[d^='M10.0001'])" },
        new() { SelectorType = "Button-Upload", SelectorIndex = 4, LocatorStrategy = "css",
            SelectorElement = "button:has(svg path[d^='M10.0001 4.16675V15.8334'])" },
        new() { SelectorType = "Button-Upload", SelectorIndex = 5, LocatorStrategy = "xpath",
            SelectorElement = "//*[@id='upload-file-button']" },
        new() { SelectorType = "Button-Upload", SelectorIndex = 6, LocatorStrategy = "xpath",
            SelectorElement = "//button[@aria-label='More' and @id='upload-file-button']" },

        // ── File-Input (fallback hidden input) ─────────────────────────────
        new() { SelectorType = "File-Input", SelectorIndex = 1, LocatorStrategy = "css",
            SelectorElement = "input[type='file']" },
        new() { SelectorType = "File-Input", SelectorIndex = 2, LocatorStrategy = "xpath",
            SelectorElement = "//input[@type='file']" },
    };
}
