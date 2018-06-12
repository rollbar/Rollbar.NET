namespace Rollbar.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface ITelemetryConfig
    {
        bool TelemetryEnabled { get; }

        TelemetrySettings TelemetrySettings { get; }

        int TelemetryQueueDepth { get; }

        TimeSpan TelemetryCollectionInterval { get; }
    }
}
