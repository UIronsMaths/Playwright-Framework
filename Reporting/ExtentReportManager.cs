using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using PlaywrightCSharpFramework.Utilities;
namespace PlaywrightCSharpFramework.Reporting;

public static class ExtentReportManager
{
    private static readonly Lazy<ExtentReports> InstanceHolder = new(CreateReport);
    private static readonly AsyncLocal<ExtentTest?> CurrentHolder = new();
    // Ensure a test instance is always available to avoid exceptions when NUnit runs tear-down on a different context
    public static ExtentTest Current => CurrentHolder.Value ?? InstanceHolder.Value.CreateTest("Unnamed Test");
    public static void StartTest(string name)
    => CurrentHolder.Value = InstanceHolder.Value.CreateTest(name);
    public static void Pass(string message)
    {
        try { Current.Pass(message); } catch { /* best-effort */ }
    }
    public static void Fail(string message)
    {
        try { Current.Fail(message); } catch { /* best-effort */ }
    }
    public static void AttachScreenshot(string path)
    {
        try { Current.AddScreenCaptureFromPath(path); } catch { /* best-effort */ }
    }
    public static void Flush() => InstanceHolder.Value.Flush();
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
