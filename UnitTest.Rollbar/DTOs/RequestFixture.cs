#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Principal;

#if NETFX
    using System.ServiceModel.Channels;
    using System.Web;
    using System.Web.SessionState;
#endif

    [TestClass]
    [TestCategory(nameof(RequestFixture))]
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
            var jObject = JObject.Parse(json)["GET"] as JObject;
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
            var jObject = JObject.Parse(json)["POST"] as JObject;
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
            Assert.AreEqual(postBody, jObject["body"]);
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

#if NETFX

        [TestMethod]
        public void RequestUserIPPolicyTest()
        {
            var testCases = new[]
            {
                //(policy: IpAddressCollectionPolicy.Collect, initialUserHost: "10.9.8.7", expectedUserHost: "10.9.8.7"),

                //IPv4:
                new { Policy = IpAddressCollectionPolicy.Collect, InitialUserHost = @"10.9.8.7", ExpectedUserHost = @"10.9.8.7" },
                new { Policy = IpAddressCollectionPolicy.CollectAnonymized, InitialUserHost = @"10.9.8.7", ExpectedUserHost = @"10.9.8.0/24" },
                new { Policy = IpAddressCollectionPolicy.DoNotCollect, InitialUserHost = @"10.9.8.7", ExpectedUserHost = null as string },
                //host name:
                new { Policy = IpAddressCollectionPolicy.CollectAnonymized, InitialUserHost = @"UserHost.com", ExpectedUserHost = @"UserHost.com" },
                //IPv6:
                new { Policy = IpAddressCollectionPolicy.Collect, InitialUserHost = @"2001:0db8:0000:0042:0000:8a2e:0370:7334", ExpectedUserHost = @"2001:0db8:0000:0042:0000:8a2e:0370:7334" },
                new { Policy = IpAddressCollectionPolicy.CollectAnonymized, InitialUserHost = @"2001:0db8:0000:0042:0000:8a2e:0370:7334", ExpectedUserHost = @"2001:0db8:00..." },
                new { Policy = IpAddressCollectionPolicy.DoNotCollect, InitialUserHost = @"2001:0db8:0000:0042:0000:8a2e:0370:7334", ExpectedUserHost = null as string },
            };

            foreach(var testCase in testCases)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Testing: policy = {testCase.Policy}, initialUserHost = {testCase.InitialUserHost}, expectedUserHost = {testCase.ExpectedUserHost}"
                    );
                RequestUserIPPolicyTest(testCase.Policy, testCase.InitialUserHost, testCase.ExpectedUserHost);
            }
        }

        private static void RequestUserIPPolicyTest(
            IpAddressCollectionPolicy policy
            , string initialUserHost
            , string expectedUserHost
            )
        {
            var rollbarConfig = new RollbarConfig(RollbarUnitTestSettings.AccessToken);
            var msg = FakeHttpRequestMessage(initialUserHost);

            rollbarConfig.IpAddressCollectionPolicy = policy;
            var request = new Request(null, rollbarConfig, msg);

            Assert.AreEqual(expectedUserHost, request.UserIp);
        }

        private static HttpRequestMessage FakeHttpRequestMessage(string userHost)
        {
            var msg = new Mock<HttpRequestMessage>();
            HttpContextBase ctx = 
                FakeHttpContext(@"http://www.url.com", userHost, @"http://www.referrer.com");
            msg.Object.Properties.Add("MS_HttpContext", ctx);
            return msg.Object;
        }

        public static HttpContextBase FakeHttpContext(string url, string ip, string referrer)
        {
            Uri uri = new Uri(url);

            var context = new Mock<HttpContextBase>();
            var files = new Mock<HttpFileCollectionBase>();
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session = new Mock<HttpSessionStateBase>();
            var server = new Mock<HttpServerUtilityBase>();

            var user = new Mock<IPrincipal>();
            var identity = new Mock<IIdentity>();

            request.Setup(req => req.ApplicationPath).Returns("~/");
            request.Setup(req => req.AppRelativeCurrentExecutionFilePath).Returns("~/");
            request.Setup(req => req.PathInfo).Returns(string.Empty);
            request.Setup(req => req.Form).Returns(new NameValueCollection());
            request.Setup(req => req.QueryString).Returns(HttpUtility.ParseQueryString(uri.Query));
            request.Setup(req => req.Files).Returns(files.Object);
            request.Setup(req => req.UserHostAddress).Returns(ip);
            request.Setup(req => req.UrlReferrer).Returns(new Uri(referrer));
            request.Setup(req => req.Url).Returns(uri);
            request.Setup(req => req.RawUrl).Returns(url);

            response.Setup(res => res.ApplyAppPathModifier(It.IsAny<string>())).Returns((string virtualPath) => virtualPath);

            user.Setup(usr => usr.Identity).Returns(identity.Object);

            identity.SetupGet(ident => ident.IsAuthenticated).Returns(true);

            context.Setup(ctx => ctx.Request).Returns(request.Object);
            context.Setup(ctx => ctx.Response).Returns(response.Object);
            context.Setup(ctx => ctx.Session).Returns(session.Object);
            context.Setup(ctx => ctx.Server).Returns(server.Object);
            context.Setup(ctx => ctx.User).Returns(user.Object);

            return context.Object;
        }

#endif
    }
}
