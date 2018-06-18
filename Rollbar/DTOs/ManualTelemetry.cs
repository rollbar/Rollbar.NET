namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Implements manual/custom telemetry body
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.TelemetryBody" />
    public class ManualTelemetry
        : TelemetryBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManualTelemetry"/> class.
        /// </summary>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public ManualTelemetry(IDictionary<string, object> arbitraryKeyValuePairs) 
            : base(TelemetryType.Manual, arbitraryKeyValuePairs)
        {
        }
    }
}
