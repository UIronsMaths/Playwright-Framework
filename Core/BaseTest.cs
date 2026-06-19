using Allure.Net.Commons;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using PlaywrightCSharpFramework.Config;
using PlaywrightCSharpFramework.Reporting;
using PlaywrightCSharpFramework.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace PlaywrightCSharpFramework.Core;

public abstract class PlaywrightTestBase
{
    protected FrameworkSettings Settings { get; private set; } = null!;
    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected string BrowserName { get; }

    // Built fresh in SetUpAsync with TestId/TestName/Browser baked in via ForContext.
    // Use this instead of the static Log.Information(...) anywhere inside a test
    // (SetUp, the test body, TearDown). Unlike LogContext/AsyncLocal, this is just
    // a field on the test instance, so it survives thread hops under parallel
    // execution without any ambient-context flow required.
    protected ILogger TestLog { get; private set; } = Log.Logger;

    protected PlaywrightTestBase(string browserName = "chromium")
    {
        BrowserName = browserName;
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Logging.Configure();
        Settings = SettingsLoader.Load();
    }

    [SetUp]
    public async Task SetUpAsync()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        var testId = TestContext.CurrentContext.Test.ID;

        TestLog = Log.Logger
            .ForContext("TestId", testId)
            .ForContext("TestName", testName)
            .ForContext("Browser", BrowserName);

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await LaunchBrowserAsync();

        ExtentReportManager.StartTest($"{testName} [{BrowserName}]");

        Context = await Browser.NewContextAsync(new()
        {
            BaseURL = Settings.BaseUrl,
            RecordVideoDir = Settings.RecordVideo ? ArtifactPaths.Videos : null
        });

        Context.SetDefaultTimeout(Settings.ActionTimeoutMilliseconds);
        Context.SetDefaultNavigationTimeout(Settings.NavigationTimeoutMilliseconds);

        if (Settings.CaptureTraceOnFailure)
        {
            await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
        }

        Page = await Context.NewPageAsync();
        TestLog.Information("Started test {TestName}", testName);
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        var result = TestContext.CurrentContext.Result;
        bool failed = result.Outcome.Status == TestStatus.Failed;
        string safeName = string.Concat(TestContext.CurrentContext.Test.Name
            .Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

        try
        {
            if (failed && Settings.CaptureScreenshotOnFailure)
            {
                string screenshot = Path.Combine(ArtifactPaths.Screenshots, $"{safeName}-{stamp}.png");
                await Page.ScreenshotAsync(new() { Path = screenshot, FullPage = true });
                ExtentReportManager.AttachScreenshot(screenshot);
                AllureApi.AddAttachment("Failure screenshot", "image/png", screenshot);
            }

            if (Settings.CaptureTraceOnFailure)
            {
                string trace = Path.Combine(ArtifactPaths.Traces, $"{safeName}-{stamp}.zip");
                if (failed)
                {
                    await Context.Tracing.StopAsync(new() { Path = trace });
                    AllureApi.AddAttachment("Failure trace", "application/zip", trace);
                }
                else
                {
                    await Context.Tracing.StopAsync();
                }
            }

            if (failed) ExtentReportManager.Fail(result.Message ?? "Failed");
            else ExtentReportManager.Pass("Test passed");
        }
        finally
        {
            ExtentReportManager.EndTest();

            string? videoPath = null;
            if (Settings.RecordVideo)
            {
                videoPath = await Page.Video!.PathAsync();
            }

            await Context.CloseAsync(); // finalizes the video file

            if (Settings.RecordVideo && videoPath is not null)
            {
                if (failed)
                {
                    AllureApi.AddAttachment("Failure video", "video/webm", videoPath);
                }
                else if (File.Exists(videoPath))
                {
                    File.Delete(videoPath);
                }
            }

            await Browser.CloseAsync();
            Playwright.Dispose();
            // ExtentReportManager.Flush() intentionally NOT called here.
            // It now runs exactly once, in AssemblyTearDown's [OneTimeTearDown],
            // after all fixtures/tests in the run have finished.
        }
    }

    private async Task<IBrowser> LaunchBrowserAsync()
    {
        var options = new BrowserTypeLaunchOptions
        {
            Headless = Settings.Headless,
            SlowMo = Settings.SlowMoMilliseconds
        };
        var browserName = !string.IsNullOrWhiteSpace(BrowserName) ? BrowserName : Settings.Browser;
        return browserName.ToLowerInvariant() switch
        {
            "firefox" => await Playwright.Firefox.LaunchAsync(options),
            "webkit" => await Playwright.Webkit.LaunchAsync(options),
            _ => await Playwright.Chromium.LaunchAsync(options)
        };
    }
}