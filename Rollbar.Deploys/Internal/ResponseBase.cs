namespace Rollbar.Deploys
{
    using Newtonsoft.Json;

    /// <summary>
    /// Models the response abstraction.
    /// </summary>
    internal abstract class ResponseBase
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        [JsonProperty("err", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ErrorCode { get; set; }
    }
}
