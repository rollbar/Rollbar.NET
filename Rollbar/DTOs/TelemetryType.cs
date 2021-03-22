namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Enumerates supported telemetry types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum TelemetryType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x000,

        /// <summary>
        /// The log
        /// </summary>
        [EnumMember(Value = "log")]
        Log = 0x0001 << 0,

        /// <summary>
        /// The network
        /// </summary>
        [EnumMember(Value = "network")]
        Network = 0x0001 << 1,

        /// <summary>
        /// The DOM
        /// </summary>
        [EnumMember(Value = "dom")]
        Dom = 0x0001 << 2,

        /// <summary>
        /// The navigation
        /// </summary>
        [EnumMember(Value = "navigation")]
        Navigation = 0x0001 << 3,

        /// <summary>
        /// The error
        /// </summary>
        [EnumMember(Value = "error")]
        Error = 0x0001 << 4,

        /// <summary>
        /// The manual
        /// </summary>
        [EnumMember(Value = "manual")]
        Manual = 0x0001 << 5,
    }

}
