namespace Rollbar.Deploys
{
    using Newtonsoft.Json;

    /// <summary>
    /// Models deploys page query response.
    /// </summary>
    /// <seealso cref="Rollbar.Deploys.ResponseBase" />
    internal class DeploysPageResponse
        : ResponseBase
    {
        /// <summary>
        /// Gets or sets the deploys page.
        /// </summary>
        /// <value>
        /// The deploys page.
        /// </value>
        [JsonProperty("result", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DeploysPage? DeploysPage { get; set; }
    }
}
