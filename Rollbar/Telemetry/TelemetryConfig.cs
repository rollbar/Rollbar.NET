namespace Rollbar.Telemetry
{
    using Rollbar.Common;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class TelemetryConfig
        : ReconfigurableBase<TelemetryConfig, ITelemetryConfig>
        , ITelemetryConfig
        , ITraceable
    {
        internal TelemetryConfig()
        {
        }

        public TelemetryConfig(bool telemetryEnabled, int telemetryQueueDepth, TimeSpan telemetryCollectionInterval)
        {
            TelemetryEnabled = telemetryEnabled;
            TelemetryQueueDepth = telemetryQueueDepth;
            TelemetryCollectionInterval = telemetryCollectionInterval;
        }

        public bool TelemetryEnabled { get; private set; }
            = false;

        public TelemetrySettings TelemetrySettings { get; set; }
            = TelemetrySettings.None;

        public int TelemetryQueueDepth { get; private set; } 
            = 5;

        public TimeSpan TelemetryCollectionInterval { get; private set; }
            = TimeSpan.FromMilliseconds(500);

        public string TraceAsString(string indent = "")
        {
            return this.RenderAsString(indent);
        }
    }
}
