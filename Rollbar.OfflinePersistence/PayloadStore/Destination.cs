namespace Rollbar.PayloadStore 
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;

    /// <summary>
    /// Class Destination.
    /// </summary>
    public class Destination : IDestination
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid ID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        /// <value>The endpoint.</value>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>The access token.</value>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the payload records.
        /// </summary>
        /// <value>The payload records.</value>
        public ICollection<PayloadRecord> PayloadRecords { get; set; }
            = new List<PayloadRecord>();
    }
}
