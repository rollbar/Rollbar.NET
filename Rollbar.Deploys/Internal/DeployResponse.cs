namespace Rollbar.Deploys
{
    using Newtonsoft.Json;

    /// <summary>
    /// Models a deploy query response.
    /// </summary>
    /// <seealso cref="Rollbar.Deploys.ResponseBase" />
    internal class DeployResponse
        : ResponseBase
    {
        /// <summary>
        /// Gets or sets the deploy.
        /// </summary>
        /// <value>
        /// The deploy.
        /// </value>
        [JsonProperty("result", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Deploy? Deploy { get; set; }
    }
}
