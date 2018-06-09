namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class NavigationTelemetry
        : TelemetryBody
    {
        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public static class ReservedProperties
        {
            public const string From = "from";
            public const string To = "to";
        }

        public NavigationTelemetry(string from, string to, IDictionary<string, object> arbitraryKeyValuePairs = null)
            : base(TelemetryType.Navigation, arbitraryKeyValuePairs)
        {
            this.From = from;
            this.To = to;
        }

        public string From
        {
            get { return this[ReservedProperties.From] as string; }
            private set { this[ReservedProperties.From] = value; }
        }

        public string To
        {
            get { return this[ReservedProperties.To] as string; }
            private set { this[ReservedProperties.To] = value; }
        }
    }
}
