using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using PlaywrightCSharpFramework.Config;

public static class ScreenshotUtils
{
    private static string DefaultDirectory => SettingsLoader.Load().ScreenshotDirectory;

    public static async Task<string?> CaptureOnFailureAsync(IPage page, string testName)
        => await CaptureAsync(page, testName, DefaultDirectory);

    public static async Task<string?> CaptureAsync(IPage page, string testName, string? directory = null)
    {
        var targetDir = directory ?? DefaultDirectory;

        try
        {
            Directory.CreateDirectory(targetDir);
            var fileName = $"{Sanitise(testName)}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var fullPath = Path.Combine(targetDir, fileName);

            await page.ScreenshotAsync(new PageScreenshotOptions { Path = fullPath, FullPage = true });
            return Path.GetFullPath(fullPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ScreenshotUtils] Failed to capture screenshot for '{testName}': {ex.Message}");
            return null;
        }
    }

    public static async Task<string?> CaptureAsBase64Async(IPage page)
    {
        try
        {
            var bytes = await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true });
            return Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ScreenshotUtils] Failed to capture base64 screenshot: {ex.Message}");
            return null;
        }
    }

    private static string Sanitise(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }
}