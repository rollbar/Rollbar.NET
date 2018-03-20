namespace Rollbar.PayloadTruncation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rollbar.DTOs;

    /// <summary>
    /// No-Op payload truncation strategy implementation.
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
        public override int Truncate(Payload payload)
        {
            return this.GetSizeInBytes(payload);
        }
    }
}
