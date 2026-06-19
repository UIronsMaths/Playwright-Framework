using Microsoft.Playwright;
using PlaywrightCSharpFramework.Pages;
using Serilog;

namespace PlaywrightCSharpFramework.Pages;

public sealed class CartPage : BasePage
{
    private ILocator Title => Page.Locator(".title");
    private ILocator CartItems => Page.Locator(".cart_item");
    private ILocator CheckoutButton => Page.Locator("#checkout");
    private ILocator ContinueShoppingButton => Page.Locator("#continue-shopping");

    public CartPage(IPage page) : base(page) { }

    public async Task<bool> IsDisplayedAsync() => await Title.IsVisibleAsync();

    public Task<string> GetTitleAsync() => Title.InnerTextAsync();

    public Task<int> GetItemCountAsync() => CartItems.CountAsync();

    public async Task<CheckoutStepOnePage> ProceedToCheckoutAsync()
    {
        //Log.Debug("  Step: Proceeding to checkout");
        await CheckoutButton.ClickAsync();
        await Page.WaitForURLAsync("**/checkout-step-one.html");

        return new CheckoutStepOnePage(Page);
    }

    public async Task<InventoryPage> ContinueShoppingAsync()
    {
        //Log.Debug("  Step: Continuing shopping from cart");
        await ContinueShoppingButton.ClickAsync();
        await Page.WaitForURLAsync("**/inventory.html");

        return new InventoryPage(Page);
    }
}