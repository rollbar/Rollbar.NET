#if NETFX

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.NetFramework
{
    using global::Rollbar;
    using global::Rollbar.NetFramework;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;
    using System.Threading;
    using System.Configuration;

    [TestClass]
    [TestCategory(nameof(RollbarConfigSectionFixture))]
    public class RollbarConfigSectionFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void LoadRollbarConfigSectionTest()
        {
            ExeConfigurationFileMap exeConfigurationFileMap = new ExeConfigurationFileMap(); //new ExeConfigurationFileMap(@"TestData\App.config");
            exeConfigurationFileMap.ExeConfigFilename = @"TestData\App.config";
            var testAppConfig  = ConfigurationManager.OpenMappedExeConfiguration(exeConfigurationFileMap, ConfigurationUserLevel.None);
            RollbarConfigSection config =
                testAppConfig.GetSection("rollbar") as RollbarConfigSection;

            //<rollbar
            // accessToken = "17965fa5041749b6bf7095a190001ded"
            // environment = "RollbarNetSamples"
            // personDataCollectionPolicies = "Username | Email"
            // ipAddressCollectionPolicy = "CollectAnonymized"
            // />

            Assert.AreEqual("17965fa5041749b6bf7095a190001ded", config.AccessToken);
            Assert.AreEqual("unit-tests", config.Environment);
            Assert.AreEqual(true, config.Enabled.Value);
            Assert.AreEqual(160, config.MaxReportsPerMinute.Value);
            Assert.AreEqual(120, config.ReportingQueueDepth.Value);
            Assert.AreEqual(ErrorLevel.Info, config.LogLevel.Value);
            Assert.AreEqual("ThePassword, Secret", config.ScrubFields);
            Assert.AreEqual(
                PersonDataCollectionPolicies.Username | PersonDataCollectionPolicies.Email
                , config.PersonDataCollectionPolicies
                );
            Assert.AreEqual(
                IpAddressCollectionPolicy.CollectAnonymized
                , config.IpAddressCollectionPolicy
                );
        }
    }
}

#endif
