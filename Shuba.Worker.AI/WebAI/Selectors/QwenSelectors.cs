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

        // ── Button-Plus (tombol "+" → span.ant-dropdown-trigger > div.mode-select-open) ──
        new() { SelectorType = "Button-Plus", SelectorIndex = 1, LocatorStrategy = "css",
            SelectorElement = "span.ant-dropdown-trigger:has(> .mode-select-open)" },
        new() { SelectorType = "Button-Plus", SelectorIndex = 2, LocatorStrategy = "css",
            SelectorElement = "div.mode-select-open:has(use[href*='plus'])" },
        new() { SelectorType = "Button-Plus", SelectorIndex = 3, LocatorStrategy = "css",
            SelectorElement = "span.ant-dropdown-trigger:has(use[href*='icon-line-plus-01'])" },
        new() { SelectorType = "Button-Plus", SelectorIndex = 4, LocatorStrategy = "css",
            SelectorElement = "div.mode-select-open" },
        new() { SelectorType = "Button-Plus", SelectorIndex = 5, LocatorStrategy = "xpath",
            SelectorElement = "//span[contains(@class,'ant-dropdown-trigger')]//div[contains(@class,'mode-select-open')]" },

        // ── Menu-Upload ("Upload attachment" → li[data-menu-id*='upload']) ──
        new() { SelectorType = "Menu-Upload", SelectorIndex = 1, LocatorStrategy = "css",
            SelectorElement = "li[data-menu-id*='upload']" },
        new() { SelectorType = "Menu-Upload", SelectorIndex = 2, LocatorStrategy = "css",
            SelectorElement = "li.ant-dropdown-menu-item:has(use[href*='icon-line-upload'])" },
        new() { SelectorType = "Menu-Upload", SelectorIndex = 3, LocatorStrategy = "css",
            SelectorElement = "li.ant-dropdown-menu-item:has-text('Upload attachment')" },
        new() { SelectorType = "Menu-Upload", SelectorIndex = 4, LocatorStrategy = "css",
            SelectorElement = "li.ant-dropdown-menu-item:has(.mode-select-dropdown-item-content span:has-text('Upload'))" },
        new() { SelectorType = "Menu-Upload", SelectorIndex = 5, LocatorStrategy = "text",
            SelectorElement = "Upload attachment" },
        new() { SelectorType = "Menu-Upload", SelectorIndex = 6, LocatorStrategy = "xpath",
            SelectorElement = "//li[contains(@data-menu-id,'upload')]" },

        // ── File-Input (hidden input#filesUpload muncul setelah klik "+") ──
        new() { SelectorType = "File-Input", SelectorIndex = 1, LocatorStrategy = "css",
            SelectorElement = "input#filesUpload" },
        new() { SelectorType = "File-Input", SelectorIndex = 2, LocatorStrategy = "css",
            SelectorElement = "div.mode-select input[type='file']" },
        new() { SelectorType = "File-Input", SelectorIndex = 3, LocatorStrategy = "css",
            SelectorElement = "input[type='file']" },
        new() { SelectorType = "File-Input", SelectorIndex = 4, LocatorStrategy = "xpath",
            SelectorElement = "//*[@id='filesUpload']" },
    };
}
