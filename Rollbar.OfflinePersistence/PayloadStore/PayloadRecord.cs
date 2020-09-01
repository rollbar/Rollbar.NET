namespace Rollbar.PayloadStore 
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Class PayloadRecord.
    /// </summary>
    public class PayloadRecord : IPayloadRecord
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid ID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>The timestamp.</value>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the payload json.
        /// </summary>
        /// <value>The payload json.</value>
        public string PayloadJson { get; set; }

        /// <summary>
        /// Gets or sets the configuration json.
        /// </summary>
        /// <value>The configuration json.</value>
        public string ConfigJson { get; set; }

        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        /// <value>The destination.</value>
        public Destination Destination { get; set; }

        /// <summary>
        /// Gets or sets the destination identifier.
        /// </summary>
        /// <value>The destination identifier.</value>
        public Guid DestinationID { get; set; }

    }
}
