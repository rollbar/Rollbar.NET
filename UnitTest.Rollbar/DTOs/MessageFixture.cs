#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    [TestClass]
    [TestCategory(nameof(MessageFixture))]
    public class MessageFixture
    {
        private Message _message;

        [TestInitialize]
        public void SetupFixture()
        {
            this._message = new Message("Body of the message");
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void RequiredFieldsSerialization()
        {
            Assert.AreEqual("{\"body\":\"Body of the message\"}", JsonConvert.SerializeObject(_message));
        }

        [TestMethod]
        public void ArbitraryFieldsSerialization()
        {
            var message = new Message("Body of the message");
            message["whatever"] = "fun";

            string json = JsonConvert.SerializeObject(message);

            Assert.IsTrue(
                (json == "{\"whatever\":\"fun\",\"body\":\"Body of the message\"}")
                || (json == "{\"body\":\"Body of the message\",\"whatever\":\"fun\"}")
                );
        }

        [TestMethod]
        public void InitializedArbitraryFieldsSerialization()
        {
            var extraAttributes = new Dictionary<string, object>();
            extraAttributes["whatever"] = "fun";
            var message = new Message("Body of the message", extraAttributes);

            string json = JsonConvert.SerializeObject(message);

            Assert.IsTrue(
                (json == "{\"whatever\":\"fun\",\"body\":\"Body of the message\"}")
                || (json == "{\"body\":\"Body of the message\",\"whatever\":\"fun\"}")
                );
        }

        [TestMethod]
        public void ValidMessageBodyCantBeNullifiedViaIndexer()
        {
            Assert.ThrowsException<ArgumentException>(() => _message["body"] = null);
        }

        [TestMethod]
        public void MessageBodyCantBeSetToIncorrectType()
        {
            Assert.ThrowsException<ArgumentException>(() => _message["body"] = 10);
        }

    }
}
