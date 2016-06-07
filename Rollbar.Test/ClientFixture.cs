using Newtonsoft.Json;
using Xunit;

namespace RollbarDotNet.Test {
    public class ClientFixture {
        private readonly Client _client;

        public ClientFixture() {
            this._client= new Client();
        }

        [Fact]
        public void Client_rendered_as_dict_when_empty() {
            Assert.Equal("{}", JsonConvert.SerializeObject(_client));
        }

        [Fact]
        public void Client_renders_arbitrary_keys_correctly() {
            _client["test-key"] = "test-value";
            Assert.Equal("{\"test-key\":\"test-value\"}", JsonConvert.SerializeObject(_client));
        }

        [Fact]
        public void Client_renders_javascript_entry_correctly() {
            _client.Javascript = new JavascriptClient();
            Assert.Equal("{\"javascript\":{}}", JsonConvert.SerializeObject(_client));
        }
    }
}
