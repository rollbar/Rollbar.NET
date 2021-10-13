namespace Rollbar.DTOs
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Models Rollbar Trace DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    public class Trace
        : DtoBase
    {
        private static readonly TraceSource traceSource = new TraceSource(typeof(Trace).FullName ?? "Trace");

        internal Trace(string callStack, string exceptionInfo)
        {
            Assumption.AssertNotNullOrEmpty(callStack, nameof(callStack));
            Assumption.AssertNotNullOrEmpty(exceptionInfo, nameof(exceptionInfo));

            string[] entries = callStack.Split(new [] { Environment.NewLine,}, StringSplitOptions.None);
            List<DTOs.Frame> frames = new List<Frame>(entries.Length);
            foreach(var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry))
                {
                    continue;
                }

                frames.Add(new DTOs.Frame(entry));
            }
            this.Frames = frames.ToArray();

            entries = exceptionInfo.Split(new [] { ": ", }, StringSplitOptions.None);
            DTOs.Exception ex;
            switch (entries.Length)
            {
                case 3:
                    ex = new DTOs.Exception(entries[0], entries[1], entries[2]);
                    break;
                case 2:
                    ex = new DTOs.Exception(entries[0], entries[1]);
                    break;
                case 1:
                    ex = new DTOs.Exception(entries[0]);
                    break;
                default:
                    traceSource.TraceEvent(TraceEventType.Warning, 0, $"Unexpected exception info component/entry...");
                    ex = new DTOs.Exception("Default exception mock!");
                    break;
            }
            this.Exception = ex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Trace"/> class.
        /// </summary>
        /// <param name="frames">The frames.</param>
        /// <param name="exception">The exception.</param>
        public Trace(Frame[] frames, Exception exception)
        {
            Assumption.AssertNotNull(frames, nameof(frames));
            Assumption.AssertNotNull(exception, nameof(exception));

            this.Frames = frames;
            this.Exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Trace"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public Trace(System.Exception exception)
        {
            Assumption.AssertNotNull(exception, nameof(exception));

            var frames = new StackTrace(exception, true).GetFrames() ?? new StackFrame[0];

            this.Frames = frames.Select(frame => new Frame(frame)).ToArray();
            this.Exception = new DTOs.Exception(exception);
        }

        /// <summary>
        /// Gets the frames.
        /// </summary>
        /// <value>
        /// The frames.
        /// </value>
        [JsonProperty("frames", Required = Required.Always)]
        public Frame[] Frames { get; internal set; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        [JsonProperty("exception", Required = Required.Always)]
        public Exception Exception { get; private set; }
    }
}
