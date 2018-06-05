namespace Rollbar.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class TelemetryData
    {
        public DateTimeOffset Timestamp 
            => DateTimeOffset.Now;

        public Dictionary<TelemetryAttribute, object> TelemetrySnapshot 
            => new Dictionary<TelemetryAttribute, object>();
    }
}
