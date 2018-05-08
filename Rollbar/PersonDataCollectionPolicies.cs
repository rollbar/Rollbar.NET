namespace Rollbar
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;

    /// <summary>
    /// Lists all applicable Person data collection policies.
    /// </summary>
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PersonDataCollectionPolicies
    {
        /// <summary>
        /// None of the personal data gets collected...
        /// </summary>
        None = 0x00,

        /// <summary>
        /// The Person username gets collected...
        /// </summary>
        Username = 0x01 << 0,

        /// <summary>
        /// The Person email gets collected...
        /// </summary>
        Email = 0x01 << 1,
    }
}
