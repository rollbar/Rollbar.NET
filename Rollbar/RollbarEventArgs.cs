namespace Rollbar
{
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
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
        private readonly RollbarLogger _logger = null;

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
            //Assumption.AssertNotNull(logger, nameof(logger));

            this._logger = logger;
            if (dataObject != null)
            {
                this.Payload = JsonConvert.SerializeObject(dataObject);
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
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>
        /// String rendering of this instance.
        /// </returns>
        public virtual string TraceAsString(string indent = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + this.GetType().Name + ":");
            sb.Append(indent + this.Config?.TraceAsString("  "));
            sb.AppendLine(indent + "  Payload: " + this.Payload);
            return sb.ToString();
        }
    }
}
