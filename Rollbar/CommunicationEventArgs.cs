namespace Rollbar
{
    using System.Text;
    using Rollbar.DTOs;

    /// <summary>
    /// Models a communication event.
    /// </summary>
    /// <seealso cref="Rollbar.RollbarEventArgs" />
    public class CommunicationEventArgs
        : RollbarEventArgs
    {
        internal CommunicationEventArgs(RollbarLogger logger, Payload payload, RollbarResponse response) 
            : base(logger, payload)
        {
            this.Response = response;
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public RollbarResponse Response { get; private set; }

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
            sb.AppendLine(indent + "  Response: " );
            sb.AppendLine(this.Response.TraceAsString(indent + "  "));
            return sb.ToString();
        }
    }
}
