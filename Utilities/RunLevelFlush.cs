using NUnit.Framework;
using PlaywrightCSharpFramework.Reporting;
using Serilog;

namespace PlaywrightCSharpFramework.Tests;

// Runs once before any test in this namespace, and once after all of them finish,
// regardless of how many TestFixture instances (chromium/firefox/webkit) or parallel
// workers are involved. This is the single place the Extent report gets flushed to disk.
[SetUpFixture]
public class GlobalTestSetup
{
    [OneTimeTearDown]
    public void RunFinished()
    {
        ExtentReportManager.Flush();
    }
}