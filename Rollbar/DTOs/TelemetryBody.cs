namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public abstract class TelemetryBody
        : ExtendableDtoBase
    {
        protected TelemetryBody(TelemetryType type, IDictionary<string, object> arbitraryKeyValuePairs) 
            : base(arbitraryKeyValuePairs)
        {
            this.Type = type;
        }

        public TelemetryType Type { get; private set; }
    }
}
