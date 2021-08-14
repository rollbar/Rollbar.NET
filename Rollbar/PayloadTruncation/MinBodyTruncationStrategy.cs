namespace Rollbar.PayloadTruncation
{
    using Rollbar.DTOs;
    using System.Linq;

    /// <summary>
    /// Implements "Minimal Body" Payload truncation strategy.
    /// </summary>
    /// <seealso cref="Rollbar.PayloadTruncation.PayloadTruncationStrategyBase" />
    internal class MinBodyTruncationStrategy
        : PayloadTruncationStrategyBase
    {
        private const int maxExceptionMessageChars = 256;
        private const int maxTraceFrames = 1;

        /// <summary>
        /// Truncates the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        /// Payload size (in bytes) after the truncation.
        /// </returns>
        public override int Truncate(Payload? payload)
        {
            if(payload == null)
            {
                return 0;
            }
            if  (payload.Data == null || payload.Data.Body == null)
            {
                return GetSizeInBytes(payload); //nothing to truncate ...
            }

            Trace[]? traces = payload.Data.Body.TraceChain;
            if ((traces == null || traces.LongLength == 0) 
                && (payload.Data.Body.Trace != null)
                )
            {
                traces = new Trace[] { payload.Data.Body.Trace, };
            }
            if (traces == null || traces.LongLength == 0)
            {
                return GetSizeInBytes(payload); //nothing to truncate ...
            }

            foreach(var trace in traces)
            {
                if (trace.Exception != null)
                {
                    trace.Exception.Description = null;
                    if (trace.Exception.Message != null
                        && trace.Exception.Message.Length > MinBodyTruncationStrategy.maxExceptionMessageChars
                        )
                    {
                        trace.Exception.Message =
                                trace.Exception.Message
                                .Substring(startIndex: 0, length: MinBodyTruncationStrategy.maxExceptionMessageChars);
                    }
                }
                if (trace.Frames != null && trace.Frames.LongLength > MinBodyTruncationStrategy.maxTraceFrames)
                {
                    trace.Frames = trace.Frames.Take(MinBodyTruncationStrategy.maxTraceFrames).ToArray();
                }
            }

            return GetSizeInBytes(payload);
        }
    }
}
