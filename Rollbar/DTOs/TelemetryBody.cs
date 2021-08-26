namespace Rollbar.DTOs
{
    using System.Collections.Generic;

    /// <summary>
    /// Base abstraction for implementing telemetry concrete bodies.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.ExtendableDtoBase" />
    public abstract class TelemetryBody
        : ExtendableDtoBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryBody"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        protected TelemetryBody(TelemetryType type, IDictionary<string, object?>? arbitraryKeyValuePairs) 
            : base(arbitraryKeyValuePairs)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public TelemetryType Type { get; private set; }
    }
}
