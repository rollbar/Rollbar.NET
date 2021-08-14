namespace Rollbar.PayloadTruncation
{
    using Rollbar.DTOs;

    /// <summary>
    /// Defines an interface for a payload truncation strategy implementation.
    /// </summary>
    internal interface IPayloadTruncationStrategy
    {
        /// <summary>
        /// Truncates the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>Payload size (in bytes) after the truncation.</returns>
        int Truncate(Payload? payload);
    }
}
