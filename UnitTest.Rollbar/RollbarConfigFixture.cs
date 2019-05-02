namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Deploys;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Defines test class RollbarConfigFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(RollbarConfigFixture))]
    public class RollbarConfigFixture
    {
        /// <summary>
        /// Setups the fixture.
        /// </summary>
        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        /// <summary>
        /// Tears down fixture.
        /// </summary>
        [TestCleanup]
        public void TearDownFixture()
        {
        }

        /// <summary>
        /// Defines the test method TestInstanceCreation.
        /// </summary>
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

        /// <summary>
        /// Defines the test method TestGetSafeScrubFields.
        /// </summary>
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

        /// <summary>
        /// Defines the test method TestRollbarConfigEqualityMethod.
        /// </summary>
        [TestMethod]
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
            Assert.IsTrue(rConfig.Equals(rConfigSimilar), "Structured properties: Similar #1 instances are always equal.");
            rConfigSimilar.Person = personSimilar;
            Assert.IsTrue(rConfig.Equals(rConfigSimilar), "Structured properties: Similar #2 instances are always equal.");
            rConfigSimilar.Person = personOneDiff;
            Assert.IsFalse(rConfig.Equals(rConfigSimilar), "Structured properties: One different property value makes unequal.");
            rConfigSimilar.Person = personOneNull;
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
            if (personId != null)
            {
                person = new Person() { Id = personId };
            }

            RollbarConfig config = new RollbarConfig() { AccessToken = token, Person = person, };

            var failedValidationRules = config.Validate();
            Assert.AreEqual(expectedTotalFailedRules, failedValidationRules.Count);
        }
    }
}
