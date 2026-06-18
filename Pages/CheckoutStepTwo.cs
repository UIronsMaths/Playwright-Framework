using Microsoft.Playwright;
using PlaywrightCSharpFramework.Pages;
using Serilog;

namespace PlaywrightCSharpFramework.Pages;

public sealed class CheckoutStepTwoPage : BasePage
{
    private ILocator FinishButton => Page.Locator("#finish");
    private ILocator CancelButton => Page.Locator("#cancel");
    private ILocator Title => Page.Locator(".title");

    public CheckoutStepTwoPage(IPage page) : base(page) { }

    public async Task<bool> IsDisplayedAsync() => await Title.IsVisibleAsync();

    public async Task<CheckoutCompletePage> FinishAsync()
    {
        Log.Debug("  Step: Finishing checkout (step two)");
        await FinishButton.ClickAsync();
        await Page.WaitForURLAsync("**/checkout-complete.html");

        return new CheckoutCompletePage(Page);
    }

    public async Task<CartPage> CancelAsync()
    {
        Log.Debug("  Step: Cancelling checkout at step two");
        await CancelButton.ClickAsync();
        await Page.WaitForURLAsync("**/cart.html");

        return new CartPage(Page);
    }
}