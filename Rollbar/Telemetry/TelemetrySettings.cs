namespace Rollbar.Telemetry
{
    using System;

    [Flags]
    public enum TelemetrySettings
    {
        None = 0x000,

        ProcessCpuUtilization = 0x0001 << 01,
        ProcessMemoryUtilization = 0x0001 << 02,

        MachineCpuUtilization = 0x0001 << 10,
        MachineMemoryUtilization = 0x0001 << 11,

    }
}
