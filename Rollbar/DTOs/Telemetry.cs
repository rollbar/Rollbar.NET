namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using Rollbar.Common;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Implements Telemetry DTO
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.ExtendableDtoBase" />
    public class Telemetry
        : ExtendableDtoBase
    {
        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public static class ReservedProperties
        {
            /// <summary>
            /// The level
            /// </summary>
            public static readonly string Level = "level";
            /// <summary>
            /// The type
            /// </summary>
            public static readonly string Type = "type";
            /// <summary>
            /// The source
            /// </summary>
            public static readonly string Source = "source";
            /// <summary>
            /// The timestamp
            /// </summary>
            public static readonly string Timestamp = "timestamp_ms";
            /// <summary>
            /// The body
            /// </summary>
            public static readonly string Body = "body";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Telemetry"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="level">The level.</param>
        /// <param name="body">The body.</param>
        public Telemetry(
            TelemetrySource source
            , TelemetryLevel level
            , TelemetryBody body
            )
            : this(source, level, body, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Telemetry"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="level">The level.</param>
        /// <param name="body">The body.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public Telemetry(
            TelemetrySource source
            ,TelemetryLevel level
            ,TelemetryBody body
            ,IDictionary<string, object?>? arbitraryKeyValuePairs
            )
            : base(arbitraryKeyValuePairs)
        {
            Assumption.AssertNotNull(body, nameof(body));

            this.Timestamp = DateTimeUtil.ConvertToUnixTimestampInMilliseconds(DateTime.UtcNow);
            this.Source = source;
            this.Level = level;
            this.Type = body.Type;
            this.Body = body;

            Validate();
        }

        /// <summary>
        /// Gets the level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public TelemetryLevel Level
        {
            get { return (TelemetryLevel) this[ReservedProperties.Level]!; }
            private set { this[ReservedProperties.Level] = value; }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public TelemetryType Type
        {
            get { return (TelemetryType) this[ReservedProperties.Type]!; }
            private set { this[ReservedProperties.Type] = value; }
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public TelemetrySource Source
        {
            get { return (TelemetrySource)this[ReservedProperties.Source]!; }
            private set { this[ReservedProperties.Source] = value; }
        }

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public long Timestamp
        {
            get { return (long) this[ReservedProperties.Timestamp]!; }
            private set { this[ReservedProperties.Timestamp] = value; }
        }

        /// <summary>
        /// Gets the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public TelemetryBody Body
        {
            get { return (this[ReservedProperties.Body] as TelemetryBody)!; }
            private set { this[ReservedProperties.Body] = value; }
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator GetValidator()
        {
            var validator = new Validator<Telemetry, Telemetry.TelemetryValidationRule>()
                    .AddValidation(
                        Telemetry.TelemetryValidationRule.BodyRequired,
                        (telemetry) => { return (telemetry.Body != null); }
                        )
                    .AddValidation(
                        Telemetry.TelemetryValidationRule.ValidBodyExpected,
#pragma warning disable CA1307 // Specify StringComparison for clarity
                        (config) => { return this.Body.GetType().Name.Contains(this.Type.ToString()); }
#pragma warning restore CA1307 // Specify StringComparison for clarity
                        )
               ;

            return validator;
        }


        /// <summary>
        /// Enum TelemetryValidationRule
        /// </summary>
        public enum TelemetryValidationRule
        {
            /// <summary>
            /// The body required
            /// </summary>
            BodyRequired,

            /// <summary>
            /// The valid body expected
            /// </summary>
            ValidBodyExpected,
        }
    }
}
