namespace Rollbar.DTOs
{
    using System.Collections.Generic;

    /// <summary>
    /// Implements error telemetry body
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.LogTelemetry" />
    public class ErrorTelemetry
        : LogTelemetry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorTelemetry"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public ErrorTelemetry(
            System.Exception exception
            )
            : this(exception, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorTelemetry"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public ErrorTelemetry(
            System.Exception exception, 
            IDictionary<string, object?>? arbitraryKeyValuePairs
            )
            : base(TelemetryType.Error, exception.Message, arbitraryKeyValuePairs)
        {
            this.Add("telemetry.exception", exception);
        }
    }
}
