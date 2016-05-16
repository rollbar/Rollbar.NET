using Newtonsoft.Json;
using Xunit;

namespace Rollbar.Test {
    public class MessageFixture {
        private readonly Message _message;

        public MessageFixture() {
            this._message = new Message("Body of the message");
        }

        [Fact]
        public void Message_has_body() {
            Assert.Equal("{\"body\":\"Body of the message\"}", JsonConvert.SerializeObject(_message));
        }

        [Fact]
        public void Message_has_arbitrary_keys() {
            _message["whatever"] = "fun";
            Assert.Equal("{\"whatever\":\"fun\",\"body\":\"Body of the message\"}", JsonConvert.SerializeObject(_message));
        }

        [Fact]
        public void Message_body_cant_be_overwritten_by_body_indexer() {
            _message["body"] = null;
            Assert.Equal("{\"body\":\"Body of the message\"}", JsonConvert.SerializeObject(_message));
        }

        [Fact]
        public void Message_body_cant_be_set_to_incorrect_type() {
            _message["body"] = 10;
            Assert.Equal("{\"body\":\"Body of the message\"}", JsonConvert.SerializeObject(_message));
        }
    }
}
