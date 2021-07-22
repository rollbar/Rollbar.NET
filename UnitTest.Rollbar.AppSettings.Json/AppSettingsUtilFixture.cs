#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.AppSettings.Json
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using global::Rollbar.AppSettings.Json;
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

            RollbarInfrastructureConfig config = new RollbarInfrastructureConfig("default=none");
            Assert.IsNull(config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Person);
            Assert.IsNull(config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server);

            string settingsFolderPath = Path.Combine(Environment.CurrentDirectory, "TestData");
            AppSettingsUtility.LoadAppSettings(config, settingsFolderPath, "appsettings.json");

            // The test data looks like this:
            //===============================
            //"Rollbar": {
            //    "RollbarLoggerConfig": {
            //        "RollbarDestinationOptions": {
            //            "AccessToken": "17965fa5041749b6bf7095a190001ded",
            //            "Environment": "unit-tests"
            //        },
            //        "RollbarDeveloperOptions": {
            //            "Enabled": true,
            //            "LogLevel": "Info"
            //        },
            //        "RollbarPayloadAdditionOptions": {
            //            "Server": {
            //                "Root": "C://Blah/Blah",
            //                "Cpu": "x64"
            //            },
            //            "Person": {
            //                "UserName": "jbond"
            //            }
            //        },
            //        "RollbarDataSecurityOptions": {
            //            "ScrubFields": [
            //                "ThePassword",
            //                "TheSecret"
            //            ],
            //        "PersonDataCollectionPolicies": "Username, Email",
            //        "IpAddressCollectionPolicy": "CollectAnonymized"
            //        }
            //    },
            //    "RollbarInfrastructureOptions": {
            //        "MaxReportsPerMinute": 160,
            //        "ReportingQueueDepth": 120
            //    }
            //}

            Assert.AreEqual("17965fa5041749b6bf7095a190001ded", config.RollbarLoggerConfig.RollbarDestinationOptions.AccessToken);
            Assert.AreEqual("unit-tests", config.RollbarLoggerConfig.RollbarDestinationOptions.Environment);

            Assert.AreEqual(true, config.RollbarLoggerConfig.RollbarDeveloperOptions.Enabled);
            Assert.AreEqual(ErrorLevel.Info, config.RollbarLoggerConfig.RollbarDeveloperOptions.LogLevel);

            Assert.AreEqual(160, config.RollbarInfrastructureOptions.MaxReportsPerMinute);
            Assert.AreEqual(120, config.RollbarInfrastructureOptions.ReportingQueueDepth);

            Assert.IsTrue(config.RollbarLoggerConfig.RollbarDataSecurityOptions.ScrubFields.Length >= 2);
            Assert.AreEqual(
                PersonDataCollectionPolicies.Username | PersonDataCollectionPolicies.Email
                , config.RollbarLoggerConfig.RollbarDataSecurityOptions.PersonDataCollectionPolicies
                );
            Assert.AreEqual(
                IpAddressCollectionPolicy.CollectAnonymized
                , config.RollbarLoggerConfig.RollbarDataSecurityOptions.IpAddressCollectionPolicy
                );

            Assert.AreEqual("jbond", config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Person.UserName);
            Assert.IsNotNull(config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server);
            Assert.IsTrue(config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server.ContainsKey("cpu"));
            Assert.IsTrue(config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server.ContainsKey("root"));
            Assert.IsFalse(config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server.ContainsKey("Cpu"));
            Assert.IsFalse(config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server.ContainsKey("Root"));
            Assert.AreEqual(config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server["cpu"], config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server.Cpu);
            Assert.AreEqual(config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server["root"], config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server.Root);
            Assert.AreEqual("x64", config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server.Cpu);
            Assert.AreEqual("C://Blah/Blah", config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Server.Root);
        }

        [TestMethod]
        public void LoadRollbarTelemetryAppSettingsTest()
        {
            RollbarTelemetryOptions config = new RollbarTelemetryOptions(false, 5, TelemetryType.None, TimeSpan.FromMilliseconds(100));
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
