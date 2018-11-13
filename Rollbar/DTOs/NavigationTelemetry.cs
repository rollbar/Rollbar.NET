namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Implements navigation telemetry body.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.TelemetryBody" />
    public class NavigationTelemetry
        : TelemetryBody
    {
        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public static class ReservedProperties
        {
            /// <summary>
            /// Navigation source.
            /// </summary>
            public const string From = "from";
            /// <summary>
            /// Navigation destination.
            /// </summary>
            public const string To = "to";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationTelemetry"/> class.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public NavigationTelemetry(string from, string to, IDictionary<string, object> arbitraryKeyValuePairs = null)
            : base(TelemetryType.Navigation, arbitraryKeyValuePairs)
        {
            this.From = from;
            this.To = to;
        }

        /// <summary>
        /// Gets navigation-from.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        public string From
        {
            get { return this[ReservedProperties.From] as string; }
            private set { this[ReservedProperties.From] = value; }
        }

        /// <summary>
        /// Gets navigation-to.
        /// </summary>
        /// <value>
        /// To.
        /// </value>
        public string To
        {
            get { return this[ReservedProperties.To] as string; }
            private set { this[ReservedProperties.To] = value; }
        }
    }
}
