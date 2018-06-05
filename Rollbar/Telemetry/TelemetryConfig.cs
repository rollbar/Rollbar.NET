namespace Rollbar.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class TelemetryConfig
    {
        public int TelemetryQueueDepth { get; set; } 
            = 5;

        public TimeSpan TelemetryCollectionInterval { get; set; }
            = TimeSpan.FromMilliseconds(500);

        public bool TelemetryEnabled { get; set; } 
            = true;

        public TelemetrySettings TelemetrySettings { get; set; } 
            = TelemetrySettings.ProcessCpuUtilization 
            | TelemetrySettings.ProcessMemoryUtilization 
            | TelemetrySettings.MachineCpuUtilization 
            | TelemetrySettings.MachineMemoryUtilization
            ;
    }
}
