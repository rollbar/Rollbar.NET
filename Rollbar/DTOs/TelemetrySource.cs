namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TelemetrySource
    {
        [EnumMember(Value = "server")]
        Server,
        [EnumMember(Value = "client")]
        Client,
    }
}
