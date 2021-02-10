#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.App.Config
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Configuration;
    using global::Rollbar;
    using global::Rollbar.App.Config;
    using global::Rollbar.DTOs;

    [TestClass]
    [TestCategory(nameof(RollbarTelemetryConfigSectionFixture))]
    public class RollbarTelemetryConfigSectionFixture
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
        public void LoadRollbarTelemetryConfigSectionTest()
        {
            ExeConfigurationFileMap exeConfigurationFileMap = new ExeConfigurationFileMap(); //new ExeConfigurationFileMap(@"TestData\App.config");
            exeConfigurationFileMap.ExeConfigFilename = @"TestData\AppTest.config";
            var testAppConfig = ConfigurationManager.OpenMappedExeConfiguration(exeConfigurationFileMap, ConfigurationUserLevel.None);
            RollbarTelemetryConfigSection config =
                testAppConfig.GetSection("rollbarTelemetry") as RollbarTelemetryConfigSection;

            //< rollbarTelemetry
            //  telemetryEnabled = "true"
            //  telemetryQueueDepth = "100"
            //  telemetryAutoCollectionTypes = "Network, Log, Error"
            //  telemetryAutoCollectionInterval = "00:00:00.3000000"
            //  />

            Assert.IsTrue(config.TelemetryEnabled.HasValue);
            Assert.IsTrue(config.TelemetryQueueDepth.HasValue);
            Assert.IsTrue(config.TelemetryAutoCollectionTypes.HasValue);
            Assert.IsTrue(config.TelemetryAutoCollectionInterval.HasValue);

            Assert.AreEqual(true, config.TelemetryEnabled.Value);
            Assert.AreEqual(100, config.TelemetryQueueDepth.Value);
            Assert.AreEqual(TelemetryType.Network | TelemetryType.Log | TelemetryType.Error, config.TelemetryAutoCollectionTypes.Value);
            Assert.AreEqual(TimeSpan.FromMilliseconds(300), config.TelemetryAutoCollectionInterval.Value);
        }

    }
}

