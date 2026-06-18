using Microsoft.Playwright;
using PlaywrightCSharpFramework.Config;
using PlaywrightCSharpFramework.Pages;
using Serilog;
using System.ComponentModel;

namespace PlaywrightCSharpFramework.Pages;

public sealed class InventoryPage : BasePage
{
    private ILocator Title => Page.Locator(".title");
    private ILocator InventoryItems => Page.Locator(".inventory_item");
    private ILocator SortDropdown => Page.Locator(".product_sort_container");
    private ILocator CartLink => Page.Locator(".shopping_cart_link");
    private ILocator CartBadge => Page.Locator(".shopping_cart_badge");

    public InventoryPage(IPage page) : base(page) { }

    public async Task<bool> IsLoadedAsync()
    {
        await Title.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        return await GetTitleAsync() == "Products";
    }

    public Task<string> GetTitleAsync() => Title.InnerTextAsync();

    public Task<int> ProductCountAsync() => InventoryItems.CountAsync();

    // --- Item enumeration -------------------------------------------------

    public async Task<List<string>> GetItemNamesAsync()
    {
        var count = await InventoryItems.CountAsync();
        var names = new List<string>(count);

        for (var i = 0; i < count; i++)
        {
            var nameEl = InventoryItems.Nth(i).Locator(".inventory_item_name");
            names.Add(await nameEl.InnerTextAsync());
        }

        return names;
    }

    public async Task<List<decimal>> GetItemPricesAsync()
    {
        var count = await InventoryItems.CountAsync();
        var prices = new List<decimal>(count);

        for (var i = 0; i < count; i++)
        {
            var priceEl = InventoryItems.Nth(i).Locator(".inventory_item_price");
            var txt = (await priceEl.InnerTextAsync()).Replace("$", "").Trim();

            if (decimal.TryParse(txt, out var val))
                prices.Add(val);
        }

        return prices;
    }

    // --- Cart -----------------------------------------------------------

    public async Task<int> GetCartBadgeCountAsync()
    {
        try
        {
            if (await CartBadge.CountAsync() > 0 && await CartBadge.IsVisibleAsync())
            {
                var text = await CartBadge.InnerTextAsync();
                if (int.TryParse(text, out var count))
                    return count;
            }
        }
        catch
        {
            // ignore parse / element errors, mirrors Selenium behavior
        }

        return 0;
    }

    public async Task AddToCartByNameAsync(string itemName)
    {
        Log.Debug("  Step: Adding item to cart: {ItemName}", itemName);

        var item = InventoryItems.Filter(new() { Has = Page.Locator(".inventory_item_name", new() { HasTextString = itemName }) });
        var btn = item.Locator("button");

        var btnTextBefore = await btn.InnerTextAsync();
        var btnClass = await btn.GetAttributeAsync("class");
        Log.Debug("  Step: Button before click: text='{ButtonText}', class='{ButtonClass}'", btnTextBefore, btnClass);

        await btn.ClickAsync();

        var btnTextAfter = await btn.InnerTextAsync();
        Log.Debug("  Step: Button after click: text='{ButtonText}'", btnTextAfter);

        await Page.WaitForFunctionAsync(
            "btn => btn.textContent.trim() === 'Remove'",
            await btn.ElementHandleAsync(),
            //new() { Timeout = new SettingsLoader.Load().ActionTimeoutMilliseconds });
    }

    public async Task RemoveFromCartByNameAsync(string itemName)
    {
        Log.Debug("  Step: Removing item from cart: {ItemName}", itemName);

        var item = InventoryItems.Filter(new() { Has = Page.Locator(".inventory_item_name", new() { HasTextString = itemName }) });
        var btn = item.Locator("button");

        await btn.ClickAsync();

        await Page.WaitForFunctionAsync(
            "btn => btn.textContent.trim() === 'Add to cart'",
            await btn.ElementHandleAsync(),
            //new() { Timeout = new TestSettings().ExplicitWaitSeconds * 1000 });
    }

    // --- Sorting ----------------------------------------------------------

    public async Task SortByNameAscAsync()
    {
        Log.Debug("  Step: Sorting by name (A to Z)");
        await SortDropdown.SelectOptionAsync(new[] { "az" });
    }

    public async Task SortByNameDescAsync()
    {
        Log.Debug("  Step: Sorting by name (Z to A)");
        await SortDropdown.SelectOptionAsync(new[] { "za" });
    }

    public async Task SortByPriceLowToHighAsync()
    {
        Log.Debug("  Step: Sorting by price (low to high)");
        await SortDropdown.SelectOptionAsync(new[] { "lohi" });
    }

    public async Task SortByPriceHighToLowAsync()
    {
        Log.Debug("  Step: Sorting by price (high to low)");
        await SortDropdown.SelectOptionAsync(new[] { "hilo" });
    }

    // --- Navigation -------------------------------------------------------

    public async Task<CartPage> GoToCartAsync()
    {
        Log.Debug("  Step: Navigating to Cart page");
        await CartLink.ClickAsync();
        await Page.WaitForURLAsync("**/cart.html");

        return new CartPage(Page);
    }
}