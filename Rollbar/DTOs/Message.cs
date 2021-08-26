namespace Rollbar.DTOs
{
    using System.Collections.Generic;
    using Rollbar.Common;
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
        public Message(string? body)
            : this(body, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public Message(
            string? body, 
            IDictionary<string, object?>? arbitraryKeyValuePairs
            )
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
        public string? Body
        {
            get { return this[ReservedProperties.Body] as string; }
            private set { this[ReservedProperties.Body] = value; }
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator GetValidator()
        {
            var validator = new Validator<Message, Message.MessageValidationRule>()
                    .AddValidation(
                        Message.MessageValidationRule.BodyRequired,
                        (message) => { return !string.IsNullOrWhiteSpace(message.Body); }
                        )
               ;

            return validator;
        }

        /// <summary>
        /// Enum MessageValidationRule
        /// </summary>
        public enum MessageValidationRule
        {
            /// <summary>
            /// The body required
            /// </summary>
            BodyRequired,
        }

    }
}
