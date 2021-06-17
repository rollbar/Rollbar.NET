#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory(nameof(PayloadFixture))]
    public class PayloadFixture
    {
        private readonly RollbarLoggerConfig _config;

        public PayloadFixture()
        {
            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment);

            this._config = new RollbarLoggerConfig();
            this._config.RollbarDestinationOptions.Reconfigure(destinationOptions);
        }

        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void TestStringPropertiesTruncation()
        {

            Payload[] testPayloads = new Payload[]
            {
                new Payload(this._config.RollbarDestinationOptions.AccessToken, new Data(
                    this._config,
                    new Body(new Message("A message I wish to send to the rollbar overlords", new Dictionary<string, object>() {{"longMessageString", "very-long-string-very-long-string-very-long-" }, {"theMessageNumber", 11 }, })),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
                new Payload(this._config.RollbarDestinationOptions.AccessToken, new Data(
                    this._config,
                    new Body("A terrible crash!"),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
                new Payload(this._config.RollbarDestinationOptions.AccessToken, new Data(
                    this._config,
                    new Body(GetException()),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
                new Payload(this._config.RollbarDestinationOptions.AccessToken, new Data(
                    this._config,
                    new Body(GetAggregateException()),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
            };

            string truncated = null;
            foreach (var testPayload in testPayloads)
            {
                string original = JsonConvert.SerializeObject(testPayload);
                System.Diagnostics.Trace.WriteLine($"Original payload ({original.Length}): " + original);
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                testPayload.TruncateStrings(Encoding.UTF8, 10);
                truncated = JsonConvert.SerializeObject(testPayload);
                System.Diagnostics.Trace.WriteLine($"Truncated payload ({truncated.Length}): " + truncated);

                testPayload.TruncateStrings(Encoding.UTF8, 7);
                truncated = JsonConvert.SerializeObject(testPayload);
                System.Diagnostics.Trace.WriteLine($"Truncated payload ({truncated.Length}): " + truncated);

                testPayload.TruncateStrings(Encoding.UTF8, 0);
                truncated = JsonConvert.SerializeObject(testPayload);
                System.Diagnostics.Trace.WriteLine($"Truncated payload ({truncated.Length}): " + truncated);

                sw.Stop();
                System.Diagnostics.Trace.WriteLine($"Truncation time: {sw.ElapsedMilliseconds} [msec] or {sw.ElapsedTicks} [ticks].");

                Assert.IsTrue(truncated.Length < original.Length);
            }
        }

        [TestMethod]
        public void BasicExceptionCreatesValidRollbarObject()
        {
            var exceptionExample = new Payload(RollbarUnitTestSettings.AccessToken, new Data(this._config, new Body(GetException())));

            var asJson = JObject.Parse(JsonConvert.SerializeObject(exceptionExample));

            Assert.AreEqual(RollbarUnitTestSettings.AccessToken, asJson["access_token"].Value<string>());

            var data = asJson["data"] as JObject;
            Assert.IsNotNull(data);

            Assert.AreEqual(RollbarUnitTestSettings.Environment, data["environment"].Value<string>());

            var body = data["body"] as JObject;
            Assert.IsNotNull(body);

            var trace = body["trace"] as JObject;
            Assert.IsNotNull(trace);

            Assert.IsNull(body["trace_chain"]);
            Assert.IsNull(body["message"]);
            Assert.IsNull(body["crash_report"]);

            var frames = trace["frames"] as JArray;
            Assert.IsNotNull(frames);

            Assert.IsTrue(frames.All( token => token["filename"] != null));

            string[] platformDependentTopFrameMethods = new string[]
            {
                "UnitTest.Rollbar.DTOs.PayloadFixture.ThrowAnException()",
                "UnitTest.Rollbar.DTOs.PayloadFixture.GetException()",
            };
            Assert.IsTrue(platformDependentTopFrameMethods.Contains(frames[0]["method"].Value<string>()));

            var exception = trace["exception"] as JObject;
            Assert.IsNotNull(exception);

            Assert.AreEqual("Test", exception["message"].Value<string>());
            Assert.AreEqual("System.Exception", exception["class"].Value<string>());

            Assert.AreNotEqual("windows", data["platform"].Value<string>());
            Assert.AreEqual("c#", data["language"].Value<string>());

            var left = exceptionExample.Data.Notifier.ToArray();
            var right = data["notifier"].ToObject<Dictionary<string, string>>();

            Assert.AreEqual(left.Length, right.Count);
            int i = 0;
            while (i < right.Count)
            {
                Assert.IsTrue(right.ContainsKey(left[i].Key));
                Assert.AreEqual(right[left[i].Key], left[i].Value);
                i++;
            }

            IEnumerable<string> keys = data.Properties().Select(p => p.Name).ToArray();

            var expectedProperties = new string[] {
                "environment",
                "level",
                "body",
                "timestamp",
                "platform",
                "language",
                "framework",
                "notifier",
                "uuid",
            };
            foreach(var property in expectedProperties)
            {
                Assert.IsTrue(keys.Contains(property), $"{property} should be present");
            }

            var notExpectedProperties = new string[] {
                "code_version",
                "context",
                "request",
                "person",
                "server",
                "client",
                "custom",
                "fingerprint",
                "title",
            };
            foreach (var property in notExpectedProperties)
            {
                Assert.IsFalse(keys.Contains(property), $"{property} should not be present");
            }
        }

        [TestMethod]
        public void MessageCreatesValidRollbarObject()
        {
            var messageException = new Payload(RollbarUnitTestSettings.AccessToken, new Data(this._config, new Body(new Message("A message I wish to send to the rollbar overlords"))));

            var asJson = JObject.Parse(JsonConvert.SerializeObject(messageException));

            Assert.AreEqual(RollbarUnitTestSettings.AccessToken, asJson["access_token"].Value<string>());

            Assert.IsInstanceOfType(asJson["data"], typeof(JObject));
            var data = asJson["data"] as JObject;
            Assert.IsNotNull(data);

            Assert.AreEqual(RollbarUnitTestSettings.Environment, data["environment"].Value<string>());

            Assert.IsInstanceOfType(data["body"], typeof(JObject));
            var body = data["body"] as JObject;
            Assert.IsNotNull(body);

            Assert.IsInstanceOfType(body["message"], typeof(JObject));
            var message = body["message"] as JObject;
            Assert.IsNotNull(message);

            Assert.IsNull(body["trace_chain"]);
            Assert.IsNull(body["trace"]);
            Assert.IsNull(body["crash_report"]);

            Assert.AreEqual("A message I wish to send to the rollbar overlords", message["body"].Value<string>());
            Assert.AreEqual(1, message.Properties().Count());

            Assert.AreNotEqual("windows", data["platform"].Value<string>());
            Assert.AreEqual("c#", data["language"].Value<string>());

            var left = messageException.Data.Notifier.ToArray();
            var right = data["notifier"].ToObject<Dictionary<string, string>>();

            Assert.AreEqual(left.Length, right.Count);
            int i = 0;
            while (i < right.Count)
            {
                Assert.IsTrue(right.ContainsKey(left[i].Key));
                Assert.AreEqual(right[left[i].Key], left[i].Value);
                i++;
            }

            IEnumerable<string> keys = data.Properties().Select(p => p.Name).ToArray();

            var expectedProperties = new string[] {
                "environment",
                "level",
                "body",
                "timestamp",
                "platform",
                "language",
                "framework",
                "notifier",
                "uuid",
            };
            foreach (var property in expectedProperties)
            {
                Assert.IsTrue(keys.Contains(property), $"{property} should be present");
            }

            var notExpectedProperties = new string[] {
                "code_version",
                "context",
                "request",
                "person",
                "server",
                "client",
                "custom",
                "fingerprint",
                "title",
            };
            foreach (var property in notExpectedProperties)
            {
                Assert.IsFalse(keys.Contains(property), $"{property} should not be present");
            }
        }

        [TestMethod]
        public void TraceChainCreatesValidRollbarObject()
        {
            var aggregateExample = new Payload(RollbarUnitTestSettings.AccessToken, new Data(this._config, new Body(GetAggregateException())));

            var asJson = JObject.Parse(JsonConvert.SerializeObject(aggregateExample));

            Assert.AreEqual(RollbarUnitTestSettings.AccessToken, asJson["access_token"].Value<string>());

            Assert.IsInstanceOfType(asJson["data"], typeof(JObject));
            var data = asJson["data"] as JObject;
            Assert.IsNotNull(data);

            Assert.AreEqual(RollbarUnitTestSettings.Environment, data["environment"].Value<string>());

            Assert.IsInstanceOfType(data["body"], typeof(JObject));
            var body = data["body"] as JObject;
            Assert.IsNotNull(body);

            Assert.IsInstanceOfType(body["trace_chain"], typeof(JArray));
            var traceChain = body["trace_chain"] as JArray;
            Assert.IsNotNull(traceChain);

            Assert.IsNull(body["trace"]);
            Assert.IsNull(body["message"]);
            Assert.IsNull(body["crash_report"]);

            void traceFunc(JToken trace)
            {
                Assert.IsInstanceOfType(trace["frames"], typeof(JArray));
                var frames = trace["frames"] as JArray;
                Assert.IsNotNull(frames);

                Assert.IsTrue(frames.All(frame => frame["filename"] != null));

                string[] platformDependentTopFrameMethods = new string[]
                {
                    "ThrowAnException",
                    "GetAggregateException",
                };
                string firstFrameMethod = frames[0]["method"].Value<string>();
                Assert.IsTrue(
                    firstFrameMethod.Contains(platformDependentTopFrameMethods[0]) || firstFrameMethod.Contains(platformDependentTopFrameMethods[1]),
                    firstFrameMethod
                    );

                Assert.IsInstanceOfType(trace["exception"], typeof(JObject));
                var exception = trace["exception"] as JObject;
                Assert.IsNotNull(exception);

                Assert.AreEqual("Test", exception["message"].Value<string>());
                Assert.AreEqual("System.Exception", exception["class"].Value<string>());
            }

            foreach (var t in traceChain)
            {
                traceFunc(t);
            }

            Assert.AreNotEqual("windows", data["platform"].Value<string>());
            Assert.AreEqual("c#", data["language"].Value<string>());

            //Assert.AreEqual(_exceptionExample.Data.Notifier.ToArray(), data["notifier"].ToObject<Dictionary<string, string>>());
            var left = aggregateExample.Data.Notifier.ToArray();
            var right = data["notifier"].ToObject<Dictionary<string, string>>();

            Assert.AreEqual(left.Length, right.Count);
            int i = 0;
            while (i < right.Count)
            {
                Assert.IsTrue(right.ContainsKey(left[i].Key));
                Assert.AreEqual(right[left[i].Key], left[i].Value);
                i++;
            }

            IEnumerable<string> keys = data.Properties().Select(p => p.Name).ToArray();

            var expectedProperties = new string[] {
                "environment",
                "level",
                "body",
                "timestamp",
                "platform",
                "language",
                "framework",
                "notifier",
                "uuid",
            };
            foreach (var property in expectedProperties)
            {
                Assert.IsTrue(keys.Contains(property), $"{property} should be present");
            }

            var notExpectedProperties = new string[] {
                "code_version",
                "context",
                "request",
                "person",
                "server",
                "client",
                "custom",
                "fingerprint",
                "title",
            };
            foreach (var property in notExpectedProperties)
            {
                Assert.IsFalse(keys.Contains(property), $"{property} should not be present");
            }
        }

        [TestMethod]
        public void CrashReportCreatesValidRollbarObject()
        {
            var crashException = new Payload(RollbarUnitTestSettings.AccessToken, new Data(this._config, new Body("A terrible crash!")));


            var asJson = JObject.Parse(JsonConvert.SerializeObject(crashException));

            Assert.AreEqual(RollbarUnitTestSettings.AccessToken, asJson["access_token"].Value<string>());

            Assert.IsInstanceOfType(asJson["data"], typeof(JObject));
            var data = asJson["data"] as JObject;
            Assert.IsNotNull(data);

            Assert.AreEqual(RollbarUnitTestSettings.Environment, data["environment"].Value<string>());

            Assert.IsInstanceOfType(data["body"], typeof(JObject));
            var body = data["body"] as JObject;
            Assert.IsNotNull(body);

            Assert.IsInstanceOfType(body["crash_report"], typeof(JObject));
            var crashReport = body["crash_report"] as JObject;
            Assert.IsNotNull(crashReport);

            Assert.IsNull(body["trace_chain"]);
            Assert.IsNull(body["message"]);
            Assert.IsNull(body["trace"]);

            Assert.AreEqual("A terrible crash!", crashReport["raw"].Value<string>());

            Assert.AreNotEqual("windows", data["platform"].Value<string>());
            Assert.AreEqual("c#", data["language"].Value<string>());

            var left = crashException.Data.Notifier.ToArray();
            var right = data["notifier"].ToObject<Dictionary<string, string>>();

            Assert.AreEqual(left.Length, right.Count);
            int i = 0;
            while (i < right.Count)
            {
                Assert.IsTrue(right.ContainsKey(left[i].Key));
                Assert.AreEqual(right[left[i].Key], left[i].Value);
                i++;
            }

            IEnumerable<string> keys = data.Properties().Select(p => p.Name).ToArray();

            var expectedProperties = new string[] {
                "environment",
                "level",
                "body",
                "timestamp",
                "platform",
                "language",
                "framework",
                "notifier",
                "uuid",
            };
            foreach (var property in expectedProperties)
            {
                Assert.IsTrue(keys.Contains(property), $"{property} should be present");
            }

            var notExpectedProperties = new string[] {
                "code_version",
                "context",
                "request",
                "person",
                "server",
                "client",
                "custom",
                "fingerprint",
                "title",
            };
            foreach (var property in notExpectedProperties)
            {
                Assert.IsFalse(keys.Contains(property), $"{property} should not be present");
            }
        }

        [TestMethod]
        public void RollbarPayloadCannotHaveNullAccessToken()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var x = new Payload(null, new Data(this._config, new Body("test")));
            });
        }

        [TestMethod]
        public void RollbarPayloadCannotHaveNullData()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var x = new Payload(RollbarUnitTestSettings.AccessToken, null);
            });
        }

        private static void ThrowAnException()
        {
            throw new System.Exception("Test");
        }

        private static System.Exception GetException()
        {
            try
            {
                ThrowAnException();
                throw new System.Exception("Unreachable");
            }
            catch (System.Exception e)
            {
                return e;
            }
        }

        private static AggregateException GetAggregateException()
        {
            try
            {
                Parallel.ForEach(new[] { 1, 2 }, i => ThrowAnException());
            }
            catch (AggregateException e)
            {
                return e;
            }

            throw new System.Exception("Unreachable");
        }

    }
}
