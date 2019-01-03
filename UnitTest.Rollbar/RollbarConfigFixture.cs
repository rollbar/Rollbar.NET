#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Deploys;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarConfigFixture))]
    public class RollbarConfigFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void TestInstanceCreation()
        {
            try
            {
                RollbarConfig rConfig = new RollbarConfig(RollbarUnitTestSettings.AccessToken);
            }
            catch
            {
                Assert.Fail("The instance creation is expected to succeed, but did not!");
            }

        }

        [TestMethod]
        public void TestGetSafeScrubFields()
        {
            var scrubFields = new string[] { "one", "two", "three", };
            var scrubWhitelistFields = new string[] { "two", };
            var expectedSafeScrubFields = new string[] { "one", "three", };

            var loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken)
                {
                    Environment = RollbarUnitTestSettings.Environment,
                    ScrubFields = scrubFields,
                    ScrubWhitelistFields = scrubWhitelistFields,
                };

            var result = loggerConfig.GetFieldsToScrub();

            Assert.AreEqual(expectedSafeScrubFields.Length, result.Count);
            foreach(var expected in expectedSafeScrubFields)
            {
                Assert.IsTrue(result.Contains(expected));
            }
        }
    }
}
