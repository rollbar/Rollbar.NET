namespace Rollbar.PayloadTruncation
{
    using Rollbar.DTOs;

    /// <summary>
    /// Implements "No-Op" Payload truncation strategy.
    /// </summary>
    /// <seealso cref="Rollbar.PayloadTruncation.PayloadTruncationStrategyBase" />
    internal class RawTruncationStrategy
        : PayloadTruncationStrategyBase
    {
        /// <summary>
        /// Truncates the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        /// Payload size (in bytes) after the truncation.
        /// </returns>
        public override int Truncate(Payload? payload)
        {
            return GetSizeInBytes(payload);
        }
    }
}
