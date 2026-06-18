using Microsoft.Playwright;
using PlaywrightCSharpFramework.Pages;
using Serilog;

namespace PlaywrightCSharpFramework.Pages;

public sealed class CheckoutStepOnePage : BasePage
{
    private ILocator FirstName => Page.Locator("#first-name");
    private ILocator LastName => Page.Locator("#last-name");
    private ILocator PostalCode => Page.Locator("#postal-code");
    private ILocator ContinueButton => Page.Locator("#continue");
    private ILocator CancelButton => Page.Locator("#cancel");
    private ILocator Title => Page.Locator(".title");

    public CheckoutStepOnePage(IPage page) : base(page) { }

    public async Task<bool> IsDisplayedAsync() => await Title.IsVisibleAsync();

    public async Task<CheckoutStepTwoPage> ContinueAsync(string firstName, string lastName, string postalCode)
    {
        Log.Debug("  Step: Filling checkout-step-one: {FirstName} {LastName} {PostalCode}", firstName, lastName, postalCode);

        await FirstName.FillAsync(firstName);
        await LastName.FillAsync(lastName);
        await PostalCode.FillAsync(postalCode);
        await ContinueButton.ClickAsync();

        await Page.WaitForURLAsync("**/checkout-step-two.html");

        return new CheckoutStepTwoPage(Page);
    }

    public async Task<CartPage> CancelAsync()
    {
        Log.Debug("  Step: Cancelling checkout at step one");
        await CancelButton.ClickAsync();
        await Page.WaitForURLAsync("**/cart.html");

        return new CartPage(Page);
    }
}