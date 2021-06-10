#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [TestClass]
    [TestCategory(nameof(RollbarDestinationOptionsFixture))]
    public class RollbarDestinationOptionsFixture
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
            RollbarDestinationOptions options = new RollbarDestinationOptions();
            Console.WriteLine(options.TraceAsString());
            Assert.AreEqual(null, options.AccessToken, "AccessToken is null!");

            var results = options.Validate();
            Assert.AreEqual(1, results.Count, "One Validation Rule failed!");
            Console.WriteLine("Validation Results:");
            foreach(var result in results)
            {
                Console.WriteLine($"  {result}");
            }
            Console.WriteLine();


            options.AccessToken = "ACCESS_TOKEN";
            Console.WriteLine(options.TraceAsString());

            Assert.AreEqual("ACCESS_TOKEN", options.AccessToken, "Expected AccessToken");

            results = options.Validate();
            Assert.AreEqual(0, results.Count, "No Validation Rule failed!");
            Console.WriteLine("Validation Results:");
            foreach(var result in results)
            {
                Console.WriteLine($"  {result}");
            }
            Console.WriteLine();
        }

    }

}
