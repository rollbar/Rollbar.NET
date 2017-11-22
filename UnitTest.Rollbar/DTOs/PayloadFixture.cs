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
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory("PayloadFixture")]
    public class PayloadFixture
    {
        private Payload _exceptionExample;
        private Payload _messageException;
        private Payload _crashException;
        private Payload _aggregateExample;

        [TestInitialize]
        public void SetupFixture()
        {
            this._exceptionExample = new Payload("access-token", new Data("test", new Body(GetException())));
            this._messageException = new Payload("access-token", new Data("test", new Body(new Message("A message I wish to send to the rollbar overlords"))));
            this._crashException = new Payload("access-token", new Data("test", new Body("A terrible crash!")));
            this._aggregateExample = new Payload("access-token", new Data("test", new Body(GetAggregateException())));
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void BasicExceptionCreatesValidRollbarObject()
        {
            var asJson = JObject.Parse(JsonConvert.SerializeObject(_exceptionExample));

            Assert.AreEqual("access-token", asJson["access_token"].Value<string>());

            var data = asJson["data"] as JObject;
            Assert.IsNotNull(data);

            Assert.AreEqual("test", data["environment"].Value<string>());

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
            Assert.AreEqual("UnitTest.Rollbar.DTOs.PayloadFixture.ThrowAnException()", frames[0]["method"].Value<string>());

            var exception = trace["exception"] as JObject;
            Assert.IsNotNull(exception);

            Assert.AreEqual("Test", exception["message"].Value<string>());
            Assert.AreEqual("System.Exception", exception["class"].Value<string>());

            Assert.AreEqual("windows", data["platform"].Value<string>());
            Assert.AreEqual("c#", data["language"].Value<string>());

            var left = _exceptionExample.Data.Notifier.ToArray();
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

            Assert.IsFalse(keys.Contains("level"), "level should not be present");
            Assert.IsFalse(keys.Contains("code_version"), "code_version should not be present");
            Assert.IsFalse(keys.Contains("framework"), "framework should not be present");
            Assert.IsFalse(keys.Contains("context"), "context should not be present");
            Assert.IsFalse(keys.Contains("request"), "request should not be present");
            Assert.IsFalse(keys.Contains("person"), "person should not be present");
            Assert.IsFalse(keys.Contains("server"), "server should not be present");
            Assert.IsFalse(keys.Contains("client"), "client should not be present");
            Assert.IsFalse(keys.Contains("custom"), "custom should not be present");
            Assert.IsFalse(keys.Contains("fingerprint"), "fingerprint should not be present");
            Assert.IsFalse(keys.Contains("title"), "title should not be present");
            Assert.IsFalse(keys.Contains("uuid"), "uuid should not be present");
        }

        [TestMethod]
        public void MessageCreatesValidRollbarObject()
        {
            var asJson = JObject.Parse(JsonConvert.SerializeObject(_messageException));

            Assert.AreEqual("access-token", asJson["access_token"].Value<string>());

            Assert.IsInstanceOfType(asJson["data"], typeof(JObject));
            var data = asJson["data"] as JObject;
            Assert.IsNotNull(data);

            Assert.AreEqual("test", data["environment"].Value<string>());

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

            Assert.AreEqual("windows", data["platform"].Value<string>());
            Assert.AreEqual("c#", data["language"].Value<string>());

            var left = _exceptionExample.Data.Notifier.ToArray();
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

            Assert.IsFalse(keys.Contains("level"), "level should not be present");
            Assert.IsFalse(keys.Contains("code_version"), "code_version should not be present");
            Assert.IsFalse(keys.Contains("framework"), "framework should not be present");
            Assert.IsFalse(keys.Contains("context"), "context should not be present");
            Assert.IsFalse(keys.Contains("request"), "request should not be present");
            Assert.IsFalse(keys.Contains("person"), "person should not be present");
            Assert.IsFalse(keys.Contains("server"), "server should not be present");
            Assert.IsFalse(keys.Contains("client"), "client should not be present");
            Assert.IsFalse(keys.Contains("custom"), "custom should not be present");
            Assert.IsFalse(keys.Contains("fingerprint"), "fingerprint should not be present");
            Assert.IsFalse(keys.Contains("title"), "title should not be present");
            Assert.IsFalse(keys.Contains("uuid"), "uuid should not be present");
        }

        [TestMethod]
        public void TraceChainCreatesValidRollbarObject()
        {
            var asJson = JObject.Parse(JsonConvert.SerializeObject(_aggregateExample));

            Assert.AreEqual("access-token", asJson["access_token"].Value<string>());

            Assert.IsInstanceOfType(asJson["data"], typeof(JObject));
            var data = asJson["data"] as JObject;
            Assert.IsNotNull(data);

            Assert.AreEqual("test", data["environment"].Value<string>());

            Assert.IsInstanceOfType(data["body"], typeof(JObject));
            var body = data["body"] as JObject;
            Assert.IsNotNull(body);

            Assert.IsInstanceOfType(body["trace_chain"], typeof(JArray));
            var traceChain = body["trace_chain"] as JArray;
            Assert.IsNotNull(traceChain);

            Assert.IsNull(body["trace"]);
            Assert.IsNull(body["message"]);
            Assert.IsNull(body["crash_report"]);

            Action<JToken> traceFunc = trace =>
            {
                Assert.IsInstanceOfType(trace["frames"], typeof(JArray));
                var frames = trace["frames"] as JArray;
                Assert.IsNotNull(frames);

                Assert.IsTrue(frames.All(frame => frame["filename"] != null));
                Assert.AreEqual(
                    "UnitTest.Rollbar.DTOs.PayloadFixture.ThrowAnException()", 
                    frames[0]["method"].Value<string>()
                    );

                Assert.IsInstanceOfType(trace["exception"], typeof(JObject));
                var exception = trace["exception"] as JObject;
                Assert.IsNotNull(exception);

                Assert.AreEqual("Test", exception["message"].Value<string>());
                Assert.AreEqual("System.Exception", exception["class"].Value<string>());
            };

            foreach(var t in traceChain)
            {
                traceFunc(t);
            }

            Assert.AreEqual("windows", data["platform"].Value<string>());
            Assert.AreEqual("c#", data["language"].Value<string>());

            //Assert.AreEqual(_exceptionExample.Data.Notifier.ToArray(), data["notifier"].ToObject<Dictionary<string, string>>());
            var left = _exceptionExample.Data.Notifier.ToArray();
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

            Assert.IsFalse(keys.Contains("level"), "level should not be present");
            Assert.IsFalse(keys.Contains("code_version"), "code_version should not be present");
            Assert.IsFalse(keys.Contains("framework"), "framework should not be present");
            Assert.IsFalse(keys.Contains("context"), "context should not be present");
            Assert.IsFalse(keys.Contains("request"), "request should not be present");
            Assert.IsFalse(keys.Contains("person"), "person should not be present");
            Assert.IsFalse(keys.Contains("server"), "server should not be present");
            Assert.IsFalse(keys.Contains("client"), "client should not be present");
            Assert.IsFalse(keys.Contains("custom"), "custom should not be present");
            Assert.IsFalse(keys.Contains("fingerprint"), "fingerprint should not be present");
            Assert.IsFalse(keys.Contains("title"), "title should not be present");
            Assert.IsFalse(keys.Contains("uuid"), "uuid should not be present");
        }

        [TestMethod]
        public void CrashReportCreatesValidRollbarObject()
        {
            var asJson = JObject.Parse(JsonConvert.SerializeObject(_crashException));

            Assert.AreEqual("access-token", asJson["access_token"].Value<string>());

            Assert.IsInstanceOfType(asJson["data"], typeof(JObject));
            var data = asJson["data"] as JObject;
            Assert.IsNotNull(data);

            Assert.AreEqual("test", data["environment"].Value<string>());

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

            Assert.AreEqual("windows", data["platform"].Value<string>());
            Assert.AreEqual("c#", data["language"].Value<string>());

            var left = _exceptionExample.Data.Notifier.ToArray();
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

            Assert.IsFalse(keys.Contains("level"), "level should not be present");
            Assert.IsFalse(keys.Contains("code_version"), "code_version should not be present");
            Assert.IsFalse(keys.Contains("framework"), "framework should not be present");
            Assert.IsFalse(keys.Contains("context"), "context should not be present");
            Assert.IsFalse(keys.Contains("request"), "request should not be present");
            Assert.IsFalse(keys.Contains("person"), "person should not be present");
            Assert.IsFalse(keys.Contains("server"), "server should not be present");
            Assert.IsFalse(keys.Contains("client"), "client should not be present");
            Assert.IsFalse(keys.Contains("custom"), "custom should not be present");
            Assert.IsFalse(keys.Contains("fingerprint"), "fingerprint should not be present");
            Assert.IsFalse(keys.Contains("title"), "title should not be present");
            Assert.IsFalse(keys.Contains("uuid"), "uuid should not be present");
        }

        [TestMethod]
        public void RollbarPayloadCannotHaveNullAccessToken()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var x = new Payload(null, new Data("test", new Body("test")));
            });
        }

        [TestMethod]
        public void RollbarPayloadCannotHaveNullData()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var x = new Payload("test", null);
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
