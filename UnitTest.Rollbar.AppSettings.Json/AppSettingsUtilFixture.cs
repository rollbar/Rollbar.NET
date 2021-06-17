#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.AppSettings.Json
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using global::Rollbar.AppSettings.Json;
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
            var sever = new Server();

            RollbarLoggerConfig config = new RollbarLoggerConfig("default=none");
            Assert.IsNull(config.RollbarPayloadAdditionOptions.Person);
            Assert.IsNull(config.RollbarPayloadAdditionOptions.Server);

            AppSettingsUtility.LoadAppSettings(config, Path.Combine(Environment.CurrentDirectory, "TestData"), "appsettings.json");

            // The test data looks like this:
            //===============================
            //"Rollbar": {
            //    "RollbarDestinationOptions": {
            //        "AccessToken": "17965fa5041749b6bf7095a190001ded",
            //        "Environment": "unit-tests"
            //    },
            //    "RollbarDeveloperOptions": {
            //        "Enabled": true,
            //        "LogLevel": "Info"
            //    },
            //    "RollbarPayloadAdditionOptions": {
            //        "Server": {
            //            "Root": "C://Blah/Blah",
            //            "Cpu": "x64"
            //        },
            //        "Person": {
            //            "UserName": "jbond"
            //        }
            //    },
            //    "RollbarDataSecurityOptions": {
            //    "ScrubFields": [
            //        "ThePassword",
            //        "TheSecret"
            //    ],
            //    "PersonDataCollectionPolicies": "Username, Email",
            //    "IpAddressCollectionPolicy": "CollectAnonymized"
            //    },
            //    "RollbarInfrastructureOptions": {
            //        "MaxReportsPerMinute": 160,
            //        "ReportingQueueDepth": 120
            //    }
            //}

            Assert.AreEqual("17965fa5041749b6bf7095a190001ded", config.RollbarDestinationOptions.AccessToken);
            Assert.AreEqual("unit-tests", config.RollbarDestinationOptions.Environment);

            Assert.AreEqual(true, config.RollbarDeveloperOptions.Enabled);
            Assert.AreEqual(ErrorLevel.Info, config.RollbarDeveloperOptions.LogLevel);

            Assert.AreEqual(160, config.RollbarInfrastructureOptions.MaxReportsPerMinute);
            Assert.AreEqual(120, config.RollbarInfrastructureOptions.ReportingQueueDepth);

            Assert.IsTrue(config.RollbarDataSecurityOptions.ScrubFields.Length >= 2);
            Assert.AreEqual(
                PersonDataCollectionPolicies.Username | PersonDataCollectionPolicies.Email
                , config.RollbarDataSecurityOptions.PersonDataCollectionPolicies
                );
            Assert.AreEqual(
                IpAddressCollectionPolicy.CollectAnonymized
                , config.RollbarDataSecurityOptions.IpAddressCollectionPolicy
                );

            Assert.AreEqual("jbond", config.RollbarPayloadAdditionOptions.Person.UserName);
            Assert.IsNotNull(config.RollbarPayloadAdditionOptions.Server);
            Assert.IsTrue(config.RollbarPayloadAdditionOptions.Server.ContainsKey("cpu"));
            Assert.IsTrue(config.RollbarPayloadAdditionOptions.Server.ContainsKey("root"));
            Assert.IsFalse(config.RollbarPayloadAdditionOptions.Server.ContainsKey("Cpu"));
            Assert.IsFalse(config.RollbarPayloadAdditionOptions.Server.ContainsKey("Root"));
            Assert.AreEqual(config.RollbarPayloadAdditionOptions.Server["cpu"], config.RollbarPayloadAdditionOptions.Server.Cpu);
            Assert.AreEqual(config.RollbarPayloadAdditionOptions.Server["root"], config.RollbarPayloadAdditionOptions.Server.Root);
            Assert.AreEqual("x64", config.RollbarPayloadAdditionOptions.Server.Cpu);
            Assert.AreEqual("C://Blah/Blah", config.RollbarPayloadAdditionOptions.Server.Root);
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
