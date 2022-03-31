#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.DTOs;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [TestClass]
    [TestCategory(nameof(RollbarInfrastructureConfigFixture))]
    public class RollbarInfrastructureConfigFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
            //var config = new RollbarInfrastructureConfig("access_token", "special_environment");

            //if (RollbarInfrastructure.Instance.IsInitialized)
            //{
            //    RollbarInfrastructure.Instance.Config.Reconfigure(config);
            //}
            //else
            //{
            //    RollbarInfrastructure.Instance.Init(config);
            //}
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void TestBasics()
        {
            var config = new RollbarInfrastructureConfig();
            Console.WriteLine(config.TraceAsString());

            var results = config.Validate();
            Assert.AreEqual(1, results.Count, "One Validation Rule failed!");
            Console.WriteLine("Validation Results:");
            foreach(var result in results)
            {
                Console.WriteLine($"  {result}");
            }
            Console.WriteLine();
        }


        [TestMethod]
        public void CustomTelemetryOptionsReconfigured()
        {
            int expectedTelemetryQueueDepth = 20;

            var config = new RollbarInfrastructureConfig("access_token", "special_environment");

            Assert.IsFalse(config.RollbarTelemetryOptions.TelemetryEnabled);
            Assert.IsTrue(config.RollbarTelemetryOptions.TelemetryQueueDepth != expectedTelemetryQueueDepth);
            Assert.IsTrue(config.RollbarTelemetryOptions.TelemetryAutoCollectionTypes == TelemetryType.None);

            var telemetryOptions = new RollbarTelemetryOptions(true, 20)
            {
                TelemetryAutoCollectionTypes = TelemetryType.Log | TelemetryType.Error | TelemetryType.Network
            };
            config.RollbarTelemetryOptions.Reconfigure(telemetryOptions);

            Assert.IsTrue(config.RollbarTelemetryOptions.TelemetryEnabled);
            Assert.AreEqual(expectedTelemetryQueueDepth, config.RollbarTelemetryOptions.TelemetryQueueDepth);
            Assert.IsTrue((config.RollbarTelemetryOptions.TelemetryAutoCollectionTypes & TelemetryType.Log) == TelemetryType.Log);
            Assert.IsTrue((config.RollbarTelemetryOptions.TelemetryAutoCollectionTypes & TelemetryType.Error) == TelemetryType.Error);
            Assert.IsTrue((config.RollbarTelemetryOptions.TelemetryAutoCollectionTypes & TelemetryType.Network) == TelemetryType.Network);
            Assert.IsTrue((config.RollbarTelemetryOptions.TelemetryAutoCollectionTypes & TelemetryType.Manual) == TelemetryType.None);
        }

        [TestMethod]
        public void CustomTelemetryOptionsMakeIntoTelemetryCollector()
        {
            int expectedTelemetryQueueDepth = 20;

            var config = new RollbarInfrastructureConfig("access_token", "special_environment");

            Assert.IsFalse(config.RollbarTelemetryOptions.TelemetryEnabled);
            Assert.IsTrue(config.RollbarTelemetryOptions.TelemetryQueueDepth != expectedTelemetryQueueDepth);
            Assert.IsTrue(config.RollbarTelemetryOptions.TelemetryAutoCollectionTypes == TelemetryType.None);

            var telemetryOptions = new RollbarTelemetryOptions(true, 20)
            {
                TelemetryAutoCollectionTypes = TelemetryType.Log | TelemetryType.Error | TelemetryType.Network
            };
            config.RollbarTelemetryOptions.Reconfigure(telemetryOptions);


            if (RollbarInfrastructure.Instance.IsInitialized)
            {
                RollbarInfrastructure.Instance.Config.Reconfigure(config);
            }
            else
            {
                RollbarInfrastructure.Instance.Init(config);
            }

            Assert.IsTrue(RollbarInfrastructure.Instance.TelemetryCollector.Config.TelemetryEnabled);
            Assert.AreEqual(expectedTelemetryQueueDepth, RollbarInfrastructure.Instance.TelemetryCollector.Config.TelemetryQueueDepth);
            Assert.IsTrue((RollbarInfrastructure.Instance.TelemetryCollector.Config.TelemetryAutoCollectionTypes & TelemetryType.Log) == TelemetryType.Log);
            Assert.IsTrue((RollbarInfrastructure.Instance.TelemetryCollector.Config.TelemetryAutoCollectionTypes & TelemetryType.Error) == TelemetryType.Error);
            Assert.IsTrue((RollbarInfrastructure.Instance.TelemetryCollector.Config.TelemetryAutoCollectionTypes & TelemetryType.Network) == TelemetryType.Network);
            Assert.IsTrue((RollbarInfrastructure.Instance.TelemetryCollector.Config.TelemetryAutoCollectionTypes & TelemetryType.Manual) == TelemetryType.None);

            Assert.AreEqual(expectedTelemetryQueueDepth, RollbarInfrastructure.Instance.TelemetryCollector.QueueDepth);
        }

    }

}
