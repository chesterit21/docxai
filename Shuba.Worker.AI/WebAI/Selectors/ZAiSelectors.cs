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
    };
}
