namespace Rollbar.PayloadTruncation
{
    using System.Text;
    using Rollbar.DTOs;

    /// <summary>
    /// Implements "Limited strings length" Payload truncation strategy.
    /// </summary>
    /// <seealso cref="Rollbar.PayloadTruncation.PayloadTruncationStrategyBase" />
    internal class StringsTruncationStrategy
        : PayloadTruncationStrategyBase
    {
        private readonly int _stringBytesLimit; //from higher to lower threshold value...

        /// <summary>
        /// Prevents a default instance of the <see cref="StringsTruncationStrategy"/> class from being created.
        /// </summary>
        private StringsTruncationStrategy()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringsTruncationStrategy"/> class.
        /// </summary>
        /// <param name="stringBytesLimit">The string bytes limit.</param>
        public StringsTruncationStrategy(int stringBytesLimit)
        {
            this._stringBytesLimit = stringBytesLimit;
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
            if(payload == null)
            {
                return 0;
            }

            payload.TruncateStrings(Encoding.UTF8, _stringBytesLimit);

            return GetSizeInBytes(payload);
        }
    }
}
