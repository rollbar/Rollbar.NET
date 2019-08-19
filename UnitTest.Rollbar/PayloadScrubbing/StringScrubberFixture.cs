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
    [TestCategory(nameof(StringScrubberFixture))]
    public class StringScrubberFixture
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
            };
            var scrubber = new StringScrubber(scrubMask, scrubFields);

            string inputToScrub = "{'secret1':'aha', {'data': { 'secret2': 'aha'}}}";
            string scrubbedResult = scrubber.Scrub(inputToScrub);

            Assert.IsFalse(scrubbedResult.Contains(scrubMask));
            Assert.AreEqual("StringScrubber: Data scrubbing failed!", scrubbedResult);
        }

    }
}
