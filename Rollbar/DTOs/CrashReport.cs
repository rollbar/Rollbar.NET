namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Common;
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
            this.Raw = string.Empty;
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
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator GetValidator()
        {
            var validator = new Validator<CrashReport, CrashReport.CrashReportValidationRule>()
                    .AddValidation(
                        CrashReport.CrashReportValidationRule.ValidRaw,
                        (crashReport) => { return !string.IsNullOrWhiteSpace(crashReport.Raw); }
                        )
               ;

            return validator;
        }

        /// <summary>
        /// Enum CrashReportValidationRule
        /// </summary>
        public enum CrashReportValidationRule
        {
            /// <summary>
            /// The valid raw
            /// </summary>
            ValidRaw,
        }
    }
}
