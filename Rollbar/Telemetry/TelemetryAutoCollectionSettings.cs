namespace Rollbar
{
    using System;

    /// <summary>
    /// Enum TelemetryAutoCollectionSettings
    /// </summary>
    [Flags]
    public enum TelemetryAutoCollectionSettings
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x000,

        /// <summary>
        /// The process cpu utilization
        /// </summary>
        ProcessCpuUtilization = 0x0001 << 01,
        /// <summary>
        /// The process memory utilization
        /// </summary>
        ProcessMemoryUtilization = 0x0001 << 02,

        /// <summary>
        /// The machine cpu utilization
        /// </summary>
        MachineCpuUtilization = 0x0001 << 10,
        /// <summary>
        /// The machine memory utilization
        /// </summary>
        MachineMemoryUtilization = 0x0001 << 11,

    }
}
