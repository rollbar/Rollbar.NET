using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RollbarDotNet.Test 
{
    public class JavascriptClientFixture 
    {
        private readonly JavascriptClient _jsClient;

        public JavascriptClientFixture() 
        {
            this._jsClient = new JavascriptClient();
        }

        [Fact]
        public void Empty_request_rendered_as_empty_dict() 
        {
            Assert.Equal("{}", JsonConvert.SerializeObject(_jsClient));
        }

        [Fact]
        public void JsClient_browser_rendered_when_present() 
        {
            const string browser = "chromex64";
            _jsClient.Browser = browser;
            var json = JsonConvert.SerializeObject(_jsClient);
            Assert.Contains(browser, json);
            var jObject = JObject.Parse(json);
            Assert.Equal(browser, jObject["browser"]);
        }

        [Fact]
        public void JsClient_code_version_rendered_when_present() 
        {
            const string codeVersion = "6846d84aecf68d46d8acease684cfe86a6es84cf";
            _jsClient.CodeVersion = codeVersion;
            var json = JsonConvert.SerializeObject(_jsClient);
            Assert.Contains(codeVersion, json);
            var jObject = JObject.Parse(json);
            Assert.Equal(codeVersion, jObject["code_version"]);
        }

        [Fact]
        public void JsClient_source_map_enabled_rendered_when_present() 
        {
            const bool sourceMapEnabled = true;
            _jsClient.SourceMapEnabled = sourceMapEnabled;
            var json = JsonConvert.SerializeObject(_jsClient);
            Assert.Contains("true", json);
            var jObject = JObject.Parse(json);
            Assert.Equal(sourceMapEnabled, jObject["source_map_enabled"]);
        }

        [Fact]
        public void JsClient_guess_uncaught_frames_rendered_when_present() 
        {
            const bool guessUncaughtFrames = false;
            _jsClient.GuessUncaughtFrames = guessUncaughtFrames;
            var json = JsonConvert.SerializeObject(_jsClient);
            Assert.Contains("false", json);
            var jObject = JObject.Parse(json);
            Assert.Equal(guessUncaughtFrames, jObject["guess_uncaught_frames"]);
        }

        [Fact]
        public void JsClient_can_have_arbitrary_keys() 
        {
            const string browser = "chromex64";
            _jsClient.Browser = browser;
            _jsClient["whatever"] = "test";
            var json = JsonConvert.SerializeObject(_jsClient);
            var jObject = JObject.Parse(json);
            Assert.Equal(browser, jObject["browser"]);
            Assert.Equal("test", jObject["whatever"]);
        }
    }
}
