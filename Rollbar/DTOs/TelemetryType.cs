namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TelemetryType
    {
        [EnumMember(Value = "log")]
        Log,
        [EnumMember(Value = "network")]
        Network,
        [EnumMember(Value = "dom")]
        Dom,
        [EnumMember(Value = "navigation")]
        Navigation,
        [EnumMember(Value = "error")]
        Error,
        [EnumMember(Value = "manual")]
        Manual,
    }

}
