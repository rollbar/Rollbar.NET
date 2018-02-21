namespace Rollbar
{
    using System.Text;
    using Rollbar.DTOs;

    /// <summary>
    /// Models an internal event.
    /// </summary>
    /// <seealso cref="Rollbar.RollbarEventArgs" />
    public class InternalErrorEventArgs
        : RollbarEventArgs
    {
        internal InternalErrorEventArgs(
            RollbarLogger logger,
            Payload payload,
            System.Exception error,
            string details
            ) 
            : base(logger, payload)
        {
            this.Error = error;
            this.Details = details;
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public System.Exception Error { get; private set; }
        /// <summary>
        /// Gets the details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        public string Details { get; private set; }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>
        /// String rendering of this instance.
        /// </returns>
        public override string TraceAsString(string indent = "")
        {
            StringBuilder sb = new StringBuilder(base.TraceAsString(indent));
            sb.AppendLine(indent + "  Error: " + this.Error);
            sb.AppendLine(indent + "  Details: " + this.Details);
            return sb.ToString();
        }
    }
}
