using Allure.Net.Commons;
//using Allure.Net.Commons.Model;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Microsoft.Playwright;
using PlaywrightCSharpFramework.Core;
using PlaywrightCSharpFramework.Pages;
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
    public LoginTests(string browser) : base(browser) { }

    [Test]
    [AllureFeature("Authentication")]
    [AllureStory("Login matrix from fixture data")]
    [AllureSeverity(SeverityLevel.critical)]
    [TestCaseSource(nameof(UserCases))]
    public async Task Login_BehavesAsExpected(UserData testCase)
    {
        TestLog.Information(
            "EXPECTED: {JsonTestName} | {Expected}",
            testCase.Testname, testCase.Expected);

        string outcome = "FAILED";
        string actualResult = "N/A";

        try
        {
            var loginPage = new LoginPage(Page);

            await loginPage.OpenAsync(Settings.BaseUrl);
            await loginPage.LoginAsync(testCase.Username, testCase.Password);

            if (testCase.Expected == "SUCCESS")
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
                outcome = "SUCCESS";
            }
            else
            {
                await Expect(Page).Not.ToHaveURLAsync(new Regex("inventory.html"));
                var errorMessage = await loginPage.ErrorTextAsync();
                actualResult = errorMessage;
                Assert.That(errorMessage, Is.Not.Empty);
                outcome = "FAILURE";
            }
        }
        finally
        {
            // This always runs, whether assertions passed, failed, or an exception was thrown
            TestLog.Information(
                "ACTUAL: {JsonTestName} | {Outcome} | {ActualResult}",
                testCase.Testname, outcome, actualResult);
        }
    }

    private static IEnumerable<UserData> UserCases() => JsonReader.ReadUsers();
}