using Microsoft.Playwright;
using PlaywrightCSharpFramework.Pages;

public sealed class LoginPage : BasePage
{
    private ILocator Username => Page.Locator("[data-test=\"username\"]");
    private ILocator Password => Page.Locator("[data-test=\"password\"]");
    private ILocator LoginButton => Page.Locator("[data-test=\"login-button\"]");
    private ILocator ErrorMessage => Page.Locator("[data-test=\"error\"]");

    public LoginPage(IPage page) : base(page) { }

    public Task OpenAsync(string baseUrl) => NavigateToAsync(baseUrl);

    public async Task LoginAsync(string username, string password)
    {
        await Username.FillAsync(username);
        await Password.FillAsync(password);
        await LoginButton.ClickAsync();
    }

    public Task<string> ErrorTextAsync() => ErrorMessage.InnerTextAsync();
}
