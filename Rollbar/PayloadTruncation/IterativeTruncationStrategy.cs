namespace Rollbar.PayloadTruncation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rollbar.DTOs;

    internal class IterativeTruncationStrategy
        : PayloadTruncationStrategyBase
    {
        private readonly IPayloadTruncationStrategy[] _orderedTruncationStrategies = 
            new IPayloadTruncationStrategy[] {
                new RawTruncationStrategy(),
                new FramesTruncationStrategy(),
                new RawTruncationStrategy(),
                new MinBodyTruncationStrategy(),
            };

        /// <summary>
        /// Truncates the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        /// Payload size (in bytes) after the truncation.
        /// </returns>
        public override int Truncate(Payload payload)
        {
            int result = 0;

            foreach(var strategy in _orderedTruncationStrategies)
            {
                result = strategy.Truncate(payload);
                if (result <= this.MaxPayloadSizeInBytes)
                {
                    return result;
                }
            }

            return result;
        }
    }
}
