using Microsoft.Playwright;

namespace PlaywrightCSharpFramework.Pages;

public abstract class BasePage
{
    protected readonly IPage Page;

    protected BasePage(IPage page) => Page = page;

    protected async Task NavigateToAsync(string url)
    {
        await Page.GotoAsync(url);
    }

    public Task<string> GetPageTitleAsync() => Page.TitleAsync();

    public string GetCurrentUrl() => Page.Url;
}