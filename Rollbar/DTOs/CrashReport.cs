namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Text;

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

        [JsonProperty("data", Required = Required.Always)]
        public string Raw { get; private set; }

        public override void Validate()
        {
            Assumption.AssertNotNullOrWhiteSpace(this.Raw, nameof(this.Raw));
        }
    }
}
