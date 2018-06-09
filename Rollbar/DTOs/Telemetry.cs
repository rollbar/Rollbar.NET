namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rollbar.Diagnostics;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.ExtendableDtoBase" />
    public class Telemetry
        : ExtendableDtoBase
    {
        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public static class ReservedProperties
        {
            public const string Level = "level";
            public const string Type = "type";
            public const string Source = "source";
            public const string Timestamp = "timestamp_ms";
            public const string Body = "body";
        }

        public Telemetry(
            TelemetrySource source
            ,TelemetryLevel level
            ,TelemetryBody body
            ,IDictionary<string, object> arbitraryKeyValuePairs = null
            )
            : base(arbitraryKeyValuePairs)
        {
            Assumption.AssertNotNull(body, nameof(body));

            this.Timestamp = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            this.Level = level;
            this.Type = body.Type;
            this.Body = body;

            Validate();
        }

        public TelemetryLevel Level
        {
            get { return (TelemetryLevel) this._keyedValues[ReservedProperties.Level]; }
            private set { this._keyedValues[ReservedProperties.Level] = value; }
        }

        public TelemetryType Type
        {
            get { return (TelemetryType) this._keyedValues[ReservedProperties.Type]; }
            private set { this._keyedValues[ReservedProperties.Type] = value; }
        }

        public TelemetrySource Source
        {
            get { return (TelemetrySource)this._keyedValues[ReservedProperties.Source]; }
            private set { this._keyedValues[ReservedProperties.Source] = value; }
        }

        public long Timestamp
        {
            get { return (long) this._keyedValues[ReservedProperties.Timestamp]; }
            private set { this._keyedValues[ReservedProperties.Timestamp] = value; }
        }

        public TelemetryBody Body
        {
            get { return this._keyedValues[ReservedProperties.Body] as TelemetryBody; }
            private set { this._keyedValues[ReservedProperties.Body] = value; }
        }

        public override void Validate()
        {
            base.Validate();
        }
    }
}
