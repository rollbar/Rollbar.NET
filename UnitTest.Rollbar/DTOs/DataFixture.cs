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
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory(nameof(DataFixture))]
    public class DataFixture
    {
        private static readonly Regex WhiteSpace = new Regex(@"\s");
        private readonly RollbarConfig _config;
        private readonly Data _basicData;  

        public DataFixture()
        {
            this._config = new RollbarConfig(RollbarUnitTestSettings.AccessToken)
            {
                Environment = "test",
            };

            this._basicData = new Data(this._config,
            new Body("test"))
            {
                Timestamp = null,
                Platform = null,
                Language = null
            };
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
        public void NotifierIsSet()
        {
            Assert.IsTrue(_basicData.Notifier.Keys.Contains("name"));
            Assert.IsTrue(_basicData.Notifier.Keys.Contains("version"));
            Assert.AreEqual(_basicData.Notifier["name"], "Rollbar.NET");
            var version = typeof(Data).Assembly.GetName().Version.ToString(3);
            Assert.AreEqual(string.Format("{{`name`:`Rollbar.NET`,`version`:`{0}`}}", version), WhiteSpace.Replace(JsonConvert.SerializeObject(_basicData.Notifier), "").Replace('"', '`'));
        }

        [TestMethod]
        public void JsonDoesNotOutputNullFields()
        {
            var props = AsJson(_basicData).Properties().Select(x => x.Name).OrderBy(x => x).ToArray();
            var expected = new[] { "body", "environment", "framework", "level", "notifier", "uuid" };

            Assert.AreEqual(expected.Length, props.Length);
            int i = 0;
            while(i < expected.Length)
            {
                Assert.AreEqual(expected[i], props[i]);
                i++;
            }
        }

        [TestMethod]
        public void EnvironmentIsMandatory()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new Data(null, new Body(new System.Exception("Oops.")));
            });
        }

        [TestMethod]
        public void BodyIsMandatory()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new Data(this._config, null);
            });
        }

        [TestMethod]
        public void TimestampSetWhenCreated()
        {
            var timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var data = new Data(this._config, new Body(new System.Exception("Oops.")));
            Thread.Sleep(50);
            Assert.IsTrue(data.Timestamp - timestamp < 50, "Timestamp was created when it was fetched, not when it was created");
        }

        [TestMethod]
        public void PlatformDefaultsToWindows()
        {
            var data = new Data(this._config, new Body(new System.Exception("Oops.")));
            Assert.AreEqual("windows", data.Platform);
        }

        [TestMethod]
        public void PlatformDefaultCanBeOverriden()
        {
            var defaultPlatform = Data.DefaultPlatform;
            try
            {
                Data.DefaultPlatform = "mono";
                var data = new Data(this._config, new Body(new System.Exception("Oops.")));
                Assert.AreEqual("mono", data.Platform);
            }
            finally
            {
                Data.DefaultPlatform = defaultPlatform;
            }
        }

        [TestMethod]
        public void LanguageDefaultCanBeOverriden()
        {
            var defaultLanguage = Data.DefaultLanguage;
            try
            {
                Data.DefaultLanguage = "f#";
                var data = new Data(this._config, new Body(new System.Exception("Oops.")));
                Assert.AreEqual("f#", data.Language);
            }
            finally
            {
                Data.DefaultLanguage = defaultLanguage;
            }
        }

        private static JObject AsJson<T>(T data)
        {
            return JObject.Parse(JsonConvert.SerializeObject(data));
        }

        private static JToken GetJsonToken<T>(string prop, T data)
        {
            return AsJson(data)[prop];
        }

        [TestMethod]
        public void TimestampShowsUpInJson()
        {
            var rollbarData = new Data(this._config, new Body("test"));
            var timeStamp = rollbarData.Timestamp;
            Assert.AreEqual(timeStamp, GetJsonToken("timestamp", rollbarData));
            rollbarData.Timestamp = null;
            Assert.AreEqual(null, GetJsonToken("timestamp", rollbarData));
        }

        [TestMethod]
        public void LevelShowsUpInJson()
        {
            _basicData.Level = ErrorLevel.Critical;
            Assert.AreEqual("critical", GetJsonToken("level", _basicData));
            _basicData.Level = null;
            Assert.AreEqual(null, GetJsonToken("level", _basicData));
        }

        [TestMethod]
        public void CodeVersionShowsUpInJson()
        {
            TestOptionalDataProperty("code_version", (rd, v) => rd.CodeVersion = v, "fa31c82012458e897d9ab");
        }

        [TestMethod]
        public void PlatformShowsUpInJson()
        {
            TestOptionalDataProperty("platform", (rd, v) => rd.Platform = v, "Mono");
        }

        [TestMethod]
        public void LanguageShowsUpInJson()
        {
            TestOptionalDataProperty("language", (rd, v) => rd.Language = v, "f#");
        }

        [TestMethod]
        public void FrameworkShowsUpInJson()
        {
            TestOptionalDataProperty("framework", (rd, v) => rd.Framework = v, "Nancy");
        }

        [TestMethod]
        public void ContextShowsUpInJson()
        {
            TestOptionalDataProperty("context", (rd, v) => rd.Context = v, "IndexModule.ctor.GET[/whatever]");
        }

        [TestMethod]
        public void RequestShowsUpInJson()
        {
            var rr = new Request();
            _basicData.Request = rr;
            Assert.IsNotNull(AsJson(_basicData)["request"]);
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            rr.Url = "/my/url";
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains("/my/url"));
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            _basicData.Request = null;
            Assert.IsTrue(!JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            Assert.IsNull(AsJson(_basicData)["request"]);
        }

        [TestMethod]
        public void PersonShowsUpInJson()
        {
            var rp = new Person("person-id");
            _basicData.Person = rp;
            Assert.IsNotNull(AsJson(_basicData)["person"]);
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rp)));
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains("person-id"));
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rp)));
            _basicData.Person = null;
            Assert.IsTrue(!JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rp)));
            Assert.IsNull(AsJson(_basicData)["person"]);
        }

        [TestMethod]
        public void ServerShowsUpInJson()
        {
            var rr = new Server();
            _basicData.Server = rr;
            Assert.IsNotNull(AsJson(_basicData)["server"]);
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            rr["url"] = "/my/url";
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains("/my/url"));
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            _basicData.Server = null;
            Assert.IsTrue(!JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            Assert.IsNull(AsJson(_basicData)["server"]);
        }

        [TestMethod]
        public void ClientShowsUpInJson()
        {
            var rr = new Client();
            _basicData.Client = rr;
            Assert.IsNotNull(AsJson(_basicData)["client"]);
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            rr["url"] = "/my/url";
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains("/my/url"));
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            _basicData.Client = null;
            Assert.IsTrue(!JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            Assert.IsNull(AsJson(_basicData)["client"]);
        }

        [TestMethod]
        public void CustomShowsUpInJson()
        {
            var rr = new Dictionary<string, object> { };
            _basicData.Custom = rr;
            Assert.IsNotNull(AsJson(_basicData)["custom"]);
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            rr["url"] = "/my/url";
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains("/my/url"));
            Assert.IsTrue(JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            _basicData.Custom = null;
            Assert.IsTrue(!JsonConvert.SerializeObject(_basicData).Contains(JsonConvert.SerializeObject(rr)));
            Assert.IsNull(AsJson(_basicData)["custom"]);
        }

        [TestMethod]
        public void FingerprintShowsUpInJson()
        {
            TestOptionalDataProperty("fingerprint", (rd, v) => rd.Fingerprint = v, "blah02349janzvlk239084");
        }

        [TestMethod]
        public void TitleShowsUpInJson()
        {
            TestOptionalDataProperty("title", (rd, v) => rd.Title = v, "This will show up as the title of this occurence in Rollbar");
        }

        [TestMethod]
        public void UuidShowsUpInJson()
        {
            TestOptionalDataProperty("uuid", (rd, v) => rd.Uuid = v, Guid.NewGuid().ToString("N"));
        }

        [TestMethod]
        public void GuidSetAndUpdatesUuid()
        {
            var rd = new Data(this._config, new Body(new System.Exception("Oops")));
            Assert.IsTrue(rd.GuidUuid.HasValue);
            Assert.AreEqual(rd.Uuid, rd.GuidUuid.Value.ToString("n"));
        }

        private void TestOptionalDataProperty<T>(string jsonName, Action<Data, T> set, T value) where T : class
        {
            set(_basicData, value);
            Assert.AreEqual(value, GetJsonToken(jsonName, _basicData).Value<T>());
            set(_basicData, null);
            Assert.AreEqual(null, GetJsonToken(jsonName, _basicData));
        }

    }
}
