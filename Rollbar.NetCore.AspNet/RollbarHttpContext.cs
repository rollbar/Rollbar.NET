namespace Rollbar.NetCore.AspNet
{
    using System;

    /// <summary>
    /// Models metadata of interest about relevant HTTP context.
    /// </summary>
    public class RollbarHttpContext
    {
        /// <summary>
        /// Gets or sets the HTTP attributes.
        /// </summary>
        /// <value>
        /// The HTTP attributes.
        /// </value>
        public RollbarHttpAttributes HttpAttributes { get; set; }

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public DateTimeOffset Timestamp { get; } 
            = DateTimeOffset.UtcNow;
    }
}
