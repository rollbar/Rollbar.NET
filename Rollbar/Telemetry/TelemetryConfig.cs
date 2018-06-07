namespace Rollbar.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class TelemetryConfig
    {
        public bool TelemetryEnabled { get; set; }
            = false;

        public TelemetrySettings TelemetrySettings { get; set; }
            = TelemetrySettings.ProcessCpuUtilization
            | TelemetrySettings.ProcessMemoryUtilization
            | TelemetrySettings.MachineCpuUtilization
            | TelemetrySettings.MachineMemoryUtilization
            ;

        public int TelemetryQueueDepth { get; set; } 
            = 5;

        public TimeSpan TelemetryCollectionInterval { get; set; }
            = TimeSpan.FromMilliseconds(500);

    }
}
