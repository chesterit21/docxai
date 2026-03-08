namespace Shuba.Worker.AI.WebAI.Selectors;

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
