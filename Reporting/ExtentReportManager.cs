using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using PlaywrightCSharpFramework.Utilities;
using System.Collections.Concurrent;
namespace PlaywrightCSharpFramework.Reporting;

public static class ExtentReportManager
{
    private static readonly Lazy<ExtentReports> InstanceHolder = new(CreateReport);
    private static readonly ConcurrentDictionary<string, ExtentTest> Tests = new();
    private static readonly object FlushLock = new();

    // Keyed by the NUnit test ID so each parallel/parameterized test instance
    // gets its own ExtentTest, instead of relying on AsyncLocal flowing correctly
    // across SetUp -> Test -> TearDown (it doesn't, reliably, under parallel fixtures).
    public static ExtentTest Current
    {
        get
        {
            var id = NUnit.Framework.TestContext.CurrentContext.Test.ID;
            if (Tests.TryGetValue(id, out var test))
            {
                return test;
            }
            throw new InvalidOperationException(
                $"Extent test has not been started for test ID '{id}'.");
        }
    }

    public static void StartTest(string name)
    {
        var id = NUnit.Framework.TestContext.CurrentContext.Test.ID;
        Tests[id] = InstanceHolder.Value.CreateTest(name);
    }

    public static void Pass(string message) => Current.Pass(message);
    public static void Fail(string message) => Current.Fail(message);
    public static void AttachScreenshot(string path) => Current.AddScreenCaptureFromPath(path);

    public static void EndTest()
    {
        var id = NUnit.Framework.TestContext.CurrentContext.Test.ID;
        Tests.TryRemove(id, out _);
    }

    // Safe to call from multiple threads/fixtures; only one Flush runs at a time.
    // Still best called once, from a [OneTimeTearDown] at the assembly/SetUpFixture
    // level, rather than after every single test.
    public static void Flush()
    {
        lock (FlushLock)
        {
            InstanceHolder.Value.Flush();
        }
    }

    private static ExtentReports CreateReport()
    {
        var reporter = new ExtentSparkReporter(
        Path.Combine(ArtifactPaths.Extent, "PlaywrightReport.html"));
        var extent = new ExtentReports();
        extent.AttachReporter(reporter);
        extent.AddSystemInfo("Framework", "Playwright .NET + NUnit");
        return extent;
    }
}