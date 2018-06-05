namespace Rollbar.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public enum TelemetryAttribute
    {
        ProcessCpuUtilization,
        ProcessMemoryUtilization,

        MachineCpuUtilization,
        MachineMemoryUtilization,
    }
}
