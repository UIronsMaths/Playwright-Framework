using NUnit.Framework;
using PlaywrightCSharpFramework.Utilities;
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
            Directory.CreateDirectory(Path.Combine(workDir, "Artifacts"));

            // Clear out artifacts from previous runs
            ClearDirectory(ArtifactPaths.Videos);
            ClearDirectory(ArtifactPaths.Traces);
            ClearDirectory(ArtifactPaths.Screenshots);
            ClearDirectory(ArtifactPaths.Logs); // adjust the name if your logs path constant differs
        }

        private static void ClearDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return;
            }

            foreach (var file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }
        }
    }
}