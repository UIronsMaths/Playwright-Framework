using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using PlaywrightCSharpFramework.Config;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;

public static class WaitUtils
{
    private static int DefaultTimeoutMs => SettingsLoader.Load().ActionTimeoutMilliseconds;

    public static async Task<ILocator> WaitForVisibleAsync(IPage page, string selector, int? timeoutMs = null)
    {
        var locator = page.Locator(selector);
        await locator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs ?? DefaultTimeoutMs
        });
        return locator;
    }

    public static async Task<ILocator> WaitForHiddenAsync(IPage page, string selector, int? timeoutMs = null)
    {
        var locator = page.Locator(selector);
        await locator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = timeoutMs ?? DefaultTimeoutMs
        });
        return locator;
    }

    public static async Task WaitForUrlContainingAsync(IPage page, string partialUrl, int? timeoutMs = null)
    {
        await page.WaitForURLAsync(new Regex(Regex.Escape(partialUrl), RegexOptions.IgnoreCase),
            new PageWaitForURLOptions { Timeout = timeoutMs ?? DefaultTimeoutMs });
    }

    public static async Task WaitForTitleContainingAsync(IPage page, string partialTitle, int? timeoutMs = null)
    {
        await Expect(page).ToHaveTitleAsync(new Regex(Regex.Escape(partialTitle), RegexOptions.IgnoreCase),
            new PageAssertionsToHaveTitleOptions { Timeout = timeoutMs ?? DefaultTimeoutMs });
    }

    public static async Task<ILocator> WaitForTextAsync(IPage page, string selector, string text, int? timeoutMs = null)
    {
        var locator = page.Locator(selector);
        await Expect(locator).ToHaveTextAsync(text, new LocatorAssertionsToHaveTextOptions
        {
            Timeout = timeoutMs ?? DefaultTimeoutMs
        });
        return locator;
    }
}