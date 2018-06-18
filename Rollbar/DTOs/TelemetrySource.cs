namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Enumerates supported telemetry sources.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TelemetrySource
    {
        /// <summary>
        /// The server
        /// </summary>
        [EnumMember(Value = "server")]
        Server,

        /// <summary>
        /// The client
        /// </summary>
        [EnumMember(Value = "client")]
        Client,
    }
}
