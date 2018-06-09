namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class ErrorTelemetry
        : LogTelemetry
    {
        public ErrorTelemetry(Exception exception)
            : base(exception.Message)
        {
            this.Add("telemetry.exception", exception);
        }
    }
}
