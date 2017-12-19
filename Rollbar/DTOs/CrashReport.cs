namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Models Rollbar CrashReport DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    public class CrashReport
        : DtoBase
    {
        private CrashReport()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrashReport"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        public CrashReport(string report)
        {
            this.Raw = report;
            Validate();
        }

        /// <summary>
        /// Gets the raw.
        /// </summary>
        /// <value>
        /// The raw.
        /// </value>
        [JsonProperty("raw", Required = Required.Always)]
        public string Raw { get; private set; }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public override void Validate()
        {
            Assumption.AssertNotNullOrWhiteSpace(this.Raw, nameof(this.Raw));
        }
    }
}
