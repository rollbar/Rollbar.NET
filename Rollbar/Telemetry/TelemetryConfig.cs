namespace Rollbar.Telemetry
{
    using Rollbar.Common;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{Rollbar.Telemetry.TelemetryConfig, Rollbar.Telemetry.ITelemetryConfig}" />
    /// <seealso cref="Rollbar.Telemetry.ITelemetryConfig" />
    /// <seealso cref="Rollbar.ITraceable" />
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
            = false;

        /// <summary>
        /// Gets the telemetry queue depth.
        /// </summary>
        /// <value>
        /// The telemetry queue depth.
        /// </value>
        public int TelemetryQueueDepth { get; set; }
            = 5;

        /// <summary>
        /// Gets the telemetry automatic collection types.
        /// </summary>
        /// <value>
        /// The telemetry automatic collection types.
        /// </value>
        public TelemetryType TelemetryAutoCollectionTypes { get; set; }
            = TelemetryType.None;

        /// <summary>
        /// Gets the telemetry automatic collection interval.
        /// </summary>
        /// <value>
        /// The telemetry automatic collection interval.
        /// </value>
        public TimeSpan TelemetryAutoCollectionInterval { get; set; }
            = TimeSpan.Zero;

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
    }
}
