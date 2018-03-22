namespace Rollbar.PayloadTruncation
{
    using Rollbar.DTOs;

    /// <summary>
    /// Implements "Iterative as needed" Payload truncation strategy.
    /// </summary>
    /// <seealso cref="Rollbar.PayloadTruncation.PayloadTruncationStrategyBase" />
    internal class IterativeTruncationStrategy
        : PayloadTruncationStrategyBase
    {
        private readonly IPayloadTruncationStrategy[] _orderedTruncationStrategies = 
            new IPayloadTruncationStrategy[] {
                new RawTruncationStrategy(),
                new FramesTruncationStrategy(totalHeadFramesToPreserve: 10, totalTailFramesToPreserve: 10),
                new StringsTruncationStrategy(stringBytesLimit: 1024),
                new StringsTruncationStrategy(stringBytesLimit:  512),
                new StringsTruncationStrategy(stringBytesLimit:  256),
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
