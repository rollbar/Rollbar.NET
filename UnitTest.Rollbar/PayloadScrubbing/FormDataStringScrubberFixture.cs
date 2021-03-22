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
    [TestCategory(nameof(FormDataStringScrubberFixture))]
    public class FormDataStringScrubberFixture
    {

        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        private const string contentTypeHeaderValue =
            "multipart/form-data; boundary=---------------------------8721656041911415653955004498";
        private const string partBoundary = "---------------------------8721656041911415653955004498";
        private const string scrubMask = "***";

        private TestCase<string, string> BuildTestCase()
        {

            StringBuilder input = new StringBuilder();
            input.AppendLine(partBoundary);
            input.AppendLine("Content-Disposition: form-data; name=\"myPassword\"");
            input.AppendLine("");
            input.AppendLine("Test");
            input.AppendLine(partBoundary);
            input.AppendLine("Content-Disposition: form-data; name=\"myCheckBox\"");
            input.AppendLine("");
            input.AppendLine("on");
            input.AppendLine(partBoundary);
            input.AppendLine("Content-Disposition: form-data; name=\"mySecretFile\"; filename=\"test.txt\"");
            input.AppendLine("Content-Type: text/plain");
            input.AppendLine("");
            input.AppendLine("Some secret file content...");
            input.AppendLine("Some other secret file content...");
            input.AppendLine(partBoundary + "--");

            StringBuilder expectedOutput = new StringBuilder();
            expectedOutput.AppendLine(partBoundary);
            expectedOutput.AppendLine("Content-Disposition: form-data; name=\"myPassword\"");
            expectedOutput.AppendLine("");
            expectedOutput.AppendLine(scrubMask);
            expectedOutput.AppendLine(partBoundary);
            expectedOutput.AppendLine("Content-Disposition: form-data; name=\"myCheckBox\"");
            expectedOutput.AppendLine("");
            expectedOutput.AppendLine("on");
            expectedOutput.AppendLine(partBoundary);
            expectedOutput.AppendLine("Content-Disposition: form-data; name=\"mySecretFile\"; filename=\"test.txt\"");
            expectedOutput.AppendLine("Content-Type: text/plain");
            expectedOutput.AppendLine("");
            expectedOutput.AppendLine(scrubMask);
            expectedOutput.AppendLine(partBoundary + "--");

            return new TestCase<string, string>(input.ToString(), expectedOutput.ToString());
        }

        [TestMethod]
        public void BasicTest()
        {
            string[] scrubFields = new[]
            {
                "myPassword",
                "mySecretFile",
                "secret3",
            };
            var scrubber = new FormDataStringScrubber(contentTypeHeaderValue, scrubMask, scrubFields);

            string scrubbedResult = scrubber.Scrub("<This is not a form-data>");
            Assert.IsTrue(scrubbedResult.StartsWith("FormDataStringScrubber: Data scrubbing failed!"), "Properly handles mis-formatted form-data.");

            var testCase = BuildTestCase();
            scrubbedResult = scrubber.Scrub(testCase.Input);
            Assert.AreEqual(testCase.ExpectedOutput, scrubbedResult);

        }
    }
}
