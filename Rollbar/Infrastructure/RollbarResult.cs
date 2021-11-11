namespace Rollbar.Infrastructure
{
    using System.Text;

    /// <summary>
    /// Models Rollbar API response result.
    /// </summary>
    /// <seealso cref="Rollbar.ITraceable" />
    public class RollbarResult
        : ITraceable
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the UUID.
        /// </summary>
        /// <value>
        /// The UUID.
        /// </value>
        public string? Uuid { get; set; }

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
        /// <returns>String rendering of this instance.</returns>
        public string TraceAsString(string indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + this.GetType().Name + ":");
            sb.AppendLine(indent + "  Id: " + this.Id);
            sb.AppendLine(indent + "  Uuid: " + this.Uuid);
            return sb.ToString();
        }
    }
}
