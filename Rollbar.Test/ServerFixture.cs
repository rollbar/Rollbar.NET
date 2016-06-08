using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RollbarDotNet.Test {
    public class ServerFixture {
        private readonly Server _server;

        public ServerFixture() {
            this._server = new Server();
        }

        [Fact]
        public void Request_host_rendered_when_present() {
            const string host = "www.rollbar.com";
            _server.Host = host;
            var json = JsonConvert.SerializeObject(_server);
            Assert.Contains(host, json);
            var jObject = JObject.Parse(json);
            Assert.Equal(host, jObject["host"]);
        }

        [Fact]
        public void Request_root_rendered_when_present() {
            const string root = @"C:/inetpub/www/root";
            _server.Root = root;
            var json = JsonConvert.SerializeObject(_server);
            Assert.Contains(root, json);
            var jObject = JObject.Parse(json);
            Assert.Equal(root, jObject["root"]);
        }

        [Fact]
        public void Request_branch_rendered_when_present() {
            const string branch = "master";
            _server.Branch = branch;
            var json = JsonConvert.SerializeObject(_server);
            Assert.Contains(branch, json);
            var jObject = JObject.Parse(json);
            Assert.Equal(branch, jObject["branch"]);
        }

        [Fact]
        public void Request_code_version_rendered_when_present() {
            const string codeVersion = "b6437f45b7bbbb15f5eddc2eace4c71a8625da8c";
            _server.CodeVersion = codeVersion;
            var json = JsonConvert.SerializeObject(_server);
            Assert.Contains(codeVersion, json);
            var jObject = JObject.Parse(json);
            Assert.Equal(codeVersion, jObject["code_version"]);
        }
    }
}
