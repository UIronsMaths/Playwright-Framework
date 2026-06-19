namespace PlaywrightCSharpFramework.Config;

public sealed class FrameworkSettings
{
    public string BaseUrl { get; set; } = "https://www.saucedemo.com/";
    public string Browser { get; set; } = "chromium";
    public bool Headless { get; set; } = true;
    public int SlowMoMilliseconds { get; set; }
    public int ActionTimeoutMilliseconds { get; set; } = 10_000;
    public int NavigationTimeoutMilliseconds { get; set; } = 30_000;
    public string ReportType { get; set; } = "both";
    public bool CaptureScreenshotOnFailure { get; set; } = true;
    public bool CaptureTraceOnFailure { get; set; } = true;
    public bool RecordVideo { get; set; } = true;
    public string ScreenshotDirectory { get; set; } = "Artifacts/Screenshots";
    public string ExtentReportDirectory { get; set; } = "Artifacts/Extent";
    public string LogDirectory { get; set; } = "Artifacts/Logs";
    public string BaseDirectory { get; set; } = string.Empty;
    public string Username { get; set;  } = string.Empty;
    public string Password { get; set; } = string.Empty;
}