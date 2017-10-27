namespace Rollbar.DTOs
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Newtonsoft.Json;

    public class Trace
        : DtoBase
    {
        public Trace(Frame[] frames, Exception exception)
        {
            if (frames == null)
            {
                throw new ArgumentNullException(nameof(frames));
            }

            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            Frames = frames;
            Exception = exception;
        }

        public Trace(System.Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

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
