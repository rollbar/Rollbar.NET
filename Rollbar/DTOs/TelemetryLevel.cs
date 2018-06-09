namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TelemetryLevel
    {
        [EnumMember(Value = "critical")]
        Critical,
        [EnumMember(Value = "error")]
        Error,
        [EnumMember(Value = "warning")]
        Warning,
        [EnumMember(Value = "info")]
        Info,
        [EnumMember(Value = "debug")]
        Debug,
    }
}
