namespace Rollbar.Telemetry
{
    using Rollbar.Common;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Text;

#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
#pragma warning disable CS1658 // Warning is overriding an error
    /// <summary>
    /// ReconfigurableBase
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{Rollbar.Telemetry.TelemetryConfig, Rollbar.Telemetry.ITelemetryConfig}" />
    /// <seealso cref="Rollbar.Telemetry.ITelemetryConfig" />
    /// <seealso cref="Rollbar.ITraceable" />
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
    public class TelemetryConfig
        : ReconfigurableBase<TelemetryConfig, ITelemetryConfig>
        , ITelemetryConfig
        , ITraceable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryConfig"/> class.
        /// </summary>
        internal TelemetryConfig()
        {
            this.SetDefaults();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryConfig"/> class.
        /// </summary>
        /// <param name="telemetryEnabled">if set to <c>true</c> [telemetry enabled].</param>
        /// <param name="telemetryQueueDepth">The telemetry queue depth.</param>
        public TelemetryConfig(bool telemetryEnabled, int telemetryQueueDepth)
            : this(telemetryEnabled, telemetryQueueDepth, TelemetryType.None, TimeSpan.Zero)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryConfig" /> class.
        /// </summary>
        /// <param name="telemetryEnabled">if set to <c>true</c> [telemetry enabled].</param>
        /// <param name="telemetryQueueDepth">The telemetry queue depth.</param>
        /// <param name="telemetryAutoCollectionTypes">The telemetry automatic collection types.</param>
        /// <param name="telemetryCollectionInterval">The telemetry collection interval.</param>
        public TelemetryConfig(
            bool telemetryEnabled, 
            int telemetryQueueDepth, 
            TelemetryType telemetryAutoCollectionTypes, 
            TimeSpan telemetryCollectionInterval
            )
        {
            this.SetDefaults();

            this.TelemetryEnabled = telemetryEnabled;
            this.TelemetryQueueDepth = telemetryQueueDepth;
            this.TelemetryAutoCollectionTypes = telemetryAutoCollectionTypes;
            this.TelemetryAutoCollectionInterval = telemetryCollectionInterval;
        }

        /// <summary>
        /// Gets a value indicating whether telemetry is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if telemetry is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool TelemetryEnabled { get; set; }

        /// <summary>
        /// Gets the telemetry queue depth.
        /// </summary>
        /// <value>
        /// The telemetry queue depth.
        /// </value>
        public int TelemetryQueueDepth { get; set; }

        /// <summary>
        /// Gets the telemetry automatic collection types.
        /// </summary>
        /// <value>
        /// The telemetry automatic collection types.
        /// </value>
        public TelemetryType TelemetryAutoCollectionTypes { get; set; }

        /// <summary>
        /// Gets the telemetry automatic collection interval.
        /// </summary>
        /// <value>
        /// The telemetry automatic collection interval.
        /// </value>
        public TimeSpan TelemetryAutoCollectionInterval { get; set; }

        /// <summary>
        /// Traces as a string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>
        /// String rendering of this instance.
        /// </returns>
        public string TraceAsString(string indent = "")
        {
            return this.RenderAsString(indent);
        }

        private void SetDefaults()
        {
            // let's set some default values:
            this.TelemetryEnabled = false;
            this.TelemetryQueueDepth = 5;
            this.TelemetryAutoCollectionTypes = TelemetryType.None;
            this.TelemetryAutoCollectionInterval = TimeSpan.Zero;

#if NETFX
            // initialize based on app.config settings of Rollbar section (if any):
            this.InitFromAppConfig();
#endif
        }

#if NETFX
        private void InitFromAppConfig()
        {
            Rollbar.NetFramework.RollbarTelemetryConfigSection config =
                Rollbar.NetFramework.RollbarTelemetryConfigSection.GetConfiguration();
            if (config == null)
            {
                return;
            }

            if (config.TelemetryEnabled.HasValue)
            {
                this.TelemetryEnabled = config.TelemetryEnabled.Value;
            }
            if (config.TelemetryQueueDepth.HasValue)
            {
                this.TelemetryQueueDepth = config.TelemetryQueueDepth.Value;
            }
            if (config.TelemetryAutoCollectionTypes.HasValue)
            {
                this.TelemetryAutoCollectionTypes = config.TelemetryAutoCollectionTypes.Value;
            }
            if (config.TelemetryAutoCollectionInterval.HasValue)
            {
                this.TelemetryAutoCollectionInterval = config.TelemetryAutoCollectionInterval.Value;
            }

        }
#endif

    }
}
