namespace Rollbar.PayloadTruncation
{
    using Rollbar.DTOs;

    /// <summary>
    /// Implements "middle Frames reduction" Payload truncation strategy.
    /// </summary>
    /// <seealso cref="Rollbar.PayloadTruncation.PayloadTruncationStrategyBase" />
    internal class FramesTruncationStrategy
        : PayloadTruncationStrategyBase
    {
        private readonly int _totalHeadFramesToPreserve;
        private readonly int _totalTailFramesToPreserve;

        /// <summary>
        /// Prevents a default instance of the <see cref="FramesTruncationStrategy"/> class from being created.
        /// </summary>
        private FramesTruncationStrategy()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FramesTruncationStrategy"/> class.
        /// </summary>
        /// <param name="totalHeadFramesToPreserve">The total head frames to preserve.</param>
        /// <param name="totalTailFramesToPreserve">The total tail frames to preserve.</param>
        public FramesTruncationStrategy(
            int totalHeadFramesToPreserve, 
            int totalTailFramesToPreserve
            )
        {
            this._totalHeadFramesToPreserve = totalHeadFramesToPreserve;
            this._totalTailFramesToPreserve = totalTailFramesToPreserve;
        }

        /// <summary>
        /// Truncates the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        /// Payload size (in bytes) after the truncation.
        /// </returns>
        public override int Truncate(Payload? payload)
        {
            Trace[]? traceChain = payload?.Data.Body.TraceChain;
            if (traceChain == null && (payload?.Data.Body.Trace != null))
            {
                traceChain = new Trace[] { payload.Data.Body.Trace };
            }

            if (traceChain == null || traceChain.Length == 0)
            {
                return GetSizeInBytes(payload);
            }

            foreach(var trace in traceChain)
            {
                this.TrimFrames(trace);
            }
            return GetSizeInBytes(payload);
        }

        private void TrimFrames(Trace trace)
        {
            int totalFramesToPreserve = 
                this._totalHeadFramesToPreserve + this._totalTailFramesToPreserve;

            if (trace.Frames.Length <= totalFramesToPreserve)
            {
                return; //nothing to trim...
            }

            Frame[] trimmedFrames = new Frame[totalFramesToPreserve];
            int trimmedIndx = 0;
            int inputIndx = 0;
            int lastIndxToSkip = 
                trace.Frames.Length - this._totalTailFramesToPreserve - 1;
            while(inputIndx < trace.Frames.Length)
            {
                if ((inputIndx < this._totalHeadFramesToPreserve) || (inputIndx > lastIndxToSkip))
                {
                    trimmedFrames[trimmedIndx++] = trace.Frames[inputIndx];
                }

                inputIndx++;
            }

            trace.Frames = trimmedFrames;
        }
    }
}
