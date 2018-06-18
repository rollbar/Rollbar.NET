namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Implements DOM telemetry body.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.TelemetryBody" />
    public class DomTelemetry
        : TelemetryBody
    {
        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public static class ReservedProperties
        {
            public const string Element = "element";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomTelemetry"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public DomTelemetry(string element, IDictionary<string, object> arbitraryKeyValuePairs = null)
            : base(TelemetryType.Dom, arbitraryKeyValuePairs)
        {
            this.Element = element;
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        public string Element
        {
            get { return this[ReservedProperties.Element] as string; }
            private set { this[ReservedProperties.Element] = value; }
        }

    }
}
