namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

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

        public DomTelemetry(string element, IDictionary<string, object> arbitraryKeyValuePairs = null)
            : base(TelemetryType.Dom, arbitraryKeyValuePairs)
        {
            this.Element = element;
        }

        public string Element
        {
            get { return this[ReservedProperties.Element] as string; }
            private set { this[ReservedProperties.Element] = value; }
        }

    }
}
