namespace UnitTest.Rollbar.Common
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Defines test class ValidatorFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(RuntimeEnvironmentUtilityFixture))]
    public class RuntimeEnvironmentUtilityFixture
    {
        /// <summary>
        /// Setups the fixture.
        /// </summary>
        [TestInitialize]
        public void SetupFixture()
        {
        }

        /// <summary>
        /// Tears down fixture.
        /// </summary>
        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [DataTestMethod]
        public void TestGetSdkRuntimeLocationPath()
        {
            var path = RuntimeEnvironmentUtility.GetSdkRuntimeLocationPath();
            Assert.IsNotNull(path);
            Assert.IsFalse(string.IsNullOrWhiteSpace(path));
        }

        [TestMethod]
        public void TestGetSdkRuntimeLocationPathWithBinSubfolder()
        {
            var expectedPath = Path.Combine(AppContext.BaseDirectory, "bin");
            try
            {
                Directory.CreateDirectory(expectedPath);

                var path = RuntimeEnvironmentUtility.GetSdkRuntimeLocationPath();
                Assert.IsNotNull(path);
                Assert.IsFalse(string.IsNullOrWhiteSpace(path));
                Assert.IsTrue(path.EndsWith("bin"));
            }
            finally
            {
                if (Directory.Exists(expectedPath))
                {
                    Directory.Delete(expectedPath);
                }
            }
        }
    }
}
