namespace Rollbar.DTOs
{
    using System.Collections.Generic;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Models Rollbar Message DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.ExtendableDtoBase" />
    public class Message 
        : ExtendableDtoBase
    {
        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public static class ReservedProperties
        {
            /// <summary>
            /// The body
            /// </summary>
            public static readonly string Body = "body";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public Message(string body, IDictionary<string, object> arbitraryKeyValuePairs = null)
            : base(arbitraryKeyValuePairs)
        {
            Body = body;

            Validate();
        }

        /// <summary>
        /// Gets the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public string Body
        {
            get { return this._keyedValues[ReservedProperties.Body] as string; }
            private set { this._keyedValues[ReservedProperties.Body] = value; }
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public override void Validate()
        {
            Assumption.AssertNotNullOrWhiteSpace(this.Body, nameof(this.Body));
        }

    }
}
