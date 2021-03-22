namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines test class PersonFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(PersonFixture))]
    public class PersonFixture
    {
        /// <summary>
        /// Setups the fixture.
        /// </summary>
        [TestInitialize]
        public void SetupFixture()
        {
        }

        /// <summary>
        /// Tears down fixture.
        /// </summary>
        [TestCleanup]
        public void TearDownFixture()
        {
        }

        /// <summary>
        /// Defines the test method PersonIdRenderedCorrectly.
        /// </summary>
        [TestMethod]
        public void PersonIdRenderedCorrectly()
        {
            var rp = new Person("person_id");
            Assert.AreEqual("{\"id\":\"person_id\"}", JsonConvert.SerializeObject(rp));
        }

        /// <summary>
        /// Defines the test method PersonUsernameRenderedCorrectly.
        /// </summary>
        [TestMethod]
        public void PersonUsernameRenderedCorrectly()
        {
            var rp = new Person("person_id")
            {
                UserName = "chris_pfohl",
            };
            Assert.AreEqual("{\"id\":\"person_id\",\"username\":\"chris_pfohl\"}", JsonConvert.SerializeObject(rp));
        }

        /// <summary>
        /// Defines the test method PersonEmailRenderedCorrectly.
        /// </summary>
        [TestMethod]
        public void PersonEmailRenderedCorrectly()
        {
            var rp = new Person("person_id")
            {
                Email = "chris@rollbar.com",
            };
            Assert.AreEqual("{\"id\":\"person_id\",\"email\":\"chris@rollbar.com\"}", JsonConvert.SerializeObject(rp));
        }

        /// <summary>
        /// Persons the identifier validation works.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="expectedTotalValidationErrors">The expected total validation errors.</param>
        [DataTestMethod]
        [DataRow(null, 1)]
        [DataRow("", 1)]
        [DataRow("PersonId", 0)]
        public void PersonIdValidationWorks(string personId, int expectedTotalValidationErrors)
        {
            Assert.AreEqual(expectedTotalValidationErrors, (new Person() { Id = personId }).Validate().Count);
        }
    }
}
