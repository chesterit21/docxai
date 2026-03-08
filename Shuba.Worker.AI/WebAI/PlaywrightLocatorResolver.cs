using Microsoft.Playwright;

namespace Shuba.Worker.AI.WebAI;

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
