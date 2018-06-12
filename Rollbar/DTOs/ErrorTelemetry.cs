namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class ErrorTelemetry
        : LogTelemetry
    {
        public ErrorTelemetry(System.Exception exception, IDictionary<string, object> arbitraryKeyValuePairs = null)
            : base(TelemetryType.Error, exception.Message, arbitraryKeyValuePairs)
        {
            this.Add("telemetry.exception", exception);
        }
    }
}
