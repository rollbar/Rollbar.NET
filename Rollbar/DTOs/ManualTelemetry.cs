namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class ManualTelemetry
        : TelemetryBody
    {
        public ManualTelemetry(IDictionary<string, object> arbitraryKeyValuePairs) 
            : base(TelemetryType.Manual, arbitraryKeyValuePairs)
        {
        }
    }
}
