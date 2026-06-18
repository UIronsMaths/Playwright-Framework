using NUnit.Framework;

namespace PlaywrightCSharpFramework.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class InventoryTests : BaseTest
{
    [Test]
    public async Task InventoryPageShowsItems()
    {
        LogStep("Starting Inventory page test");

        var loginPage = new LoginPage(Page);
        LogStep("Logging in with valid credentials");
        await loginPage.LoginAsAsync(settings.Username, settings.Password);
        var inventoryPage = new InventoryPage(Page);

        LogStep("Verifying inventory page is displayed and contains items");

        Assert.Multiple(async () =>
        {
            Assert.That(Page.Url, Does.Contain("inventory.html"));
            Assert.That(await inventoryPage.IsLoadedAsync(), Is.True, "Inventory page should be visible after login");

            var count = await inventoryPage.ProductCountAsync();
            LogStep($"Found {count} inventory items");
            Assert.That(count, Is.GreaterThan(0), "No inventory items were found on the page");
        });

        // Log first few item names for additional detail
        var names = await inventoryPage.GetItemNamesAsync();
        var i = 1;
        foreach (var name in names)
        {
            LogStep($"Item {i}: {name}");
            i++;
        }
    }

    [Test]
    public async Task AddItemToCart()
    {
        LogStep("AddItemToCart: start");
        var loginPage = new LoginPage(Page);
        await loginPage.LoginAsAsync(settings.Username, settings.Password);
        var inventoryPage = new InventoryPage(Page);

        var names = await inventoryPage.GetItemNamesAsync();
        var first = names.FirstOrDefault();
        Assert.That(first, Is.Not.Null.And.Not.Empty, "No item name available to add");

        await inventoryPage.AddToCartByNameAsync(first!);
        LogStep($"Added '{first}' to cart");

        var badge = await inventoryPage.GetCartBadgeCountAsync();
        LogStep($"Cart badge count after adding: {badge}");
        Assert.That(badge, Is.GreaterThan(0), "Cart badge should be incremented after adding an item");
    }

    [Test]
    public async Task RemoveItemFromCart()
    {
        LogStep("RemoveItemFromCart: start");
        var loginPage = new LoginPage(Page);
        await loginPage.LoginAsAsync(settings.Username, settings.Password);
        var inventoryPage = new InventoryPage(Page);

        var names = await inventoryPage.GetItemNamesAsync();
        var first = names.FirstOrDefault();
        Assert.That(first, Is.Not.Null.And.Not.Empty, "No item name available to remove");

        await inventoryPage.AddToCartByNameAsync(first!);
        LogStep($"Added '{first}' to cart");

        var badgeAfterAdd = await inventoryPage.GetCartBadgeCountAsync();
        Assert.That(badgeAfterAdd, Is.GreaterThan(0));

        await inventoryPage.RemoveFromCartByNameAsync(first!);
        LogStep($"Removed '{first}' from cart");

        var badgeAfterRemove = await inventoryPage.GetCartBadgeCountAsync();
        LogStep($"Cart badge count after removal: {badgeAfterRemove}");
        Assert.That(badgeAfterRemove, Is.LessThanOrEqualTo(0), "Cart badge should be zero after removing the item");
    }

    [Test]
    public async Task FilterByNameAndVerifyOrder()
    {
        LogStep("FilterByNameAndVerifyOrder: start");
        var loginPage = new LoginPage(Page);
        await loginPage.LoginAsAsync(settings.Username, settings.Password);
        var inventoryPage = new InventoryPage(Page);

        // Sort A->Z
        await inventoryPage.SortByNameAscAsync();
        LogStep("Sorted by name A->Z");
        var names = await inventoryPage.GetItemNamesAsync();
        var expected = names.OrderBy(n => n).ToList();
        Assert.That(names, Is.EqualTo(expected), "Names should be in ascending order after sorting A->Z");

        // Sort Z->A
        await inventoryPage.SortByNameDescAsync();
        LogStep("Sorted by name Z->A");
        var namesDesc = await inventoryPage.GetItemNamesAsync();
        var expectedDesc = namesDesc.OrderByDescending(n => n).ToList();
        Assert.That(namesDesc, Is.EqualTo(expectedDesc), "Names should be in descending order after sorting Z->A");
    }

    [Test]
    public async Task FilterByPriceAndVerifyOrder()
    {
        LogStep("FilterByPriceAndVerifyOrder: start");
        var loginPage = new LoginPage(Page);
        await loginPage.LoginAsAsync(settings.Username, settings.Password);
        var inventoryPage = new InventoryPage(Page);

        await inventoryPage.SortByPriceLowToHighAsync();
        LogStep("Sorted by price low->high");
        var prices = await inventoryPage.GetItemPricesAsync();
        var expected = prices.OrderBy(p => p).ToList();
        Assert.That(prices, Is.EqualTo(expected), "Prices should be in ascending order after sorting low->high");

        await inventoryPage.SortByPriceHighToLowAsync();
        LogStep("Sorted by price high->low");
        var pricesDesc = await inventoryPage.GetItemPricesAsync();
        var expectedDesc = pricesDesc.OrderByDescending(p => p).ToList();
        Assert.That(pricesDesc, Is.EqualTo(expectedDesc), "Prices should be in descending order after sorting high->low");
    }

    [Test]
    public async Task CanNavigateToCartFromInventory()
    {
        LogStep("CanNavigateToCartFromInventory: start");
        var loginPage = new LoginPage(Page);
        await loginPage.LoginAsAsync(settings.Username, settings.Password);
        var inventoryPage = new InventoryPage(Page);

        // Ensure at least one item in cart to validate cart page contents
        var names = await inventoryPage.GetItemNamesAsync();
        var first = names.FirstOrDefault();
        if (!string.IsNullOrEmpty(first))
        {
            await inventoryPage.AddToCartByNameAsync(first!);
            LogStep($"Added '{first}' to cart before navigation");
        }

        var cartPage = await inventoryPage.GoToCartAsync();
        LogStep("Navigated to cart page");

        var itemCount = await cartPage.GetItemCountAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cartPage.IsDisplayedAsync().Result, Is.True, "Cart page should be displayed");
            Assert.That(Page.Url, Does.Contain("cart.html"));
            Assert.That(itemCount, Is.GreaterThanOrEqualTo(0));
        });
    }
}