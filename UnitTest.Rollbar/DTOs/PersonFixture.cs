#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    [TestClass]
    [TestCategory("PersonFixture")]
    public class PersonFixture
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
        public void PersonIdRenderedCorrectly()
        {
            var rp = new Person("person_id");
            Assert.AreEqual("{\"id\":\"person_id\"}", JsonConvert.SerializeObject(rp));
        }

        [TestMethod]
        public void PersonUsernameRenderedCorrectly()
        {
            var rp = new Person("person_id")
            {
                UserName = "chris_pfohl",
            };
            Assert.AreEqual("{\"id\":\"person_id\",\"username\":\"chris_pfohl\"}", JsonConvert.SerializeObject(rp));
        }

        [TestMethod]
        public void PersonEmailRenderedCorrectly()
        {
            var rp = new Person("person_id")
            {
                Email = "chris@rollbar.com",
            };
            Assert.AreEqual("{\"id\":\"person_id\",\"email\":\"chris@rollbar.com\"}", JsonConvert.SerializeObject(rp));
        }

    }
}
