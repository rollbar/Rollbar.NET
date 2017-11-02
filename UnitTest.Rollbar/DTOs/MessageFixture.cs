namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [TestClass]
    [TestCategory("MessageFixture")]
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
        public void MessageBodyCantBeNullifiedViaIndexer()
        {
            _message["body"] = null;
            Assert.AreEqual("{\"body\":\"Body of the message\"}", JsonConvert.SerializeObject(_message));
        }

        [TestMethod]
        public void MessageBodyCantBeSetToIncorrectType()
        {
            _message["body"] = 10;
            Assert.AreEqual("{\"body\":\"Body of the message\"}", JsonConvert.SerializeObject(_message));
        }

    }
}
