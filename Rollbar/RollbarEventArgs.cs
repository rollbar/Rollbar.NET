namespace Rollbar
{
    using Newtonsoft.Json;
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
        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarEventArgs"/> class from being created.
        /// </summary>
        private RollbarEventArgs()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarEventArgs"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="payload">The payload.</param>
        protected RollbarEventArgs(
            RollbarConfig config, 
            Payload payload
            )
        {
            this.Config = config;
            if (payload != null)
            {
                this.Payload = JsonConvert.SerializeObject(payload);
            }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public RollbarConfig Config { get; private set; }

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
            sb.Append(indent + this.Config.TraceAsString("  "));
            sb.AppendLine(indent + "  Payload: " + this.Payload);
            return sb.ToString();
        }
    }
}
