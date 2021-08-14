namespace Rollbar.PayloadTruncation
{
    using Rollbar.DTOs;
    using System.Collections.Generic;

    /// <summary>
    /// Implements "Iterative as needed" Payload truncation strategy.
    /// </summary>
    /// <seealso cref="Rollbar.PayloadTruncation.PayloadTruncationStrategyBase" />
    internal class IterativeTruncationStrategy
        : PayloadTruncationStrategyBase
    {
        private const int DefaultMaxPayloadSizeInBytes = 512 * 1024; //512 kB

        private static readonly IEnumerable<IPayloadTruncationStrategy> DefaultTruncationIterations = 
            new IPayloadTruncationStrategy[] {
                new RawTruncationStrategy(),
                new FramesTruncationStrategy(totalHeadFramesToPreserve: 10, totalTailFramesToPreserve: 10),
                new StringsTruncationStrategy(stringBytesLimit: 1024),
                new StringsTruncationStrategy(stringBytesLimit:  512),
                new StringsTruncationStrategy(stringBytesLimit:  256),
                new MinBodyTruncationStrategy(),
            };

        private readonly int _maxPayloadSizeInBytes;

        private readonly IEnumerable<IPayloadTruncationStrategy> _orderedTruncationStrategies;

        public IterativeTruncationStrategy()
            : this(IterativeTruncationStrategy.DefaultMaxPayloadSizeInBytes)
        {
        }

        public IterativeTruncationStrategy(int maxPayloadSizeInBytes)
            : this(maxPayloadSizeInBytes, IterativeTruncationStrategy.DefaultTruncationIterations)
        {

        }

        public IterativeTruncationStrategy(IEnumerable<IPayloadTruncationStrategy> orderedTruncationIterations)
            : this(IterativeTruncationStrategy.DefaultMaxPayloadSizeInBytes, orderedTruncationIterations)
        {

        }

        public IterativeTruncationStrategy(int maxPayloadSizeInBytes, IEnumerable<IPayloadTruncationStrategy> orderedTruncationIterations)
        {
            this._maxPayloadSizeInBytes = maxPayloadSizeInBytes;
            this._orderedTruncationStrategies = orderedTruncationIterations;
        }

        /// <summary>
        /// Gets the maximum payload size in bytes.
        /// A truncation strategy attempts to truncate a provided payload to lesser or equal size.
        /// </summary>
        /// <value>
        /// The maximum payload size in bytes.
        /// </value>
        public int MaxPayloadSizeInBytes
        {
            get { return this._maxPayloadSizeInBytes; }
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

        /// <summary>
        /// Gets the ordered truncation strategies.
        /// </summary>
        /// <value>The ordered truncation strategies.</value>
        public IEnumerable<IPayloadTruncationStrategy> OrderedTruncationStrategies
        {
            get { return this._orderedTruncationStrategies; }
        }
    }
}
