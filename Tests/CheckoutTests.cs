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
public class CheckoutTests : PlaywrightTestBase
{
    public CheckoutTests(string browser) : base(browser) { }

    [Test]
    public async Task ContinueShoppingFromCartReturnsToInventory()
    {
        TestLog.Information("EXPECTED: Continue Shopping from cart returns to inventory | SUCCESS");

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
            Assert.That(first, Is.Not.Null.And.Not.Empty);

            await inventoryPage.AddToCartByNameAsync(first!);
            TestLog.Information($"Added '{first}' to cart");

            var cart = await inventoryPage.GoToCartAsync();
            TestLog.Information("At cart page");

            var inv = await cart.ContinueShoppingAsync();
            TestLog.Information("Clicked Continue Shopping");

            Assert.Multiple(async () =>
            {
                Assert.That(Page.Url, Does.Contain("inventory.html"));
            });

            actualResult = "Returned to inventory page after Continue Shopping";
            outcome = "SUCCESS";
        }
        finally
        {
            TestLog.Information(
                "ACTUAL: Continue Shopping from cart returns to inventory | {Outcome} | {ActualResult}",
                outcome, actualResult);
        }
    }

    [Test]
    public async Task CancelCheckoutAtStepOneReturnsToCart()
    {
        TestLog.Information("EXPECTED: Cancelling checkout at step one returns to cart | SUCCESS");

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
            Assert.That(first, Is.Not.Null.And.Not.Empty);

            await inventoryPage.AddToCartByNameAsync(first!);
            var cart = await inventoryPage.GoToCartAsync();

            var stepOne = await cart.ProceedToCheckoutAsync();
            TestLog.Information("On checkout step one");

            var returnedCart = await stepOne.CancelAsync();
            TestLog.Information("Cancelled checkout at step one");

            var itemCount = await returnedCart.GetItemCountAsync();

            Assert.Multiple(() =>
            {
                Assert.That(itemCount, Is.GreaterThanOrEqualTo(0));
                Assert.That(Page.Url, Does.Contain("cart.html"));
            });

            actualResult = $"Returned to cart page with {itemCount} item(s)";
            outcome = "SUCCESS";
        }
        finally
        {
            TestLog.Information(
                "ACTUAL: Cancelling checkout at step one returns to cart | {Outcome} | {ActualResult}",
                outcome, actualResult);
        }
    }

    [Test]
    public async Task CompleteCheckoutFlowFinishesAndBackHome()
    {
        TestLog.Information("EXPECTED: Completing the checkout flow finishes and returns home | SUCCESS");

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
            Assert.That(first, Is.Not.Null.And.Not.Empty);

            await inventoryPage.AddToCartByNameAsync(first!);
            var cart = await inventoryPage.GoToCartAsync();

            var stepOne = await cart.ProceedToCheckoutAsync();
            TestLog.Information("Filling checkout details and continuing");
            var stepTwo = await stepOne.ContinueAsync("Test", "User", "90210");

            TestLog.Information("Finishing checkout");
            var complete = await stepTwo.FinishAsync();

            Assert.Multiple(async () =>
            {
                Assert.That(await complete.IsDisplayedAsync(), Is.True);
                Assert.That(Page.Url, Does.Contain("checkout-complete.html"));
            });

            var inventory = await complete.BackHomeAsync();
            TestLog.Information("Clicked Back Home after complete");

            Assert.That(Page.Url, Does.Contain("inventory.html"));

            actualResult = "Checkout completed and returned to inventory via Back Home";
            outcome = "SUCCESS";
        }
        finally
        {
            TestLog.Information(
                "ACTUAL: Completing the checkout flow finishes and returns home | {Outcome} | {ActualResult}",
                outcome, actualResult);
        }
    }
}