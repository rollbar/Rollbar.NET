#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [TestClass]
    [TestCategory(nameof(RollbarLoggerConfigFixture))]
    public class RollbarLoggerConfigFixture
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
        public void TestBasics()
        {
            RollbarLoggerConfig config = new RollbarLoggerConfig();
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
        public void TestReconfiguration()
        {
            RollbarLoggerConfig config = new RollbarLoggerConfig();
            Assert.IsNull(config.RollbarDestinationOptions.AccessToken);
            Console.WriteLine(config.TraceAsString());


            RollbarDestinationOptions destinationOptions = new RollbarDestinationOptions();
            destinationOptions.AccessToken = "CUSTOM";
            config.RollbarDestinationOptions.Reconfigure(destinationOptions);
            Assert.AreEqual("CUSTOM", config.RollbarDestinationOptions.AccessToken, "Options reconfig works!");
            Console.WriteLine(config.TraceAsString());

            RollbarLoggerConfig newConfig = new RollbarLoggerConfig();
            Assert.IsNull(newConfig.RollbarDestinationOptions.AccessToken);
            Console.WriteLine(newConfig.TraceAsString());

            newConfig.Reconfigure(config);
            Assert.AreEqual("CUSTOM", newConfig.RollbarDestinationOptions.AccessToken, "Structured config's reconfig works!");
            Console.WriteLine(newConfig.TraceAsString());
        }
    }

}
