namespace Shuba.Worker.AI.WebAI;

public class WebAiSelector
{
    public string SelectorType { get; set; } = string.Empty;       // Input-Chat, Send, Response, Clear-Chat
    public string SelectorElement { get; set; } = string.Empty;    // actual selector value
    public string? LocatorStrategy { get; set; } = "css";          // css, xpath, placeholder
    public int SelectorIndex { get; set; } = 0;                    // priority order
}
