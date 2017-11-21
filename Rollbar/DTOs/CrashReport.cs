namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;

    public class CrashReport
        : DtoBase
    {
        private CrashReport()
        {

        }

        public CrashReport(string report)
        {
            this.Raw = report;
            Validate();
        }

        [JsonProperty("raw", Required = Required.Always)]
        public string Raw { get; private set; }

        public override void Validate()
        {
            Assumption.AssertNotNullOrWhiteSpace(this.Raw, nameof(this.Raw));
        }
    }
}
