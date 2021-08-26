namespace Rollbar.DTOs
{
    using System.Collections.Generic;

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
            /// <summary>
            /// The element
            /// </summary>
            public static readonly string Element = "element";
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DomTelemetry"/> class from being created.
        /// </summary>
        private DomTelemetry()
            : base(TelemetryType.Dom, null)
        {
            this.Element = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomTelemetry"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        public DomTelemetry(
            string element
            )
            : this(element, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomTelemetry"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public DomTelemetry(
            string element, 
            IDictionary<string, object?>? arbitraryKeyValuePairs
            )
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
            get { return (this[ReservedProperties.Element] as string)!; }
            private set { this[ReservedProperties.Element] = value; }
        }

    }
}
