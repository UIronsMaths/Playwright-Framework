using Microsoft.Playwright;
using PlaywrightCSharpFramework.Pages;

public sealed class InventoryPage : BasePage
{
    private ILocator Title => Page.Locator(".title");
    private ILocator InventoryItems => Page.Locator(".inventory_item");

    public InventoryPage(IPage page) : base(page) { }

    public async Task<bool> IsLoadedAsync()
    {
        await Title.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        return await GetTitleAsync() == "Products";
    }

    public Task<string> GetTitleAsync() => Title.InnerTextAsync();

    public Task<int> ProductCountAsync() => InventoryItems.CountAsync();
}
