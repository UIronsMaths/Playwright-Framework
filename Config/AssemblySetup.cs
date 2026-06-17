using NUnit.Framework;
using System.IO;

namespace PlaywrightCSharpFramework.Tests
{
    [SetUpFixture]
    public class AssemblySetup
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Ensure Allure and our framework artifact directories exist before any test attributes run
            var workDir = TestContext.CurrentContext.WorkDirectory;
            //Directory.CreateDirectory(Path.Combine(workDir, "artifacts"));
            Directory.CreateDirectory(Path.Combine(workDir, "Artifacts"));
        }
    }
}
