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

            int[] platformDependentFrameCount = new int[]
            {
                2,
                1,
            };
            Assert.IsTrue(platformDependentFrameCount.Contains(trace.Frames.Length));

            string[] platformDependentTopFrameMethods = new string[]
            {
                "UnitTest.Rollbar.DTOs.TraceFixture.ThrowException()",
                "UnitTest.Rollbar.DTOs.TraceFixture.GetException()",
            };
            Assert.IsTrue(
                platformDependentTopFrameMethods.Contains(trace.Frames[0].Method), 
                trace.Frames[0].Method
                );

            //Assert.IsTrue(trace.Frames.All(frame => frame.FileName.EndsWith("TraceFixture.cs") || frame.FileName.EndsWith("TraceFixture")), "file names");
            Assert.IsTrue(
                trace.Frames.All(frame => frame.FileName.EndsWith("TraceFixture.cs") || frame.FileName.EndsWith("TraceFixture") || (string.Compare(frame.FileName, "(unknown)") == 0)),
                trace.Frames.Select(frame => frame.FileName).Aggregate((fileName, next) => next + ", " + fileName)
                );
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
                new dto.Trace(null as Exception);
            });
        }

        [TestMethod]
        public void TraceFromCallStackString()
        {
            string sampleCallStackString = @"
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
   at System.Environment.get_StackTrace()
   at System.Diagnostics.TraceEventCache.get_Callstack()
   at System.Diagnostics.TraceListener.WriteFooter(TraceEventCache eventCache)
   at System.Diagnostics.TraceListener.TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, Int32 id, String message)
   at System.Diagnostics.TraceInternal.TraceEvent(TraceEventType eventType, Int32 id, String format, Object[] args)
   at System.Diagnostics.Trace.TraceError(String message)
   at Prototype.RollbarTraceListener.Program.Main(String[] args) in C:\GitHub\WSCLLC\Rollbar.NET\Prototype.RollbarTraceListener\Prototype.RollbarTraceListener\Program.cs:line 17
";
            dto.Trace trace = new dto.Trace(sampleCallStackString, "System.Exception: Azohen-way!");

            Assert.AreEqual(8, trace.Frames.Length);
            Assert.IsNotNull(trace.Exception);
            Assert.AreEqual("System.Exception", trace.Exception.Class);
            Assert.AreEqual("Azohen-way!", trace.Exception.Message);
            Assert.IsNull(trace.Exception.Description);
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
