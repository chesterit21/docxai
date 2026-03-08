namespace Shuba.Worker.AI.WebAI.Selectors;

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
