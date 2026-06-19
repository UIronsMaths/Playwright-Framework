using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using PlaywrightCSharpFramework.Core;
using PlaywrightCSharpFramework.Pages;

namespace PlaywrightCSharpFramework.Tests;

[TestFixture("chromium")]
[TestFixture("firefox")]
[TestFixture("webkit")]
[AllureNUnit]
[AllureSuite("SauceDemo")]
public class InventoryTests : PlaywrightTestBase
{
    public InventoryTests(string browser) : base(browser) { }

    // Tags a log line as a "Step" so it can be filtered separately from
    // Expected/Actual/assertion logs in Seq/Allure. Reuses the TestId/TestName/
    // Browser context already attached to TestLog in PlaywrightTestBase.SetUpAsync.
    private void LogStep(string messageTemplate, params object[] propertyValues) =>
        TestLog.ForContext("Phase", "Step").Information(messageTemplate, propertyValues);

    [Test]
    public async Task InventoryPageShowsItems()
    {
        TestLog.Information("EXPECTED: Inventory Page shows items | SUCCESS");

        string outcome = "FAILED";
        string actualResult = "N/A";

        try
        {
            var loginPage = new LoginPage(Page);
            await loginPage.OpenAsync(Settings.BaseUrl);
            LogStep("Logging in with valid credentials");
            await loginPage.LoginAsync(Settings.Username, Settings.Password);
            var inventoryPage = new InventoryPage(Page);

            LogStep("Verifying inventory page is displayed and contains items");

            Assert.Multiple(async () =>
            {
                Assert.That(Page.Url, Does.Contain("inventory.html"));
                Assert.That(await inventoryPage.IsLoadedAsync(), Is.True, "Inventory page should be visible after login");

                var count = await inventoryPage.ProductCountAsync();
                LogStep("Found {ItemCount} inventory items", count);
                Assert.That(count, Is.GreaterThan(0), "No inventory items were found on the page");
            });

            // Log first few item names for additional detail
            var names = await inventoryPage.GetItemNamesAsync();
            var i = 1;
            foreach (var name in names)
            {
                LogStep("Item {ItemIndex}: {ItemName}", i, name);
                i++;
            }

            actualResult = $"{names.Count} items found";
            outcome = "SUCCESS";
        }
        finally
        {
            TestLog.Information(
                "ACTUAL: Inventory Page shows items | {Outcome} | {ActualResult}",
                outcome, actualResult);
        }
    }

    [Test]
    public async Task AddItemToCart()
    {
        TestLog.Information("EXPECTED: Adding an item to the cart increments the cart badge | SUCCESS");

        string outcome = "FAILED";
        string actualResult = "N/A";

        try
        {
            var loginPage = new LoginPage(Page);
            await loginPage.OpenAsync(Settings.BaseUrl);
            await loginPage.LoginAsync(Settings.Username, Settings.Password);
            var inventoryPage = new InventoryPage(Page);

            var names = await inventoryPage.GetItemNamesAsync();
            var first = names.FirstOrDefault();
            Assert.That(first, Is.Not.Null.And.Not.Empty, "No item name available to add");

            await inventoryPage.AddToCartByNameAsync(first!);
            LogStep("Added {ItemName} to cart", first);

            var badge = await inventoryPage.GetCartBadgeCountAsync();
            LogStep("Cart badge count after adding: {BadgeCount}", badge);
            Assert.That(badge, Is.GreaterThan(0), "Cart badge should be incremented after adding an item");

            actualResult = $"Cart badge = {badge} after adding '{first}'";
            outcome = "SUCCESS";
        }
        finally
        {
            TestLog.Information(
                "ACTUAL: Adding an item to the cart increments the cart badge | {Outcome} | {ActualResult}",
                outcome, actualResult);
        }
    }

    [Test]
    public async Task RemoveItemFromCart()
    {
        TestLog.Information("EXPECTED: Removing an item from the cart clears the cart badge | SUCCESS");

        string outcome = "FAILED";
        string actualResult = "N/A";

        try
        {
            var loginPage = new LoginPage(Page);
            await loginPage.OpenAsync(Settings.BaseUrl);
            await loginPage.LoginAsync(Settings.Username, Settings.Password);
            var inventoryPage = new InventoryPage(Page);

            var names = await inventoryPage.GetItemNamesAsync();
            var first = names.FirstOrDefault();
            Assert.That(first, Is.Not.Null.And.Not.Empty, "No item name available to remove");

            await inventoryPage.AddToCartByNameAsync(first!);
            LogStep("Added {ItemName} to cart", first);

            var badgeAfterAdd = await inventoryPage.GetCartBadgeCountAsync();
            Assert.That(badgeAfterAdd, Is.GreaterThan(0));

            await inventoryPage.RemoveFromCartByNameAsync(first!);
            LogStep("Removed {ItemName} from cart", first);

            var badgeAfterRemove = await inventoryPage.GetCartBadgeCountAsync();
            LogStep("Cart badge count after removal: {BadgeCount}", badgeAfterRemove);
            Assert.That(badgeAfterRemove, Is.LessThanOrEqualTo(0), "Cart badge should be zero after removing the item");

            actualResult = $"Cart badge = {badgeAfterRemove} after removing '{first}'";
            outcome = "SUCCESS";
        }
        finally
        {
            TestLog.Information(
                "ACTUAL: Removing an item from the cart clears the cart badge | {Outcome} | {ActualResult}",
                outcome, actualResult);
        }
    }

    [Test]
    public async Task FilterByNameAndVerifyOrder()
    {
        TestLog.Information("EXPECTED: Sorting by name orders items A->Z and Z->A correctly | SUCCESS");

        string outcome = "FAILED";
        string actualResult = "N/A";

        try
        {
            var loginPage = new LoginPage(Page);
            await loginPage.OpenAsync(Settings.BaseUrl);
            await loginPage.LoginAsync(Settings.Username, Settings.Password);
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

            actualResult = "Ascending and descending name order verified";
            outcome = "SUCCESS";
        }
        finally
        {
            TestLog.Information(
                "ACTUAL: Sorting by name orders items A->Z and Z->A correctly | {Outcome} | {ActualResult}",
                outcome, actualResult);
        }
    }

    [Test]
    public async Task FilterByPriceAndVerifyOrder()
    {
        TestLog.Information("EXPECTED: Sorting by price orders items low->high and high->low correctly | SUCCESS");

        string outcome = "FAILED";
        string actualResult = "N/A";

        try
        {
            var loginPage = new LoginPage(Page);
            await loginPage.OpenAsync(Settings.BaseUrl);
            await loginPage.LoginAsync(Settings.Username, Settings.Password);
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

            actualResult = "Ascending and descending price order verified";
            outcome = "SUCCESS";
        }
        finally
        {
            TestLog.Information(
                "ACTUAL: Sorting by price orders items low->high and high->low correctly | {Outcome} | {ActualResult}",
                outcome, actualResult);
        }
    }

    [Test]
    public async Task CanNavigateToCartFromInventory()
    {
        TestLog.Information("EXPECTED: Navigating to the cart from inventory shows the cart page | SUCCESS");

        string outcome = "FAILED";
        string actualResult = "N/A";

        try
        {
            var loginPage = new LoginPage(Page);
            await loginPage.OpenAsync(Settings.BaseUrl);
            await loginPage.LoginAsync(Settings.Username, Settings.Password);
            var inventoryPage = new InventoryPage(Page);

            // Ensure at least one item in cart to validate cart page contents
            var names = await inventoryPage.GetItemNamesAsync();
            var first = names.FirstOrDefault();
            if (!string.IsNullOrEmpty(first))
            {
                await inventoryPage.AddToCartByNameAsync(first!);
                LogStep("Added {ItemName} to cart before navigation", first);
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

            actualResult = $"Cart page displayed with {itemCount} item(s)";
            outcome = "SUCCESS";
        }
        finally
        {
            TestLog.Information(
                "ACTUAL: Navigating to the cart from inventory shows the cart page | {Outcome} | {ActualResult}",
                outcome, actualResult);
        }
    }

    [Test]
    public async Task DeliberateFailureForScreenshotCheck()
    {
        TestLog.Information("EXPECTED: This test intentionally fails | FAILURE");

        string outcome = "FAILED";
        string actualResult = "N/A";

        try
        {
            var loginPage = new LoginPage(Page);
            await loginPage.OpenAsync(Settings.BaseUrl);
            await loginPage.LoginAsync(Settings.Username, Settings.Password);
            var inventoryPage = new InventoryPage(Page);

            LogStep("Intentionally asserting something false to trigger failure");

            Assert.That(await inventoryPage.IsLoadedAsync(), Is.False, "Deliberate failure: inventory page IS loaded, but we're asserting it isn't");

            actualResult = "Did not fail as expected";
            outcome = "SUCCESS";
        }
        finally
        {
            TestLog.Information(
                "ACTUAL: This test intentionally fails | {Outcome} | {ActualResult}",
                outcome, actualResult);
        }
    }
}