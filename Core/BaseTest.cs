using Allure.Net.Commons;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using PlaywrightCSharpFramework.Config;
using PlaywrightCSharpFramework.Reporting;
using PlaywrightCSharpFramework.Utilities;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace PlaywrightCSharpFramework.Core;

public abstract class PlaywrightTestBase
{
    private readonly string? _browserOverride;
    private readonly Stopwatch _stopwatch = new();
    // logging context disposables (OneTimeSetup only)
    private readonly List<IDisposable> _logContextDisposables = new();

    protected FrameworkSettings Settings { get; private set; } = null!;
    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    // Logging removed: no logger field

    protected PlaywrightTestBase(string? browser = null) => _browserOverride = browser;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Initialize logging so tests can optionally use Log.Logger or LogContext
        Logging.Configure();

        Settings = SettingsLoader.Load();
        if (_browserOverride is not null) Settings.Browser = _browserOverride;
        // Push minimal logging context for the fixture lifetime only
        _logContextDisposables.Add(LogContext.PushProperty("Machine", Environment.MachineName));
        _logContextDisposables.Add(LogContext.PushProperty("OS", RuntimeInformation.OSDescription));
        _logContextDisposables.Add(LogContext.PushProperty("Runtime", RuntimeInformation.FrameworkDescription));
        _logContextDisposables.Add(LogContext.PushProperty("Browser", Settings.Browser));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // dispose pushed logging context properties
        foreach (var d in _logContextDisposables)
        {
            try { d.Dispose(); } catch { }
        }
        _logContextDisposables.Clear();
    }

    [SetUp]
    public async Task SetUpAsync()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        _stopwatch.Restart();
        ExtentReportManager.StartTest(testName);

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await LaunchBrowserAsync();
        // Logging removed: browser launch version not logged

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

        // Logging removed: page ready information not logged
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        _stopwatch.Stop();
        var result = TestContext.CurrentContext.Result;
        bool failed = result.Outcome.Status == TestStatus.Failed;
        string safeName = string.Concat(TestContext.CurrentContext.Test.Name
            .Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

        if (failed) { Log.Information($"Test FAILED: {result.Message}"); }
        else { Log.Information("Test PASSED"); }

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
                    // Logging removed: trace saved path not logged
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
            await Context.CloseAsync();
            await Browser.CloseAsync();
            Playwright.Dispose();
            ExtentReportManager.Flush();
            // Logging removed: disposal debug message
        }
    }

    // Page diagnostics and logging removed

    private async Task<IBrowser> LaunchBrowserAsync()
    {
        var options = new BrowserTypeLaunchOptions
        {
            Headless = Settings.Headless,
            SlowMo = Settings.SlowMoMilliseconds
        };
        return Settings.Browser.ToLowerInvariant() switch
        {
            "firefox" => await Playwright.Firefox.LaunchAsync(options),
            "webkit" => await Playwright.Webkit.LaunchAsync(options),
            _ => await Playwright.Chromium.LaunchAsync(options)
        };
    }
}