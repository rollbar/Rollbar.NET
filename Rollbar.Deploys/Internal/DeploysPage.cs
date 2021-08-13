namespace Rollbar.Deploys
{
    using System;

    using Newtonsoft.Json;

    /// <summary>
    /// Models a deploys page.
    /// </summary>
    internal class DeploysPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeploysPage"/> class.
        /// </summary>
        public DeploysPage()
        {
            this.Deploys = Array.Empty<Deploy>();
        }

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        /// <value>
        /// The page number.
        /// </value>
        [JsonProperty("page", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the deploys.
        /// </summary>
        /// <value>
        /// The deploys.
        /// </value>
        [JsonProperty("deploys", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Deploy[] Deploys { get; set; }
    }
}
