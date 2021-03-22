#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using dto = global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    [TestCategory(nameof(TelemetryFixture))]
    public class TelemetryFixture
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
        public void BodyAndTypeMatch()
        {
            dto.Telemetry telemetry = null;

            telemetry = new dto.Telemetry(
                dto.TelemetrySource.Client
                , dto.TelemetryLevel.Critical
                , new dto.LogTelemetry("test")
                );
            Assert.AreEqual(dto.TelemetryType.Log, telemetry.Type);
            Console.WriteLine(JsonConvert.SerializeObject(telemetry));

            telemetry = new dto.Telemetry(
                dto.TelemetrySource.Client
                , dto.TelemetryLevel.Critical
                , new dto.ErrorTelemetry(new Exception("Test exception"))
                );
            Assert.AreEqual(dto.TelemetryType.Error, telemetry.Type);
            Console.WriteLine(JsonConvert.SerializeObject(telemetry));

            telemetry = new dto.Telemetry(
                dto.TelemetrySource.Client
                , dto.TelemetryLevel.Critical
                , new dto.DomTelemetry("TestElement")
                );
            Assert.AreEqual(dto.TelemetryType.Dom, telemetry.Type);
            Console.WriteLine(JsonConvert.SerializeObject(telemetry));

            telemetry = new dto.Telemetry(
                dto.TelemetrySource.Client
                , dto.TelemetryLevel.Critical
                , new dto.NavigationTelemetry("Here", "There")
                );
            Assert.AreEqual(dto.TelemetryType.Navigation, telemetry.Type);
            Console.WriteLine(JsonConvert.SerializeObject(telemetry));

            telemetry = new dto.Telemetry(
                dto.TelemetrySource.Client
                , dto.TelemetryLevel.Critical
                , new dto.NetworkTelemetry("GET", "api/users", DateTime.Now, null, 200)
                );
            Assert.AreEqual(dto.TelemetryType.Network, telemetry.Type);
            Console.WriteLine(JsonConvert.SerializeObject(telemetry));

            var custom = new Dictionary<string, object>
            {
                { "key1", "firstValue" },
                { "key2", "secondValue" },
            };
            telemetry = new dto.Telemetry(
                dto.TelemetrySource.Client
                , dto.TelemetryLevel.Critical
                , new dto.ManualTelemetry(custom)
                );
            Assert.AreEqual(dto.TelemetryType.Manual, telemetry.Type);
            Console.WriteLine(JsonConvert.SerializeObject(telemetry));
        }
    }
}
