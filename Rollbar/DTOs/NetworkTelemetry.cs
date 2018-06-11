namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class NetworkTelemetry
        : TelemetryBody
    {
        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public static class ReservedProperties
        {
            public const string Subtype = "subtype";
            public const string Method = "method";
            public const string Url = "url";
            public const string StatusCode = "status_code";
            public const string StartTimestamp = "start_timestamp_ms";
            public const string EndTimestamp = "end_timestamp_ms";
        }

        public NetworkTelemetry(
            string method
            , string url
            , int statusCode
            , DateTime? eventStart
            , DateTime? eventEnd
            , string subtype = null
            , IDictionary<string, object> arbitraryKeyValuePairs = null
            )
            : base(TelemetryType.Network, arbitraryKeyValuePairs)
        {
            this.Method = method;
            this.Url = url;
            this.StatusCode = $"{statusCode}";
            if (eventStart.HasValue)
            {
                this.StartTimestamp = (long)eventStart.Value.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            }
            if (eventEnd.HasValue)
            {
                this.EndTimestamp = (long)eventEnd.Value.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            }
            if (string.IsNullOrWhiteSpace(subtype))
            {
                this.Subtype = subtype;
            }
        }

        public string Subtype
        {
            get { return this[ReservedProperties.Subtype] as string; }
            private set { this[ReservedProperties.Subtype] = value; }
        }

        public string Method
        {
            get { return this[ReservedProperties.Method] as string; }
            private set { this[ReservedProperties.Method] = value; }
        }

        public string Url
        {
            get { return this[ReservedProperties.Url] as string; }
            private set { this[ReservedProperties.Url] = value; }
        }

        public string StatusCode
        {
            get { return this[ReservedProperties.StatusCode] as string; }
            private set { this[ReservedProperties.StatusCode] = value; }
        }

        public long? StartTimestamp
        {
            get { return this[ReservedProperties.StartTimestamp] as long?; }
            private set { this[ReservedProperties.StartTimestamp] = value; }
        }

        public long? EndTimestamp
        {
            get { return this[ReservedProperties.EndTimestamp] as long?; }
            private set { this[ReservedProperties.EndTimestamp] = value; }
        }

    }
}
