namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;

    [TestClass]
    [TestCategory("ServerFixture")]
    public class ServerFixture
    {
        private Server _server;

        [TestInitialize]
        public void SetupFixture()
        {
            this._server = new Server();
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void RequestHostRenderedWhenPresent()
        {
            const string host = "www.rollbar.com";
            _server.Host = host;
            var json = JsonConvert.SerializeObject(_server);
            Assert.IsTrue(json.Contains(host));
            var jObject = JObject.Parse(json);
            Assert.AreEqual(host, jObject["host"]);
        }

        [TestMethod]
        public void RequestRootRenderedWhenPresent()
        {
            const string root = @"C:/inetpub/www/root";
            _server.Root = root;
            var json = JsonConvert.SerializeObject(_server);
            Assert.IsTrue(json.Contains(root));
            var jObject = JObject.Parse(json);
            Assert.AreEqual(root, jObject["root"]);
        }

        [TestMethod]
        public void RequestBranchRenderedWhenPresent()
        {
            const string branch = "master";
            _server.Branch = branch;
            var json = JsonConvert.SerializeObject(_server);
            Assert.IsTrue(json.Contains(branch));
            var jObject = JObject.Parse(json);
            Assert.AreEqual(branch, jObject["branch"]);
        }

        [TestMethod]
        public void RequestCodeVersionRenderedWhenPresent()
        {
            const string codeVersion = "b6437f45b7bbbb15f5eddc2eace4c71a8625da8c";
            _server.CodeVersion = codeVersion;
            var json = JsonConvert.SerializeObject(_server);
            Assert.IsTrue(json.Contains(codeVersion));
            var jObject = JObject.Parse(json);
            Assert.AreEqual(codeVersion, jObject["code_version"]);
        }
    }
}
