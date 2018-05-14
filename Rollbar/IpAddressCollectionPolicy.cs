namespace Rollbar
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Enumerates all the applicable IP address collection policies.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum IpAddressCollectionPolicy
    {
        /// <summary>
        /// Collect the IP address...
        /// </summary>
        Collect,

        /// <summary>
        /// Collect the IP address but anonymize it...
        /// </summary>
        /// <remarks>
        /// About IP Anonymization in Analytics, please, refer to:
        /// https://support.google.com/analytics/answer/2763052
        /// </remarks>
        CollectAnonymized,

        /// <summary>
        /// Do not collect the IP address
        /// </summary>
        DoNotCollect,

    }
}
