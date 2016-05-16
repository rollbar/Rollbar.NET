using System;
using Newtonsoft.Json;
using Xunit;

namespace Rollbar.Test {
    public class ExceptionFixture {
        [Fact]
        public void Exception_cant_be_null() {
            Assert.Throws<ArgumentNullException>(() => {
                var rollbarException = new Exception((System.Exception) null);
            });
        }

        [Fact]
        public void Exception_from_exception_has_class() {
            var rollbarException = new Exception(GetException());
            Assert.Equal("System.NotFiniteNumberException", rollbarException.Class);
        }

        [Fact]
        public void Exception_from_exception_can_have_message() {
            var rollbarException = new Exception(GetException()) {
                Message = "Hello World!",
            };
            Assert.Equal("Hello World!", rollbarException.Message);
        }

        [Fact]
        public void Exception_from_exception_can_have_description() {
            var rollbarException = new Exception(GetException()) {
                Description = "Hello World!",
            };
            Assert.Equal("Hello World!", rollbarException.Description);
        }

        [Fact]
        public void Exception_from_class_name_has_class() {
            var rollbarException = new Exception("NotFiniteNumberException");
            Assert.Equal("NotFiniteNumberException", rollbarException.Class);
        }

        [Fact]
        public void Exception_from_class_name_can_have_mesasge() {
            var rollbarException = new Exception("NotFiniteNumberException") {
                Message = "Hello World!",
            };
            Assert.Equal("Hello World!", rollbarException.Message);
        }

        [Fact]
        public void Exception_from_class_name_can_have_description() {
            var rollbarException = new Exception("NotFiniteNumberException") {
                Description = "Hello World!",
            };
            Assert.Equal("Hello World!", rollbarException.Description);
        }

        [Fact]
        public void Exception_serializes_correctly() {
            var rollbarException = new Exception("Test");
            Assert.Equal("{\"class\":\"Test\"}", JsonConvert.SerializeObject(rollbarException));
        }

        [Fact]
        public void Exceptoin_serializes_message_correctly() {
            var rollbarException = new Exception("Test") {Message = "Oops!"};
            Assert.Contains("\"message\":\"Oops!\"", JsonConvert.SerializeObject(rollbarException));
            Assert.Contains("\"class\":\"Test\"", JsonConvert.SerializeObject(rollbarException));
        }

        [Fact]
        public void Exceptoin_serializes_description_correctly() {
            var rollbarException = new Exception("Test") { Description = "Oops!" };
            Assert.Contains("\"description\":\"Oops!\"", JsonConvert.SerializeObject(rollbarException));
            Assert.Contains("\"class\":\"Test\"", JsonConvert.SerializeObject(rollbarException));
        }

        private static System.Exception GetException() {
            try {
                throw new NotFiniteNumberException("Not a Finite Number!");
            }
            catch (System.Exception e) {
                return e;
            }
        }
    }
}
