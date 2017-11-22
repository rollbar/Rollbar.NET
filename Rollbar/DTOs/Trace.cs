namespace Rollbar.DTOs
{
    using System.Diagnostics;
    using System.Linq;
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;

    public class Trace
        : DtoBase
    {
        public Trace(Frame[] frames, Exception exception)
        {
            Assumption.AssertNotNull(frames, nameof(frames));
            Assumption.AssertNotNull(exception, nameof(exception));

            Frames = frames;
            Exception = exception;
        }

        public Trace(System.Exception exception)
        {
            Assumption.AssertNotNull(exception, nameof(exception));

            var frames = new StackTrace(exception, true).GetFrames() ?? new StackFrame[0];

            Frames = frames.Select(frame => new Frame(frame)).ToArray();
            Exception = new Exception(exception);
        }

        [JsonProperty("frames", Required = Required.Always)]
        public Frame[] Frames { get; private set; }

        [JsonProperty("exception", Required = Required.Always)]
        public Exception Exception { get; private set; }
    }
}
