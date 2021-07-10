#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.DTOs;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarLoggerConfigFixture))]
    public class RollbarLoggerConfigFixture
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
            Assert.AreEqual("seedToken", config.RollbarDestinationOptions.AccessToken);
            Console.WriteLine(config.TraceAsString());


            RollbarDestinationOptions destinationOptions = new RollbarDestinationOptions();
            destinationOptions.AccessToken = "CUSTOM";
            config.RollbarDestinationOptions.Reconfigure(destinationOptions);
            Assert.AreEqual("CUSTOM", config.RollbarDestinationOptions.AccessToken, "Options reconfig works!");
            Console.WriteLine(config.TraceAsString());

            RollbarLoggerConfig newConfig = new RollbarLoggerConfig();
            Assert.AreEqual("seedToken", newConfig.RollbarDestinationOptions.AccessToken);
            Console.WriteLine(newConfig.TraceAsString());

            newConfig.Reconfigure(config);
            Assert.AreEqual("CUSTOM", newConfig.RollbarDestinationOptions.AccessToken, "Structured config's reconfig works!");
            Console.WriteLine(newConfig.TraceAsString());
        }


        [TestMethod]
        public void TestInstanceCreation()
        {
            try
            {
                RollbarLoggerConfig rConfig = new RollbarLoggerConfig(RollbarUnitTestSettings.AccessToken);
            }
            catch
            {
                Assert.Fail("The instance creation is expected to succeed, but did not!");
            }

        }

        [TestMethod]
        public void TestDefaultScrubFields()
        {
            var config = new RollbarLoggerConfig(RollbarUnitTestSettings.AccessToken);
            Assert.IsTrue(
                new HashSet<string>(config.RollbarDataSecurityOptions.ScrubFields).IsSupersetOf(RollbarDataScrubbingHelper.Instance.GetDefaultFields())
                );
        }

        [TestMethod]
        public void TestGetSafeScrubFields()
        {
            var scrubFields = new string[] { "one", "two", "three", };
            var scrubSafelistFields = new string[] { "two", };
            var expectedSafeScrubFields = new string[] { "one", "three", };

            var destinationOptions = new RollbarDestinationOptions(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment);
            var dataSecurityOptions = new RollbarDataSecurityOptions();
            dataSecurityOptions.ScrubFields = scrubFields;
            dataSecurityOptions.ScrubSafelistFields = scrubSafelistFields;

            var loggerConfig = new RollbarLoggerConfig(RollbarUnitTestSettings.AccessToken);
            loggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);
            loggerConfig.RollbarDataSecurityOptions.Reconfigure(dataSecurityOptions);

            var result = loggerConfig.RollbarDataSecurityOptions.GetFieldsToScrub();

            Assert.AreEqual(expectedSafeScrubFields.Length, result.Count);
            foreach(var expected in expectedSafeScrubFields)
            {
                Assert.IsTrue(result.Contains(expected));
            }
        }

        [TestMethod]
        public void TestRollbarConfigEqualityMethod()
        {
            RollbarDestinationOptions destinationOptions = new RollbarDestinationOptions("12345", "env1");
            RollbarLoggerConfig rConfig = new RollbarLoggerConfig();
            rConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);

            // test the same config instance references: 
            Assert.IsTrue(rConfig.Equals(rConfig), "Same instances are always equal.");

            destinationOptions = new RollbarDestinationOptions("12345", "env1"); // same as rConfig...
            RollbarLoggerConfig rConfigSimilar = new RollbarLoggerConfig();
            rConfigSimilar.RollbarDestinationOptions.Reconfigure(destinationOptions);


            destinationOptions = new RollbarDestinationOptions("12345", "env2");
            RollbarLoggerConfig rConfigOneDiff = new RollbarLoggerConfig();

            destinationOptions = new RollbarDestinationOptions("02345", "env1");
            RollbarLoggerConfig rConfigAnotherDiff = new RollbarLoggerConfig();

            destinationOptions = new RollbarDestinationOptions("02345", "env2");
            RollbarLoggerConfig rConfigTwoDiffs = new RollbarLoggerConfig();

            destinationOptions = new RollbarDestinationOptions("12345", null);
            RollbarLoggerConfig rConfigOneNullifed = new RollbarLoggerConfig();

            // test different config instances simple properties:
            Assert.IsTrue(rConfig.Equals(rConfigSimilar), "Simple properties: Similar instances are always equal.");
            Assert.IsFalse(rConfig.Equals(rConfigOneDiff), "Simple properties: One different property value makes unequal.");
            Assert.IsFalse(rConfig.Equals(rConfigAnotherDiff), "Simple properties: Another different property value makes unequal.");
            Assert.IsFalse(rConfig.Equals(rConfigTwoDiffs), "Simple properties: Multiple different property values make unequal.");
            Assert.IsFalse(rConfig.Equals(rConfigOneNullifed), "Simple properties: Nullified property of one config instance makes unequal.");

            // test structured/complex properties:
            Person person = new Person() { UserName = "dude 1", Email = "dude1@here.com", };
            Person personSimilar = new Person() { UserName = "dude 1", Email = "dude1@here.com", };
            Person personOneDiff = new Person() { UserName = "dude 2", Email = "dude1@here.com", };
            Person personOneNull = new Person() { UserName = "dude 2", Email = null, };

            RollbarPayloadAdditionOptions additionOptions = new RollbarPayloadAdditionOptions();

            additionOptions.Person = person;
            rConfig.RollbarPayloadAdditionOptions.Reconfigure(additionOptions);
            Assert.IsTrue(rConfig.Equals(rConfig), "Structured properties: Same instances are always equal.");

            additionOptions.Person = person;
            rConfigSimilar.RollbarPayloadAdditionOptions.Reconfigure(additionOptions);
            Assert.IsTrue(rConfig.Equals(rConfigSimilar), "Structured properties: Similar #1 instances are always equal.");

            additionOptions.Person = personSimilar;
            rConfigSimilar.RollbarPayloadAdditionOptions.Reconfigure(additionOptions);
            Assert.IsTrue(rConfig.Equals(rConfigSimilar), "Structured properties: Similar #2 instances are always equal.");

            additionOptions.Person = personOneDiff;
            rConfigSimilar.RollbarPayloadAdditionOptions.Reconfigure(additionOptions);
            Assert.IsFalse(rConfig.Equals(rConfigSimilar), "Structured properties: One different property value makes unequal.");

            additionOptions.Person = personOneNull;
            rConfigSimilar.RollbarPayloadAdditionOptions.Reconfigure(additionOptions);
            Assert.IsFalse(rConfig.Equals(rConfigSimilar), "Structured properties: Nullified property of one config instance makes unequal.");
        }

        /// <summary>
        /// Tests the basic validation.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="expectedTotalFailedRules">The expected total failed rules.</param>
        [DataTestMethod]
        [DataRow("Token1", null, 0)]
        [DataRow(null, null, 1)]
        [DataRow("Token1", "", 1)]
        [DataRow(null, "", 2)]
        [DataRow("Token1", "PersonID", 0)]
        public void TestBasicValidation(string token, string personId, int expectedTotalFailedRules)
        {
            Person person = null;
            if(personId != null)
            {
                person = new Person() { Id = personId };
            }

            RollbarLoggerConfig config = new RollbarLoggerConfig(token);
            RollbarPayloadAdditionOptions additionOptions = new RollbarPayloadAdditionOptions();
            additionOptions.Person = person;
            config.RollbarPayloadAdditionOptions.Reconfigure(additionOptions);

            var failedValidationRules = config.Validate();
            Assert.AreEqual(expectedTotalFailedRules, failedValidationRules.Count);
        }

    }

}
