namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory("BodyFixture")]
    public class BodyFixture
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
        public void ExceptionsCannotBeNull()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var rb = new Body((IEnumerable<System.Exception>)null);
            });
        }

        [TestMethod]
        public void ExceptionsCannotBeEmpty()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var rb = new Body(new System.Exception[0]);
            });
        }

        [TestMethod]
        public void ExceptionCannotBeNull()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var rb = new Body((System.Exception)null);
            });
        }

        [TestMethod]
        public void MessageCannotBeNull()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var rb = new Body((Message)null);
            });
        }

        [TestMethod]
        public void CrashReportCannotBeNull()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var rb = new Body((string)null);
            });
        }

        [TestMethod]
        public void CrashReportCannotBeWhitespace()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var rb = new Body(string.Empty);
            });
        }

        [TestMethod]
        public void CrashReportWorksCorrectly()
        {
            var rollbarBody = new Body("Crash happened");
            var json = JsonConvert.SerializeObject(rollbarBody);
            Assert.AreEqual("{\"crash_report\":{\"raw\":\"Crash happened\"}}", json);
            Assert.IsTrue(!json.Contains("\"trace\":{"));
            Assert.IsTrue(!json.Contains("\"trace_chain\":{"));
            Assert.IsTrue(!json.Contains("\"message\":{"));
        }

        [TestMethod]
        public void MessageWorksCorrectly()
        {
            var rollbarBody = new Body(new Message("Body of the message")
            {
                { "key", "value" }
            });
            var json = JsonConvert.SerializeObject(rollbarBody);
            Assert.IsTrue(json.Contains("{\"message\":{"));
            Assert.IsTrue(json.Contains("\"Body of the message\""));
            Assert.IsTrue(json.Contains("\"key\":\"value\""));
            Assert.IsTrue(!json.Contains("\"trace\":{"));
            Assert.IsTrue(!json.Contains("\"trace_chain\":{"));
            Assert.IsTrue(!json.Contains("\"crash_report\":{"));
        }

        [TestMethod]
        public void TraceWorksCorrectly()
        {
            var rollbarBody = new Body(GetException());
            var json = JsonConvert.SerializeObject(rollbarBody);
            Assert.IsTrue(json.Contains("\"trace\":{"));
            Assert.IsTrue(json.Contains("\"UnitTest.Rollbar.DTOs.BodyFixture.GetException()\""));
            Assert.IsTrue(!json.Contains("\"crash_report\":{"));
            Assert.IsTrue(!json.Contains("\"trace_chain\":{"));
            Assert.IsTrue(!json.Contains("\"message\":{"));
        }

        [TestMethod]
        public void TraceChainWorksCorrectly()
        {
            var rollbarBody = new Body(GetAggregateException());
            var json = JsonConvert.SerializeObject(rollbarBody);
            Assert.IsTrue(json.Contains("\"trace_chain\":["));
            Assert.IsTrue(json.Contains("\"UnitTest.Rollbar.DTOs.BodyFixture.ThrowException()\""));
            Assert.IsTrue(!json.Contains("\"crash_report\":{"));
            Assert.IsTrue(!json.Contains("\"trace\":{"));
            Assert.IsTrue(!json.Contains("\"message\":{"));
        }

        [TestMethod]
        public void ExceptionWithInnerExceptionsGenerateTraceChain()
        {
            var exception = new System.Exception("test", new System.Exception("inner exception"));
            var rollbarBody = new Body(exception);
            var json = JsonConvert.SerializeObject(rollbarBody);
            Assert.IsTrue(json.Contains("\"trace_chain\""));
            Assert.IsTrue(!json.Contains("\"crash_report\":{"));
            Assert.IsTrue(!json.Contains("\"trace\":{"));
            Assert.IsTrue(!json.Contains("\"message\":{"));
        }

        private static AggregateException GetAggregateException()
        {
            try
            {
                Parallel.ForEach(new[] { 1, 2 }, i => ThrowException());
            }
            catch (AggregateException e)
            {
                return e;
            }

            throw new System.Exception("Unreachable");
        }

        private static System.Exception GetException()
        {
            try
            {
                ThrowException();
            }
            catch (System.Exception e)
            {
                return e;
            }

            throw new System.Exception("Unreachable");
        }

        private static void ThrowException()
        {
            throw new System.Exception("Oops");
        }
    }
}
