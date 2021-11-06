namespace Rollbar
{
    using System.Text;
    using Rollbar.DTOs;
    using Rollbar.Infrastructure;

    /// <summary>
    /// Models a communication error event.
    /// </summary>
    /// <seealso cref="Rollbar.RollbarEventArgs" />
    public class CommunicationErrorEventArgs
        : RollbarEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationErrorEventArgs"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="error">The error.</param>
        /// <param name="retriesLeft">The retries left.</param>
        internal CommunicationErrorEventArgs(
            RollbarLogger? logger, 
            string? payload, 
            System.Exception error, 
            int retriesLeft
            ) 
            : base(logger, payload)
        {
            this.Error = error;
            this.RetriesLeft = retriesLeft;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationErrorEventArgs"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="error">The error.</param>
        /// <param name="retriesLeft">The retries left.</param>
        internal CommunicationErrorEventArgs(
            RollbarLogger? logger, 
            Payload payload, 
            System.Exception error, 
            int retriesLeft
            ) 
            : base(logger, payload)
        {
            this.Error = error;
            this.RetriesLeft = retriesLeft;
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public System.Exception Error { get; private set; }

        /// <summary>
        /// Gets the retries left.
        /// </summary>
        /// <value>
        /// The retries left.
        /// </value>
        public int RetriesLeft { get; private set; }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>
        /// String rendering of this instance.
        /// </returns>
        public override string TraceAsString(string indent)
        {
            StringBuilder sb = new StringBuilder(base.TraceAsString(indent));
            sb.AppendLine(indent + "  RetriesLeft: " + this.RetriesLeft);
            sb.AppendLine(indent + "  Error: " + this.Error);
            return sb.ToString();
        }
    }
}
