#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using dto=global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    [TestCategory(nameof(TraceFixture))]
    public class TraceFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void TraceBuiltFromExceptionHasCorrectFrames()
        {
            var trace = new dto.Trace(GetException());
            Assert.IsNotNull(trace.Frames);
            Assert.IsTrue(trace.Frames.Length > 0);
            Assert.AreEqual(2, trace.Frames.Length);
            Assert.AreEqual("UnitTest.Rollbar.DTOs.TraceFixture.ThrowException()", trace.Frames[0].Method);
            Assert.AreEqual("UnitTest.Rollbar.DTOs.TraceFixture.GetException()", trace.Frames[1].Method);
            Assert.IsTrue(trace.Frames.All(frame => frame.FileName.EndsWith("TraceFixture.cs") || frame.FileName.EndsWith("TraceFixture")));
        }

        [TestMethod]
        public void TraceBuiltFromExceptionHasFrameAndException()
        {
            var trace = new dto.Trace(GetException());
            Assert.IsNotNull(trace.Exception);
            Assert.IsNotNull(trace.Frames);
            Assert.IsTrue(trace.Frames.Length > 0);
        }

        [TestMethod]
        public void TraceBuiltManuallyWorksCorrectly()
        {
            var trace = new dto.Trace(new dto.Frame[0], new dto.Exception("BadClass"));
            Assert.AreEqual("BadClass", trace.Exception.Class);
            Assert.IsTrue(trace.Frames.Length == 0);
        }

        [TestMethod]
        public void NullFramesNotAllowed()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new dto.Trace(null, new dto.Exception("whatever"));
            });
        }

        [TestMethod]
        public void NullRollbarExceptionNotAllowed()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new dto.Trace(new dto.Frame[0], null);
            });
        }

        [TestMethod]
        public void NullExceptionNotAllowed()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new dto.Trace(null);
            });
        }

        private static System.Exception GetException()
        {
            try
            {
                ThrowException();
            }
            catch (System.Exception e)
            {
                return e;
            }

            throw new System.Exception("Unreachable");
        }

        private static void ThrowException()
        {
            throw new System.Exception("Bummer");
        }
    }
}
