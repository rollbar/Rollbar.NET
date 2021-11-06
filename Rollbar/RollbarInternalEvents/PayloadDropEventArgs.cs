namespace Rollbar
{
    using System;
    using System.Text;

    using Rollbar.DTOs;
    using Rollbar.Infrastructure;

    /// <summary>
    /// Class PayloadDropEventArgs.
    /// Implements the <see cref="Rollbar.RollbarEventArgs" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarEventArgs" />
    public class PayloadDropEventArgs
        : RollbarEventArgs
    {
        /// <summary>
        /// Enum DropReason
        /// </summary>
        public enum DropReason
        {
            /// <summary>
            /// The ignorable payload
            /// </summary>
            IgnorablePayload,

            /// <summary>
            /// The token suspension
            /// </summary>
            TokenSuspension,

            /// <summary>
            /// All transmission retries failed
            /// </summary>
            AllTransmissionRetriesFailed,

            /// <summary>
            /// The rollbar queue controller flushed queues
            /// </summary>
            RollbarQueueControllerFlushedQueues,

            /// <summary>
            /// The invalid payload
            /// </summary>
            InvalidPayload,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadDropEventArgs"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="dropReason">The drop reason.</param>
        internal PayloadDropEventArgs(RollbarLogger logger, Payload? payload, DropReason dropReason) 
            : base(logger, payload)
        {
            this.Reason = dropReason;
        }

        /// <summary>
        /// Gets the reason.
        /// </summary>
        /// <value>The reason.</value>
        public DropReason Reason
        {
            get;
            private set;
        }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>String rendering of this instance.</returns>
        public override string TraceAsString(string indent)
        {
            StringBuilder sb = new StringBuilder(base.TraceAsString(indent));
            sb.AppendLine(indent + "  Drop Reason: " + this.Reason);
            return sb.ToString();
        }

    }
}
