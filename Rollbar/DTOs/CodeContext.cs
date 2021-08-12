namespace Rollbar.DTOs
{
    using Newtonsoft.Json;

    /// <summary>
    /// Models Rollbar CodeContext DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    public class CodeContext
        : DtoBase
    {
        /// <summary>
        /// Gets or sets the pre-code-context.
        /// </summary>
        /// <value>
        /// The pre-code-context.
        /// </value>
        [JsonProperty("pre", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[]? Pre { get; set; }

        /// <summary>
        /// Gets or sets the post-code-context.
        /// </summary>
        /// <value>
        /// The post-code-context.
        /// </value>
        [JsonProperty("post", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[]? Post { get; set; }
    }
}
