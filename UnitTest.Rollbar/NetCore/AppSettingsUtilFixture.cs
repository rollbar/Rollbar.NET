#if NETCOREAPP

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.NetCore
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using global::Rollbar.NetCore;
    using global::Rollbar.Telemetry;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(AppSettingsUtilFixture))]
    public class AppSettingsUtilFixture
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
        public void LoadRollbarAppSettingsTest()
        {
            RollbarConfig config = new RollbarConfig("default=none");
            AppSettingsUtility.LoadAppSettings(config, Path.Combine(Environment.CurrentDirectory, "TestData"), "appsettings.json");

            // The test data looks like this:
            //===============================
            //"Rollbar": {
            //    "AccessToken": "17965fa5041749b6bf7095a190001ded",
            //    "Environment": "unit-tests",
            //    "Enabled": true,
            //    "MaxReportsPerMinute": 160,
            //    "ReportingQueueDepth": 120,
            //    "LogLevel": "Info",
            //    "ScrubFields": [
            //      "ThePassword",
            //      "TheSecret"
            //    ],
            //    "Person": {
            //      "UserName": "jbond"
            //    },
            //    "PersonDataCollectionPolicies": "Username, Email",
            //    "IpAddressCollectionPolicy": "CollectAnonymized",
            //  }

            Assert.AreEqual("17965fa5041749b6bf7095a190001ded", config.AccessToken);
            Assert.AreEqual("unit-tests", config.Environment);
            Assert.AreEqual(true, config.Enabled);
            Assert.AreEqual(160, config.MaxReportsPerMinute);
            Assert.AreEqual(120, config.ReportingQueueDepth);
            Assert.AreEqual(ErrorLevel.Info, config.LogLevel.Value);
            Assert.IsTrue(config.ScrubFields.Length >= 2);
            Assert.AreEqual("jbond", config.Person.UserName);
            Assert.AreEqual(
                PersonDataCollectionPolicies.Username | PersonDataCollectionPolicies.Email
                , config.PersonDataCollectionPolicies
                );
            Assert.AreEqual(
                IpAddressCollectionPolicy.CollectAnonymized
                , config.IpAddressCollectionPolicy
                );
        }

        [TestMethod]
        public void LoadRollbarTelemetryAppSettingsTest()
        {
            TelemetryConfig config = new TelemetryConfig(false, 5, TelemetryType.None, TimeSpan.FromMilliseconds(100));
            Console.WriteLine(JsonConvert.SerializeObject(config));

            Assert.AreEqual(false, config.TelemetryEnabled);
            Assert.AreEqual(5, config.TelemetryQueueDepth);
            Assert.AreEqual(TelemetryType.None, config.TelemetryAutoCollectionTypes);
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), config.TelemetryAutoCollectionInterval);

            AppSettingsUtility.LoadAppSettings(config, Path.Combine(Environment.CurrentDirectory, "TestData"), "appsettings.json");
            Console.WriteLine(JsonConvert.SerializeObject(config));

            // The test data looks like this:
            //===============================
            //"RollbarTelemetry": {
            //    "TelemetryEnabled": true,
            //    "TelemetryQueueDepth": 100,
            //    "TelemetryAutoCollectionTypes": "Network, Log, Error",
            //    "TelemetryAutoCollectionInterval":  "00:00:00.3000000",
            //},

            Assert.AreEqual(true, config.TelemetryEnabled);
            Assert.AreEqual(100, config.TelemetryQueueDepth);
            Assert.AreEqual(TelemetryType.Network | TelemetryType.Log | TelemetryType.Error, config.TelemetryAutoCollectionTypes);
            Assert.AreEqual(TimeSpan.FromMilliseconds(300), config.TelemetryAutoCollectionInterval);
        }
    }
}

#endif
