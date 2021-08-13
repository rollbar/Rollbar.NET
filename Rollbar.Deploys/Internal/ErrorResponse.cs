namespace Rollbar.Deploys
{
    using Newtonsoft.Json;

    /// <summary>
    /// Models an error-like response.
    /// </summary>
    /// <seealso cref="Rollbar.Deploys.ResponseBase" />
    internal class ErrorResponse
        : ResponseBase
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Message { get; set; }
    }
}
