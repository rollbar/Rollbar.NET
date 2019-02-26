#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory(nameof(ClientFixture))]
    public class ClientFixture
    {
        private Client _client;

        [TestInitialize]
        public void SetupFixture()
        {
            this._client = new Client();
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void ClientRenderedWithCpuValue()
        {
            string cpuValue = this._client.Cpu ?? "null";

#if NETFX
            string expected = "{\"cpu\":" + cpuValue + "}";
#else
            string expected = "{\"cpu\":\"" + cpuValue + "\"}";
#endif

            Assert.AreEqual(
                expected,
                JsonConvert.SerializeObject(_client)
                );
        }

        [TestMethod]
        public void ClientRendersArbitraryKeysCorrectly()
        {
            _client["test-key"] = "test-value";
            Assert.IsTrue(JsonConvert.SerializeObject(_client).Contains("\"test-key\":\"test-value\""));
        }

    }
}
