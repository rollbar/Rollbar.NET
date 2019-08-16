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
    [TestCategory(nameof(XmlStringScrubberFixture))]
    public class XmlStringScrubberFixture
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
                "body.data.@secret2",
                "body.data1.secret2",
                "@supersecret",
                "secret3",
            };
            var scrubber = new XmlStringScrubber(scrubMask, scrubFields);

            Assert.AreEqual("XmlStringScrubber: Data scrubbing failed!", scrubber.Scrub("{This is not XML}"), "Properly handles mis-formatted XML.");

            string inputToScrub = "<body><not_secret>123</not_secret><secret1>aha1</secret1><data secret2=\"aha2\" /><data1 supersecret=\"SUPER\"><secret2>\"aha12\"</secret2></data1></body>";
            string scrubbedResult = scrubber.Scrub(inputToScrub);

            Assert.IsTrue(scrubbedResult.Contains(scrubMask), "Scrubbed at least something.");
            Assert.IsTrue(scrubbedResult.Contains("<not_secret>123</not_secret>"), "Did not touch wrong field.");
            Assert.IsTrue(scrubbedResult.Contains("<secret1>***</secret1>"), "Scrubbed by element name.");
            Assert.IsTrue(scrubbedResult.Contains("<data secret2=\"***\" />"), "Scrubbed by attribute path.");
            Assert.IsTrue(scrubbedResult.Contains("<data1 supersecret=\"***\">"), "Scrubbed by attribute name.");
            Assert.IsTrue(scrubbedResult.Contains("<secret2>***</secret2>"), "Scrubbed by element path.");
        }
    }
}
