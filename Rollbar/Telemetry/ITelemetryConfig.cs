namespace Rollbar.Telemetry
{
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Defines telemetry configuration interface.
    /// </summary>
    public interface ITelemetryConfig
    {
        /// <summary>
        /// Gets a value indicating whether telemetry is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if telemetry is enabled; otherwise, <c>false</c>.
        /// </value>
        bool TelemetryEnabled { get; }

        /// <summary>
        /// Gets the telemetry queue depth.
        /// </summary>
        /// <value>
        /// The telemetry queue depth.
        /// </value>
        int TelemetryQueueDepth { get; }

        /// <summary>
        /// Gets the telemetry automatic collection types.
        /// </summary>
        /// <value>
        /// The telemetry automatic collection types.
        /// </value>
        TelemetryType TelemetryAutoCollectionTypes { get; }

        /// <summary>
        /// Gets the telemetry automatic collection interval.
        /// </summary>
        /// <value>
        /// The telemetry automatic collection interval.
        /// </value>
        TimeSpan TelemetryAutoCollectionInterval { get; }
    }
}
