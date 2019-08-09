using System.Collections.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Deploys;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net.Http;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarClientFixture))]
    public class RollbarClientFixture
    {

        private RollbarConfig _loggerConfig;

        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            this._loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void TestPayloadScrubbing()
        {
            string initialPayload = 
                "{\"access_token\":\"17965fa5041749b6bf7095a190001ded\",\"data\":{\"environment\":\"_Rollbar - unit - tests\",\"body\":{\"message\":{\"body\":\"Via log4net\"}},\"level\":\"info\",\"timestamp\":1555443532,\"platform\":\"Microsoft Windows 10.0.17763 \",\"language\":\"c#\",\"framework\":\".NETCoreApp,Version=v2.1\",\"custom\":{\"log4net\":{\"LoggerName\":\"RollbarAppenderFixture\",\"Level\":{\"Name\":\"INFO\",\"Value\":40000,\"DisplayName\":\"INFO\"},\"Message\":\"Via log4net\",\"ThreadName\":\"3\",\"TimeStamp\":\"2019-04-16T12:38:49.0503367-07:00\",\"LocationInfo\":null,\"UserName\":\"NOT AVAILABLE\",\"Identity\":\"NOT AVAILABLE\",\"ExceptionString\":\"\",\"Domain\":\"NOT AVAILABLE\",\"Properties\":{\"log4net:UserName\":\"NOT AVAILABLE\",\"log4net:HostName\":\"wscdellwin\",\"log4net:Identity\":\"NOT AVAILABLE\"},\"TimeStampUtc\":\"2019-04-16T19:38:49.0503367Z\"}},\"uuid\":\"25f57cce37654291a1ea517fb5dfb255\",\"notifier\":{\"name\":\"Rollbar.NET\",\"version\":\"3.0.6\"}}}";

            // scrubbing by names only:
            string[] scrubFields = new string[]
            {
                "log4net:UserName",
                "log4net:HostName",
                "log4net:Identity",
            };
            Assert.IsFalse(initialPayload.Contains("***"));
            string scrubbedPayload_byName = RollbarClient.ScrubPayload(initialPayload, scrubFields);
            Assert.IsTrue(scrubbedPayload_byName.Contains("***"));
            Assert.IsTrue(scrubbedPayload_byName.Contains("\"log4net:UserName\": \"***\""));
            Assert.IsTrue(scrubbedPayload_byName.Contains("\"log4net:HostName\": \"***\""));
            Assert.IsTrue(scrubbedPayload_byName.Contains("\"log4net:Identity\": \"***\""));

            // scrubbing by path only:
            string[] scrubPaths = new string[]
            {
                "data.body.message.body",
                "data.level",
            };
            string scrubbedPayload_byPath = RollbarClient.ScrubPayload(scrubbedPayload_byName, scrubPaths);
            Assert.IsTrue(scrubbedPayload_byPath.Contains("\"body\": \"***\""));
            Assert.IsTrue(scrubbedPayload_byPath.Contains("\"level\": \"***\""));

            // scrubbing by names and by paths:
            List<string> combinedScrubTargets = new List<string>(scrubFields.Length + scrubPaths.Length);
            combinedScrubTargets.AddRange(scrubFields);
            combinedScrubTargets.AddRange(scrubPaths);
            string scrubbedPayload = RollbarClient.ScrubPayload(initialPayload, combinedScrubTargets);
            Assert.AreEqual(scrubbedPayload, scrubbedPayload_byPath);

            //let's test when scrubbed twice:
            scrubbedPayload = RollbarClient.ScrubPayload(scrubbedPayload, combinedScrubTargets);
            Assert.AreEqual(scrubbedPayload, scrubbedPayload_byPath);
        }

    }
}
