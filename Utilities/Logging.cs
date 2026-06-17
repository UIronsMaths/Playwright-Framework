using Serilog;
using Serilog.Events;
namespace PlaywrightCSharpFramework.Utilities;

public static class Logging
{
    private static readonly object Sync = new();
    private static bool _configured;

    public static void Configure()
    {
        lock (Sync)
        {
            if (_configured) return;
            Directory.CreateDirectory(ArtifactPaths.Logs);

            const string template =
                "{Timestamp:HH:mm:ss.fff} [{Level:u3}] (T{ThreadId}) [{Browser}] {TestName} | {Message:lj}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information, outputTemplate: template)
                .WriteTo.File(
                    Path.Combine(ArtifactPaths.Logs, "playwright-.log"),
                    restrictedToMinimumLevel: LogEventLevel.Debug,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: template)
                .WriteTo.File(
                    new Serilog.Formatting.Compact.CompactJsonFormatter(),
                    Path.Combine(ArtifactPaths.Logs, "playwright-structured-.json"),
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _configured = true;
        }
    }
}