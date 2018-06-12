namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class LogTelemetry
        : TelemetryBody
    {
        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public static class ReservedProperties
        {
            public const string Message = "message";
        }

        public LogTelemetry(string message, IDictionary<string, object> arbitraryKeyValuePairs = null)
            : this(TelemetryType.Log, message, arbitraryKeyValuePairs)
        {
        }

        protected LogTelemetry(TelemetryType type, string message, IDictionary<string, object> arbitraryKeyValuePairs = null)
            : base(type, arbitraryKeyValuePairs)
        {
            this.Message = message;
        }

        public string Message
        {
            get { return this[ReservedProperties.Message] as string; }
            private set { this[ReservedProperties.Message] = value; }
        }

    }
}
