using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;
using PlaywrightCSharpFramework.Core;
using PlaywrightCSharpFramework.Pages;
using Serilog;
using static Microsoft.Playwright.Assertions;
using System.Text.RegularExpressions;
namespace PlaywrightCSharpFramework.Tests;

[TestFixture("chromium")]
[TestFixture("firefox")]
[TestFixture("webkit")]
[AllureNUnit]
[AllureSuite("SauceDemo")]
public sealed class LoginTests : PlaywrightTestBase
{
    public LoginTests(string? browser) : base(browser) { }

    [Test]
    [AllureFeature("Authentication")]
    [AllureStory("Login matrix from fixture data")]
    [TestCaseSource(nameof(UserCases))]
    public async Task Login_WithMatrixValues_BehavesAsExpected(UserData testCase)
    {
        var loginPage = new LoginPage(Page);

        await loginPage.OpenAsync(Settings.BaseUrl);
        await loginPage.LoginAsync(testCase.Username, testCase.Password);

        // Log the test case name so it's available in logs
        Log.Information($"Test case | {testCase.TestName}");

        if (testCase.Expected == "success")
        {
            var inventoryPage = new InventoryPage(Page);
            await Expect(Page).ToHaveURLAsync(new Regex("inventory.html"));

            var isLoaded = await inventoryPage.IsLoadedAsync();
            var title = await inventoryPage.GetTitleAsync();

            Assert.Multiple(() =>
            {
                Assert.That(isLoaded, Is.True);
                Assert.That(title, Is.EqualTo("Products"));
            });
        }
        else
        {
            await Expect(Page).Not.ToHaveURLAsync(new Regex("inventory.html"));
            var errorMessage = await loginPage.ErrorTextAsync();
            Assert.That(errorMessage, Is.Not.Empty);
        }
    }

    private static IEnumerable<UserData> UserCases() => JsonReader.ReadUsers();
}