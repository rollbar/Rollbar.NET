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

    [TestClass]
    [TestCategory("RequestFixture")]
    public class RequestFixture
    {
        private Request _request;

        [TestInitialize]
        public void SetupFixture()
        {
            this._request = new Request();
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void EmptyRequestRenderedAsEmptyDict()
        {
            Assert.AreEqual("{}", JsonConvert.SerializeObject(_request));
        }

        [TestMethod]
        public void RequestUrlRenderedWhenPresent()
        {
            const string url = "/my/url?search=string";
            _request.Url = url;
            var json = JsonConvert.SerializeObject(_request);
            Assert.IsTrue(json.Contains(url));
            var jObject = JObject.Parse(json);
            Assert.AreEqual(url, jObject["url"]);
        }

        [TestMethod]
        public void RequestMethodRenderedWhenPresent()
        {
            const string method = "whatever";
            _request.Method = method;
            var json = JsonConvert.SerializeObject(_request);
            Assert.IsTrue(json.Contains(method));
            var jObject = JObject.Parse(json);
            Assert.AreEqual(method, jObject["method"]);
        }

        [TestMethod]
        public void RequestHeadersRenderedWhenPresent()
        {
            var headers = new Dictionary<string, string>
            {
                { "Header", "header-value" },
                { "Accept", "json" },
            };
            _request.Headers = headers;
            var json = JsonConvert.SerializeObject(_request);
            Assert.IsTrue(json.Contains("\"Header\":\"header-value\""));
            Assert.IsTrue(json.Contains("\"Accept\":\"json\""));
            JObject jObject = JObject.Parse(json)["headers"] as JObject;

            var left = headers.Values.OrderBy(x => x);
            var right = jObject.Properties().Select(x => x.Value).Values<string>().OrderBy(x => x);
            Assert.AreEqual(left.Count(), right.Count());
            int i = 0;
            while (i < left.Count())
            {
                Assert.AreEqual(left.ElementAt(i), right.ElementAt(i));
                i++;
            }
        }

        [TestMethod]
        public void RequestParamsRenderedWhenPresent()
        {
            var @params = new Dictionary<string, object>
            {
                { "One", (long)1 },
                { "Name", "Chris" },
            };
            _request.Params = @params;
            var json = JsonConvert.SerializeObject(_request);
            Assert.IsTrue(json.Contains("\"One\":1"));
            Assert.IsTrue(json.Contains("\"Name\":\"Chris\""));
            var jObject = JObject.Parse(json)["params"] as JObject;
            Assert.IsNotNull(jObject);

            var left = @params.Keys.OrderBy(x => x);
            var right = jObject.Properties().Select(x => x.Name).OrderBy(x => x);
            Assert.AreEqual(left.Count(), right.Count());
            int i = 0;
            while (i < left.Count())
            {
                Assert.AreEqual(left.ElementAt(i), right.ElementAt(i));
                i++;
            }

            foreach (var kvp in @params)
            {
                Assert.AreEqual(kvp.Value, jObject[kvp.Key].ToObject<object>());
            }
        }

        [TestMethod]
        public void RequestGetParamsRenderedWhenPresent()
        {
            var getParams = new Dictionary<string, object>
            {
                { "One", (long)1 },
                { "Name", "Chris" },
            };
            _request.GetParams = getParams;
            var json = JsonConvert.SerializeObject(_request);
            Assert.IsTrue(json.Contains("\"One\":1"));
            Assert.IsTrue(json.Contains("\"Name\":\"Chris\""));
            var jObject = JObject.Parse(json)["get_params"] as JObject;
            Assert.IsNotNull(jObject);

            var keys = getParams.Keys.OrderBy(x => x);
            var properties = jObject.Properties().Select(x => x.Name).OrderBy(x => x);
            Assert.AreEqual(keys.Count(), properties.Count());
            int i = 0;
            while(i < keys.Count())
            {
                Assert.AreEqual(keys.ElementAt(i), properties.ElementAt(i));
                i++;
            }

            foreach (var kvp in getParams)
            {
                Assert.AreEqual(kvp.Value, jObject[kvp.Key].ToObject<object>());
            }
        }

        [TestMethod]
        public void RequestQueryStringRenderedWhenPresent()
        {
            const string queryString = "whatever";
            _request.QueryString = queryString;
            var json = JsonConvert.SerializeObject(_request);
            Assert.IsTrue(json.Contains(queryString));
            var jObject = JObject.Parse(json);
            Assert.AreEqual(queryString, jObject["query_string"]);
        }

        [TestMethod]
        public void RequestPostParamsRenderedWhenPresent()
        {
            var postParams = new Dictionary<string, object>
            {
                { "One", (long)1 },
                { "Name", "Chris" },
            };
            _request.PostParams = postParams;
            var json = JsonConvert.SerializeObject(_request);
            Assert.IsTrue(json.Contains("\"One\":1"));
            Assert.IsTrue(json.Contains("\"Name\":\"Chris\""));
            var jObject = JObject.Parse(json)["post_params"] as JObject;
            Assert.IsNotNull(jObject);

            var left = postParams.Keys.OrderBy(x => x);
            var right = jObject.Properties().Select(x => x.Name).OrderBy(x => x);
            Assert.AreEqual(left.Count(), right.Count());
            int i = 0;
            while (i < left.Count())
            {
                Assert.AreEqual(left.ElementAt(i), right.ElementAt(i));
                i++;
            }

            foreach (var kvp in postParams)
            {
                Assert.AreEqual(kvp.Value, jObject[kvp.Key].ToObject<object>());
            }
        }

        [TestMethod]
        public void RequestPostBodyRenderedWhenPresent()
        {
            const string postBody = "whatever";
            _request.PostBody = postBody;
            var json = JsonConvert.SerializeObject(_request);
            Assert.IsTrue(json.Contains(postBody));
            var jObject = JObject.Parse(json);
            Assert.AreEqual(postBody, jObject["post_body"]);
        }

        [TestMethod]
        public void RequestUserIpRenderedWhenPresent()
        {
            const string userIp = "whatever";
            _request.UserIp = userIp;
            var json = JsonConvert.SerializeObject(_request);
            Assert.IsTrue(json.Contains(userIp));
            var jObject = JObject.Parse(json);
            Assert.AreEqual(userIp, jObject["user_ip"]);
        }

        [TestMethod]
        public void RequestArbitraryKeyGetsRendered()
        {
            _request["whatever"] = "value";
            Assert.IsTrue(_request.Select(x => x.Key).ToArray().Contains("whatever"));
            var json = JsonConvert.SerializeObject(_request);
            Assert.IsTrue(json.Contains("\"whatever\":\"value\""));
        }
    }
}
