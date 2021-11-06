namespace Rollbar
{
    using System;
    using Rollbar.DTOs;
    using Rollbar.Infrastructure;

    /// <summary>
    /// Class TransmissionOmittedEventArgs.
    /// Implements the <see cref="Rollbar.RollbarEventArgs" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarEventArgs" />
    public class TransmissionOmittedEventArgs
        : RollbarEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransmissionOmittedEventArgs"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="payload">The payload.</param>
        internal TransmissionOmittedEventArgs(RollbarLogger logger, Payload payload)
            : base(logger, payload)
        {
        }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>String rendering of this instance.</returns>
        public override string TraceAsString(string indent)
        {
            return base.TraceAsString(indent);
        }
    }

}
