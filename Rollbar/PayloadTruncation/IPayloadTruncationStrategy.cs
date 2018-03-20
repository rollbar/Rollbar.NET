namespace Rollbar.PayloadTruncation
{

    using Rollbar.DTOs;

    /// <summary>
    /// Defines an interface for a payload truncation strategy implementation.
    /// </summary>
    internal interface IPayloadTruncationStrategy
    {
        /// <summary>
        /// Gets the maximum payload size in bytes.
        /// 
        /// A truncation strategy attempts to truncate a provided payload to lesser or equal size.
        /// </summary>
        /// <value>
        /// The maximum payload size in bytes.
        /// </value>
        int MaxPayloadSizeInBytes { get; }

        /// <summary>
        /// Truncates the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>Payload size (in bytes) after the truncation.</returns>
        int Truncate(Payload payload);
    }
}
