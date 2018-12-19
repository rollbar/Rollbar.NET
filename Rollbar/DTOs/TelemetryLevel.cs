namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Runtime.Serialization;

    /// <summary>
    /// Enumerates supported telemetry levels.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TelemetryLevel
    {
        /// <summary>
        /// The critical
        /// </summary>
        [EnumMember(Value = "critical")]
        Critical,

        /// <summary>
        /// The error
        /// </summary>
        [EnumMember(Value = "error")]
        Error,

        /// <summary>
        /// The warning
        /// </summary>
        [EnumMember(Value = "warning")]
        Warning,

        /// <summary>
        /// The information
        /// </summary>
        [EnumMember(Value = "info")]
        Info,

        /// <summary>
        /// The debug
        /// </summary>
        [EnumMember(Value = "debug")]
        Debug,
    }
}
