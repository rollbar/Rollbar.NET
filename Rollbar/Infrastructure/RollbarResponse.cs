namespace Rollbar.Infrastructure
{
    using Newtonsoft.Json;
    using System.Text;

    /// <summary>
    /// Models Rollbar API response.
    /// </summary>
    /// <seealso cref="Rollbar.ITraceable" />
    public class RollbarResponse
        : ITraceable
    {
        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        [JsonProperty("err")]
        public int Error { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public RollbarResult? Result { get; set; }

        /// <summary>
        /// Gets or sets the HTTP details.
        /// </summary>
        /// <value>
        /// The HTTP details.
        /// </value>
        [JsonIgnore]
        public string? HttpDetails { get; set; }

        /// <summary>
        /// Gets or sets the rollbar rate limit.
        /// </summary>
        /// <value>The rollbar rate limit.</value>
        [JsonIgnore]
        public RollbarRateLimit? RollbarRateLimit { get; set; }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <returns>System.String.</returns>
        public string TraceAsString()
        {
            return this.TraceAsString(string.Empty);
        }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>
        /// String rendering of this instance.
        /// </returns>
        public string TraceAsString(string indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + this.GetType().Name + ":");
            sb.AppendLine(indent + "  Error: " + this.Error);
            sb.AppendLine(indent + "  Result: ");
            sb.AppendLine(indent  + this.Result?.TraceAsString(indent + "  "));
            sb.AppendLine(indent + "  Details: ");
            sb.AppendLine(indent + this.HttpDetails);
            return sb.ToString();
        }
    }
}
