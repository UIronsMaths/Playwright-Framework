using Microsoft.Extensions.Configuration;
namespace PlaywrightCSharpFramework.Config;

public static class SettingsLoader
{
    public static FrameworkSettings Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables(prefix: "PW_")
            .Build();
        var settings = configuration
            .GetSection("TestSettings")
            .Get<FrameworkSettings>() ?? new FrameworkSettings();
        //settings.Username = Environment.GetEnvironmentVariable("SAUCE_USERNAME")
        //?? settings.Username;
        //settings.Password = Environment.GetEnvironmentVariable("SAUCE_PASSWORD")
        //?? settings.Password;
        return settings;
    }
}