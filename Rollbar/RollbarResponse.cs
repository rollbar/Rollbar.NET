namespace Rollbar
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
        public RollbarResult Result { get; set; }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>
        /// String rendering of this instance.
        /// </returns>
        public string TraceAsString(string indent = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + this.GetType().Name + ":");
            sb.AppendLine(indent + "  Error: " + this.Error);
            sb.AppendLine(indent + "  Result: ");
            sb.AppendLine(indent  + this.Result.TraceAsString(indent + "  "));
            return sb.ToString();
        }
    }
}
