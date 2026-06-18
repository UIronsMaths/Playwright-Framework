using Microsoft.Playwright;
using PlaywrightCSharpFramework.Pages;
using Serilog;

namespace PlaywrightCSharpFramework.Pages;

public sealed class CheckoutCompletePage : BasePage
{
    private ILocator Title => Page.Locator(".title");
    private ILocator BackHomeButton => Page.Locator("#back-to-products");

    public CheckoutCompletePage(IPage page) : base(page) { }

    public async Task<bool> IsDisplayedAsync() => await Title.IsVisibleAsync();

    public async Task<InventoryPage> BackHomeAsync()
    {
        Log.Debug(" Clicking Back Home from checkout complete");
        await BackHomeButton.ClickAsync();
        await Page.WaitForURLAsync("**/inventory.html");

        return new InventoryPage(Page);
    }
}