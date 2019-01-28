namespace Rollbar.DTOs
{
    using System.Collections.Generic;

    /// <summary>
    /// Implements log telemetry body.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.TelemetryBody" />
    public class LogTelemetry
        : TelemetryBody
    {
        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public static class ReservedProperties
        {
            /// <summary>
            /// The message
            /// </summary>
            public static readonly string Message = "message";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogTelemetry"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public LogTelemetry(string message, IDictionary<string, object> arbitraryKeyValuePairs = null)
            : this(TelemetryType.Log, message, arbitraryKeyValuePairs)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogTelemetry"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="message">The message.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        protected LogTelemetry(TelemetryType type, string message, IDictionary<string, object> arbitraryKeyValuePairs = null)
            : base(type, arbitraryKeyValuePairs)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message
        {
            get { return this[ReservedProperties.Message] as string; }
            set { this[ReservedProperties.Message] = value; }
        }

    }
}
