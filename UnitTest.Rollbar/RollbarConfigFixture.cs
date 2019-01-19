#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Deploys;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarConfigFixture))]
    public class RollbarConfigFixture
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
        public void TestInstanceCreation()
        {
            try
            {
                RollbarConfig rConfig = new RollbarConfig(RollbarUnitTestSettings.AccessToken);
            }
            catch
            {
                Assert.Fail("The instance creation is expected to succeed, but did not!");
            }

        }

        [TestMethod]
        public void TestGetSafeScrubFields()
        {
            var scrubFields = new string[] { "one", "two", "three", };
            var scrubWhitelistFields = new string[] { "two", };
            var expectedSafeScrubFields = new string[] { "one", "three", };

            var loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken)
                {
                    Environment = RollbarUnitTestSettings.Environment,
                    ScrubFields = scrubFields,
                    ScrubWhitelistFields = scrubWhitelistFields,
                };

            var result = loggerConfig.GetFieldsToScrub();

            Assert.AreEqual(expectedSafeScrubFields.Length, result.Count);
            foreach(var expected in expectedSafeScrubFields)
            {
                Assert.IsTrue(result.Contains(expected));
            }
        }

        //[TestMethod] //until fully implemented...
        public void TestRollbarConfigEqualityMethod()
        {
            RollbarConfig rConfig = new RollbarConfig("12345") { Environment = "env1" };

            // test the same config instance references: 
            Assert.IsTrue(rConfig.Equals(rConfig), "Same instances are always equal.");

            RollbarConfig rConfigSimilar = new RollbarConfig("12345") { Environment = "env1" }; // same as rConfig...
            RollbarConfig rConfigOneDiff = new RollbarConfig("12345") { Environment = "env2" };
            RollbarConfig rConfigAnotherDiff = new RollbarConfig("02345") { Environment = "env1" };
            RollbarConfig rConfigTwoDiffs = new RollbarConfig("02345") { Environment = "env2" };
            RollbarConfig rConfigOneNullifed = new RollbarConfig("12345") { Environment = null };


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

            rConfig.Person = person;
            Assert.IsTrue(rConfig.Equals(rConfig), "Structured properties: Same instances are always equal.");
            rConfigSimilar.Person = person;
            Assert.IsTrue(rConfig.Equals(rConfigSimilar), "Structured properties: Similar instances are always equal.");
            rConfigSimilar.Person = personSimilar;
            Assert.IsTrue(rConfig.Equals(rConfigSimilar), "Structured properties: Similar instances are always equal.");
            rConfigSimilar.Person = personOneDiff;
            Assert.IsFalse(rConfig.Equals(rConfigSimilar), "Structured properties: One different property value makes unequal.");
            rConfigSimilar.Person = personOneNull;
            Assert.IsFalse(rConfig.Equals(rConfigSimilar), "Structured properties: Nullified property of one config instance makes unequal.");
        }
    }
}
