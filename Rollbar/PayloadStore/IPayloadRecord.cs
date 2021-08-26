namespace Rollbar.PayloadStore
{
    using System;

    /// <summary>
    /// Interface IPayloadRecord
    /// </summary>
    public interface IPayloadRecord
    {
        /// <summary>
        /// Gets or sets the configuration json.
        /// </summary>
        /// <value>The configuration json.</value>
        string? ConfigJson { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        Guid ID { get; set; }

        /// <summary>
        /// Gets or sets the payload json.
        /// </summary>
        /// <value>The payload json.</value>
        string? PayloadJson { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>The timestamp.</value>
        DateTime Timestamp { get; set; }
    }
}