namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Runtime.Serialization;

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
