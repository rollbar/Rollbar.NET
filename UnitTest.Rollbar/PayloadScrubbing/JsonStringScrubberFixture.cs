#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.PayloadScrubbing
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using global::Rollbar.PayloadScrubbing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory(nameof(JsonStringScrubberFixture))]
    public class JsonStringScrubberFixture
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
        public void BasicTest()
        {
            const string scrubMask = "***";
            string[] scrubFields = new[]
            {
                "secret1",
                "data.secret2",
                "secret3",
            };
            var scrubber = new JsonStringScrubber(scrubMask, scrubFields);

            Assert.AreEqual("JsonStringScrubber: Data scrubbing failed!", scrubber.Scrub("<This is not Json>"), "Properly handles mis-formatted JSON.");

            string inputToScrub = "{\"not_secret\":123,\"secret1\":\"aha1\",\"data\": { \"secret2\":\"aha2\"}}";
            string scrubbedResult = scrubber.Scrub(inputToScrub);

            Assert.IsTrue(scrubbedResult.Contains(scrubMask), "Scrubbed at least something.");
            Assert.IsTrue(scrubbedResult.Contains("123"), "Did not touch wrong field.");
            Assert.IsTrue(scrubbedResult.Contains("\"secret1\": \"***\""), "Scrubbed by field name.");
            Assert.IsTrue(scrubbedResult.Contains("\"secret2\": \"***\""), "Scrubbed by field path.");
        }
    }
}
