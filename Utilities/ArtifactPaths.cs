namespace PlaywrightCSharpFramework.Utilities;

public static class ArtifactPaths
{
    public static string Root => Path.Combine(TestContext.CurrentContext.WorkDirectory, "Artifacts");
    public static string Screenshots => Ensure("Screenshots");
    public static string Traces => Ensure("Traces");
    public static string Videos => Ensure("Videos");
    public static string Logs => Ensure("Logs");
    public static string Extent => Ensure("Extent");
    private static string Ensure(string folder)
    {
        var path = Path.Combine(Root, folder);
        Directory.CreateDirectory(path);
        return path;
    }
}