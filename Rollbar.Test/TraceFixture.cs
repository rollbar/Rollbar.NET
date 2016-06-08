using System;
using Xunit;

namespace RollbarDotNet.Test {
    public class TraceFixture {
        [Fact]
        public void Trace_built_from_exception_has_correct_frames() {
            var trace = new Trace(GetException());
            Assert.NotNull(trace.Frames);
            Assert.NotEmpty(trace.Frames);
            Assert.Equal(2, trace.Frames.Length);
            Assert.Equal("RollbarDotNet.Test.TraceFixture.ThrowException()", trace.Frames[0].Method);
            Assert.Equal("RollbarDotNet.Test.TraceFixture.GetException()", trace.Frames[1].Method);
            Assert.All(trace.Frames, frame => Assert.EndsWith("TraceFixture.cs", frame.FileName));
        }

        [Fact]
        public void Trace_built_from_exception_has_frame_and_exception() {
            var trace = new Trace(GetException());
            Assert.NotNull(trace.Exception);
            Assert.NotNull(trace.Frames);
            Assert.NotEmpty(trace.Frames);
        }

        [Fact]
        public void Trace_built_manually_works_correctly() {
            var trace = new Trace(new Frame[0], new Exception("BadClass"));
            Assert.Equal("BadClass", trace.Exception.Class);
            Assert.Empty(trace.Frames);
        }

        [Fact]
        public void Null_frames_not_allowed() {
            Assert.Throws<ArgumentNullException>(() => {
                new Trace(null, new Exception("whatever"));
            });
        }

        [Fact]
        public void Null_rollbar_exception_not_allowed() {
            Assert.Throws<ArgumentNullException>(() => {
                new Trace(new Frame[0], null);
            });
        }

        [Fact]
        public void Null_exception_not_allowed() {
            Assert.Throws<ArgumentNullException>(() => {
                new Trace(null);
            });
        }

        private static System.Exception GetException() {
            try {
                ThrowException();
            }
            catch (System.Exception e) {
                return e;
            }
            throw new System.Exception("Unreachable");
        }

        private static void ThrowException() {
            throw new System.Exception("Bummer");
        }
    }
}
