namespace Rollbar
{
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// An abstract base for implementing concrete Rollbar EventArgs.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    /// <seealso cref="Rollbar.ITraceable" />
    public abstract class RollbarEventArgs 
        : EventArgs
        , ITraceable
    {
        private readonly RollbarLogger _logger;

        private readonly DateTimeOffset _eventTimeStamp = DateTimeOffset.Now;

        internal RollbarLogger Logger
        {
            get { return this._logger; }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarEventArgs"/> class from being created.
        /// </summary>
        private RollbarEventArgs()
        {

        }

        internal RollbarEventArgs(
            RollbarLogger logger, 
            object dataObject
            )
        {
            this._logger = logger;

            this.Payload = string.Empty;

            if (dataObject != null)
            {
                try
                {
                    this.Payload = JsonConvert.SerializeObject(dataObject);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex, $"{nameof(RollbarEventArgs)}.{nameof(this.Payload)}");
                }
            }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IRollbarConfig Config { get { return this._logger?.Config; } }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        public string Payload { get; private set; }

        /// <summary>
        /// Gets the event timestamp.
        /// </summary>
        /// <value>The event timestamp.</value>
        public DateTimeOffset EventTimestamp
        {
            get { return this._eventTimeStamp; }
        }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <returns>System.String.</returns>
        public virtual string TraceAsString()
        {
            return this.TraceAsString(string.Empty);
        }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>
        /// String rendering of this instance.
        /// </returns>
        public virtual string TraceAsString(string indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + this._eventTimeStamp + " - " + this.GetType().Name + ":");
            sb.Append(indent + this.Config?.TraceAsString("  "));
            sb.AppendLine(indent + "  Payload: " + this.Payload);
            return sb.ToString();
        }
    }
}
