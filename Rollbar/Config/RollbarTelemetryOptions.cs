namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;
    using Rollbar.DTOs;

    /// <summary>
    /// Class RollbarTelemetryOptions.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase{T, TBase}" />
    /// Implements the <see cref="Rollbar.IRollbarTelemetryOptions" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{T, TBase}" />
    /// <seealso cref="Rollbar.IRollbarTelemetryOptions" />
    public class RollbarTelemetryOptions
        : ReconfigurableBase<RollbarTelemetryOptions, IRollbarTelemetryOptions>
        , IRollbarTelemetryOptions
    {
        private const bool defaultEnabledValue = false;
        private const int defaultQueueDepth = 5;
        private const TelemetryType defaultTelemetryTypes = TelemetryType.None;
        private static readonly TimeSpan defaultAutoCollectionInterval = TimeSpan.Zero;

        internal RollbarTelemetryOptions()
            : this(
                  RollbarTelemetryOptions.defaultEnabledValue, 
                  RollbarTelemetryOptions.defaultQueueDepth, 
                  RollbarTelemetryOptions.defaultTelemetryTypes, 
                  RollbarTelemetryOptions.defaultAutoCollectionInterval
                  )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarTelemetryOptions"/> class.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="queueDepth">The queue depth.</param>
        public RollbarTelemetryOptions(bool enabled, int queueDepth)
            : this(
                  enabled,
                  queueDepth,
                  RollbarTelemetryOptions.defaultTelemetryTypes,
                  RollbarTelemetryOptions.defaultAutoCollectionInterval
                  )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarTelemetryOptions"/> class.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="telemetryTypes">The telemetry types.</param>
        /// <param name="autoCollectionInterval">The automatic collection interval.</param>
        public RollbarTelemetryOptions(bool enabled, TelemetryType telemetryTypes, TimeSpan autoCollectionInterval)
            : this(
                  enabled, 
                  RollbarTelemetryOptions.defaultQueueDepth, 
                  telemetryTypes, 
                  autoCollectionInterval
                  )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarTelemetryOptions"/> class.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="queueDepth">The queue depth.</param>
        /// <param name="telemetryTypes">The telemetry types.</param>
        /// <param name="autoCollectionInterval">The automatic collection interval.</param>
        public RollbarTelemetryOptions(bool enabled, int queueDepth, TelemetryType telemetryTypes, TimeSpan autoCollectionInterval)
        {
            this.TelemetryEnabled = enabled;
            this.TelemetryQueueDepth = queueDepth;
            this.TelemetryAutoCollectionTypes = telemetryTypes;
            this.TelemetryAutoCollectionInterval = autoCollectionInterval;
        }

        /// <summary>
        /// Gets or sets a value indicating whether telemetry is enabled.
        /// </summary>
        /// <value><c>true</c> if telemetry is enabled; otherwise, <c>false</c>.</value>
        public bool TelemetryEnabled
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the telemetry queue depth.
        /// </summary>
        /// <value>The telemetry queue depth.</value>
        public int TelemetryQueueDepth
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the telemetry automatic collection types.
        /// </summary>
        /// <value>The telemetry automatic collection types.</value>
        public TelemetryType TelemetryAutoCollectionTypes
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the telemetry automatic collection interval.
        /// </summary>
        /// <value>The telemetry automatic collection interval.</value>
        public TimeSpan TelemetryAutoCollectionInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        public override RollbarTelemetryOptions Reconfigure(IRollbarTelemetryOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator? GetValidator()
        {
            return null;
        }

        IRollbarTelemetryOptions IReconfigurable<IRollbarTelemetryOptions, IRollbarTelemetryOptions>.Reconfigure(IRollbarTelemetryOptions likeMe)
        {
            return this.Reconfigure(likeMe);
        }
    }
}
