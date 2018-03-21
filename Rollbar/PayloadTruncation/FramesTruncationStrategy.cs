namespace Rollbar.PayloadTruncation
{
    using Rollbar.DTOs;

    internal class FramesTruncationStrategy
        : PayloadTruncationStrategyBase
    {
        private readonly int _totalHeadFramesToPreserve;
        private readonly int _totalTailFramesToPreserve;

        private FramesTruncationStrategy()
        {
        }

        public FramesTruncationStrategy(
            int totalHeadFramesToPreserve, 
            int totalTailFramesToPreserve
            )
        {
            this._totalHeadFramesToPreserve = totalHeadFramesToPreserve;
            this._totalTailFramesToPreserve = totalTailFramesToPreserve;
        }

        public override int Truncate(Payload payload)
        {
            Trace[] traceChain = payload.Data.Body.TraceChain;
            if (traceChain == null && (payload.Data.Body.Trace != null))
            {
                traceChain = new Trace[] { payload.Data.Body.Trace };
            }

            if (traceChain == null || traceChain.Length == 0)
            {
                return this.GetSizeInBytes(payload);
            }

            foreach(var trace in traceChain)
            {
                this.TrimFrames(trace);
            }
            return this.GetSizeInBytes(payload);
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
