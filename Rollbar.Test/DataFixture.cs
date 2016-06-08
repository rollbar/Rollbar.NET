using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using FakeItEasy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RollbarDotNet.Test {
    public class DataFixture {
        private static readonly Regex WhiteSpace = new Regex(@"\s");
        private readonly Data _basicData;

        public DataFixture() {
            this._basicData = new Data("environment", new Body("test")) {Timestamp = null, Platform = null, Language = null};
        }

        [Fact]
        public void Notifier_is_setup() {
            Assert.Contains("name", _basicData.Notifier.Keys);
            Assert.Contains("version", _basicData.Notifier.Keys);
            Assert.Equal(_basicData.Notifier["name"], "Rollbar.NET");
            var version = typeof (Data).Assembly.GetName().Version.ToString(3);
            Assert.Equal(string.Format("{{`name`:`Rollbar.NET`,`version`:`{0}`}}", version), WhiteSpace.Replace(JsonConvert.SerializeObject(_basicData.Notifier), "").Replace('"', '`'));
        }

        [Fact]
        public void Json_does_not_output_null_fields() {
            Assert.Equal(new[] {"body", "environment", "notifier" }, AsJson(_basicData).Properties().Select(x => x.Name).OrderBy(x => x).ToArray());
        }

        [Fact]
        public void Environment_is_mandatory() {
            Assert.Throws<ArgumentNullException>(() => {
                new Data(null, A.Fake<Body>());
            });
        }

        [Fact]
        public void Body_is_mandatory() {
            Assert.Throws<ArgumentNullException>(() => {
                new Data("whatever", null);
            });
        }

        [Fact]
        public void Timestamp_set_when_created() {
            var timestamp = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var data = new Data("whatever", A.Fake<Body>());
            Thread.Sleep(50);
            Assert.True(data.Timestamp - timestamp < 50, "Timestamp was created when it was fetched, not when it was created");
        }

        [Fact]
        public void Platform_defaults_to_windows() {
            var data = new Data("whatever", A.Fake<Body>());
            Assert.Equal("windows", data.Platform);
        }

        [Fact(Skip="Changes global state, breaks other fixtures. Run this by itself. In any case, it works.")]
        //[Fact]
        public void Platform_default_can_be_overriden() {
            var defaultPlatform = Data.DefaultPlatform;
            try {
                Data.DefaultPlatform = "mono";
                var data = new Data("whatever", A.Fake<Body>());
                Assert.Equal("mono", data.Platform);
            }
            finally {
                Data.DefaultPlatform = defaultPlatform;
            }
        }

        [Fact(Skip = "Changes global state, breaks other fixtures. Run this by itself. In any case, it works.")]
        //[Fact]
        public void Language_default_can_be_overriden() {
            var defaultLanguage = Data.DefaultLanguage;
            try {
                Data.DefaultLanguage = "f#";
                var data = new Data("whatever", A.Fake<Body>());
                Assert.Equal("f#", data.Language);
            }
            finally {
                Data.DefaultLanguage = defaultLanguage;
            }
        }

        private static JObject AsJson<T>(T data) {
            return JObject.Parse(JsonConvert.SerializeObject(data));
        }

        private static JToken GetJsonToken<T>(string prop, T data) {
            return AsJson(data)[prop];
        }

        [Fact]
        public void Timestamp_shows_up_in_json() {
            var rollbarData = new Data("env", new Body("test"));
            var timeStamp = rollbarData.Timestamp;
            Assert.Equal(timeStamp, GetJsonToken("timestamp", rollbarData));
            rollbarData.Timestamp = null;
            Assert.Equal(null, GetJsonToken("timestamp", rollbarData));
        }

        [Fact]
        public void Level_shows_up_in_json() {
            _basicData.Level = ErrorLevel.Critical;
            Assert.Equal("critical", GetJsonToken("level", _basicData));
            _basicData.Level = null;
            Assert.Equal(null, GetJsonToken("level", _basicData));
        }

        [Fact]
        public void Code_version_shows_up_in_json() {
            TestOptionalDataProperty("code_version", (rd, v) => rd.CodeVersion = v, "fa31c82012458e897d9ab");
        }

        [Fact]
        public void Platform_shows_up_in_json() {
            TestOptionalDataProperty("platform", (rd, v) => rd.Platform = v, "Mono");
        }

        [Fact]
        public void Language_shows_up_in_json() {
            TestOptionalDataProperty("language", (rd, v) => rd.Language = v, "f#");
        }

        [Fact]
        public void Framework_shows_up_in_json() {
            TestOptionalDataProperty("framework", (rd, v) => rd.Framework = v, "Nancy");
        }

        [Fact]
        public void Context_shows_up_in_json() {
            TestOptionalDataProperty("context", (rd, v) => rd.Context = v, "IndexModule.ctor.GET[/whatever]");
        }

        [Fact]
        public void Request_shows_up_in_json() {
            var rr = new Request();
            _basicData.Request = rr;
            Assert.NotNull(AsJson(_basicData)["request"]);
            Assert.Contains(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            rr.Url = "/my/url";
            Assert.Contains("/my/url", JsonConvert.SerializeObject(_basicData));
            Assert.Contains(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            _basicData.Request = null;
            Assert.DoesNotContain(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            Assert.Null(AsJson(_basicData)["request"]);
        }

        [Fact]
        public void Person_shows_up_in_json() {
            var rp = new Person("person-id");
            _basicData.Person = rp;
            Assert.NotNull(AsJson(_basicData)["person"]);
            Assert.Contains(JsonConvert.SerializeObject(rp), JsonConvert.SerializeObject(_basicData));
            Assert.Contains("person-id", JsonConvert.SerializeObject(_basicData));
            Assert.Contains(JsonConvert.SerializeObject(rp), JsonConvert.SerializeObject(_basicData));
            _basicData.Person = null;
            Assert.DoesNotContain(JsonConvert.SerializeObject(rp), JsonConvert.SerializeObject(_basicData));
            Assert.Null(AsJson(_basicData)["person"]);
        }

        [Fact]
        public void Server_shows_up_in_json() {
            var rr = new Server();
            _basicData.Server = rr;
            Assert.NotNull(AsJson(_basicData)["server"]);
            Assert.Contains(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            rr["url"] = "/my/url";
            Assert.Contains("/my/url", JsonConvert.SerializeObject(_basicData));
            Assert.Contains(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            _basicData.Server = null;
            Assert.DoesNotContain(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            Assert.Null(AsJson(_basicData)["server"]);
        }

        [Fact]
        public void Client_shows_up_in_json() {
            var rr = new Client();
            _basicData.Client = rr;
            Assert.NotNull(AsJson(_basicData)["client"]);
            Assert.Contains(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            rr["url"] = "/my/url";
            Assert.Contains("/my/url", JsonConvert.SerializeObject(_basicData));
            Assert.Contains(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            _basicData.Client = null;
            Assert.DoesNotContain(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            Assert.Null(AsJson(_basicData)["client"]);
        }

        [Fact]
        public void Custom_shows_up_in_json() {
            var rr = new Dictionary<string, object> {};
            _basicData.Custom = rr;
            Assert.NotNull(AsJson(_basicData)["custom"]);
            Assert.Contains(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            rr["url"] = "/my/url";
            Assert.Contains("/my/url", JsonConvert.SerializeObject(_basicData));
            Assert.Contains(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            _basicData.Custom = null;
            Assert.DoesNotContain(JsonConvert.SerializeObject(rr), JsonConvert.SerializeObject(_basicData));
            Assert.Null(AsJson(_basicData)["custom"]);
        }

        [Fact]
        public void Fingerprint_shows_up_in_json() {
            TestOptionalDataProperty("fingerprint", (rd, v) => rd.Fingerprint = v, "blah02349janzvlk239084");
        }

        [Fact]
        public void Title_shows_up_in_json() {
            TestOptionalDataProperty("title", (rd, v) => rd.Title = v, "This will show up as the title of this occurence in Rollbar");
        }

        [Fact]
        public void Uuid_shows_up_in_json() {
            TestOptionalDataProperty("uuid", (rd, v) => rd.Uuid = v, Guid.NewGuid().ToString("N"));
        }

        [Fact]
        public void GuidUpdatesUuid() {
            var rd = new Data("whatever", A.Fake<Body>());
            Assert.Null(rd.Uuid);
            var guid = Guid.NewGuid();
            rd.GuidUuid = guid;
            Assert.Equal(rd.Uuid, guid.ToString("n"));
            Assert.Equal(rd.GuidUuid.Value, guid);
        }

        private void TestOptionalDataProperty<T>(string jsonName, Action<Data, T> set, T value) where T : class {
            set(_basicData, value);
            Assert.Equal(value, GetJsonToken(jsonName, _basicData).Value<T>());
            set(_basicData, null);
            Assert.Equal(null, GetJsonToken(jsonName, _basicData));
        }
    }
}
