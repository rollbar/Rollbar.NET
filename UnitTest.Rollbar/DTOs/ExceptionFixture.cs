#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using dto=global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory(nameof(ExceptionFixture))]
    public class ExceptionFixture
    {
        [TestMethod]
        public void ExceptionCantBeNull()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var rollbarException = new dto.Exception((System.Exception)null);
            });
        }

        [TestMethod]
        public void ExceptionFromExceptionHasClass()
        {
            var rollbarException = new dto.Exception(GetException());
            Assert.AreEqual("System.NotFiniteNumberException", rollbarException.Class);
        }

        [TestMethod]
        public void ExceptionFromExceptionCanHaveMessage()
        {
            var rollbarException = new dto.Exception(GetException())
            {
                Message = "Hello World!",
            };
            Assert.AreEqual("Hello World!", rollbarException.Message);
        }

        [TestMethod]
        public void ExceptionFromExceptionCanHaveDescription()
        {
            var rollbarException = new dto.Exception(GetException())
            {
                Description = "Hello World!",
            };
            Assert.AreEqual("Hello World!", rollbarException.Description);
        }

        [TestMethod]
        public void ExceptionFromClassNameHasClass()
        {
            var rollbarException = new dto.Exception("NotFiniteNumberException");
            Assert.AreEqual("NotFiniteNumberException", rollbarException.Class);
        }

        [TestMethod]
        public void ExceptionFromClassNameCanHaveMesasge()
        {
            var rollbarException = new dto.Exception("NotFiniteNumberException")
            {
                Message = "Hello World!",
            };
            Assert.AreEqual("Hello World!", rollbarException.Message);
        }

        [TestMethod]
        public void ExceptionFromClassNameCanHaveDescription()
        {
            var rollbarException = new dto.Exception("NotFiniteNumberException")
            {
                Description = "Hello World!",
            };
            Assert.AreEqual("Hello World!", rollbarException.Description);
        }

        [TestMethod]
        public void ExceptionSerializesCorrectly()
        {
            var rollbarException = new dto.Exception("Test");
            Assert.AreEqual("{\"class\":\"Test\"}", JsonConvert.SerializeObject(rollbarException));
        }

        [TestMethod]
        public void ExceptoinSerializesMessageCorrectly()
        {
            var rollbarException = new dto.Exception("Test") { Message = "Oops!" };
            Assert.IsTrue(JsonConvert.SerializeObject(rollbarException).Contains("\"message\":\"Oops!\""));
            Assert.IsTrue(JsonConvert.SerializeObject(rollbarException).Contains("\"class\":\"Test\""));
        }

        [TestMethod]
        public void ExceptoinSerializesDescriptionCorrectly()
        {
            var rollbarException = new dto.Exception("Test") { Description = "Oops!" };
            Assert.IsTrue(JsonConvert.SerializeObject(rollbarException).Contains("\"description\":\"Oops!\""));
            Assert.IsTrue(JsonConvert.SerializeObject(rollbarException).Contains("\"class\":\"Test\""));
        }

        private static System.Exception GetException()
        {
            try
            {
                throw new NotFiniteNumberException("Not a Finite Number!");
            }
            catch (System.Exception e)
            {
                return e;
            }
        }
    }
}
