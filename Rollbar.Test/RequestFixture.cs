using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RollbarDotNet.Test {
    public class RequestFixture {
        private readonly Request _request;

        public RequestFixture() {
            this._request = new Request();
        }

        [Fact]
        public void Empty_request_rendered_as_empty_dict() {
            Assert.Equal("{}", JsonConvert.SerializeObject(_request));
        }

        [Fact]
        public void Request_url_rendered_when_present() {
            const string url = "/my/url?search=string";
            _request.Url = url;
            var json = JsonConvert.SerializeObject(_request);
            Assert.Contains(url, json);
            var jObject = JObject.Parse(json);
            Assert.Equal(url, jObject["url"]);
        }

        [Fact]
        public void Request_method_rendered_when_present() {
            const string method = "whatever";
            _request.Method = method;
            var json = JsonConvert.SerializeObject(_request);
            Assert.Contains(method, json);
            var jObject = JObject.Parse(json);
            Assert.Equal(method, jObject["method"]);
        }

        [Fact]
        public void Request_headers_rendered_when_present() {
            var headers = new Dictionary<string,string> {
                { "Header", "header-value" },
                { "Accept", "json" },
            };
            _request.Headers = headers;
            var json = JsonConvert.SerializeObject(_request);
            Assert.Contains("\"Header\":\"header-value\"", json);
            Assert.Contains("\"Accept\":\"json\"", json);
            JObject jObject = JObject.Parse(json)["headers"] as JObject;
            Assert.Equal(headers.Values.OrderBy(x => x), jObject.Properties().Select( x=> x.Value).Values<string>().OrderBy(x => x));
        }

        [Fact]
        public void Request_params_rendered_when_present() {
            var @params = new Dictionary<string, object> {
                { "One", (long)1 },
                { "Name", "Chris" },
            };
            _request.Params = @params;
            var json = JsonConvert.SerializeObject(_request);
            Assert.Contains("\"One\":1", json);
            Assert.Contains("\"Name\":\"Chris\"", json);
            var jObject = JObject.Parse(json)["params"] as JObject;
            Assert.NotNull(jObject);
            Assert.Equal(@params.Keys.OrderBy(x => x), jObject.Properties().Select(x => x.Name).OrderBy(x => x));
            foreach(var kvp in @params) {
                Assert.Equal(kvp.Value, jObject[kvp.Key].ToObject<object>());
            }
        }

        [Fact]
        public void Request_get_params_rendered_when_present() {
            var getParams = new Dictionary<string, object> {
                { "One", (long)1 },
                { "Name", "Chris" },
            };
            _request.GetParams = getParams;
            var json = JsonConvert.SerializeObject(_request);
            Assert.Contains("\"One\":1", json);
            Assert.Contains("\"Name\":\"Chris\"", json);
            var jObject = JObject.Parse(json)["get_params"] as JObject;
            Assert.NotNull(jObject);
            Assert.Equal(getParams.Keys.OrderBy(x => x), jObject.Properties().Select(x => x.Name).OrderBy(x => x));
            foreach (var kvp in getParams) {
                Assert.Equal(kvp.Value, jObject[kvp.Key].ToObject<object>());
            }
        }

        [Fact]
        public void Request_query_string_rendered_when_present() {
            const string queryString = "whatever";
            _request.QueryString = queryString;
            var json = JsonConvert.SerializeObject(_request);
            Assert.Contains(queryString, json);
            var jObject = JObject.Parse(json);
            Assert.Equal(queryString, jObject["query_string"]);
        }

        [Fact]
        public void Request_post_params_rendered_when_present() {
            var postParams = new Dictionary<string, object> {
                { "One", (long)1 },
                { "Name", "Chris" },
            };
            _request.PostParams = postParams;
            var json = JsonConvert.SerializeObject(_request);
            Assert.Contains("\"One\":1", json);
            Assert.Contains("\"Name\":\"Chris\"", json);
            var jObject = JObject.Parse(json)["post_params"] as JObject;
            Assert.NotNull(jObject);
            Assert.Equal(postParams.Keys.OrderBy(x => x), jObject.Properties().Select(x => x.Name).OrderBy(x => x));
            foreach(var kvp in postParams) {
                Assert.Equal(kvp.Value, jObject[kvp.Key].ToObject<object>());
            }
        }

        [Fact]
        public void Request_post_body_rendered_when_present() {
            const string postBody = "whatever";
            _request.PostBody = postBody;
            var json = JsonConvert.SerializeObject(_request);
            Assert.Contains(postBody, json);
            var jObject = JObject.Parse(json);
            Assert.Equal(postBody, jObject["post_body"]);
        }

        [Fact]
        public void Request_user_ip_rendered_when_present() {
            const string userIp = "whatever";
            _request.UserIp = userIp;
            var json = JsonConvert.SerializeObject(_request);
            Assert.Contains(userIp, json);
            var jObject = JObject.Parse(json);
            Assert.Equal(userIp, jObject["user_ip"]);
        }

        [Fact]
        public void Request_arbitrary_key_gets_rendered() {
            _request["whatever"] = "value";
            Assert.Contains("whatever", _request.Select(x => x.Key).ToArray());
            var json = JsonConvert.SerializeObject(_request);
            Assert.Contains("\"whatever\":\"value\"", json);
        }
    }
}
