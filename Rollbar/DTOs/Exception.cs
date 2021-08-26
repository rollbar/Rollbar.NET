namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Models Rollbar Exception DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    public class Exception
        : DtoBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Exception"/> class.
        /// </summary>
        /// <param name="class">The class.</param>
        public Exception(string @class)
            : this(@class, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exception"/> class.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="message">The message.</param>
        public Exception(string @class, string? message)
            : this(@class, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exception" /> class.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="message">The message.</param>
        /// <param name="description">The description.</param>
        public Exception(string @class, string? message, string? description)
        {
            this.Class = @class;
            this.Message = message;
            this.Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exception"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public Exception(System.Exception exception)
        {
            Assumption.AssertNotNull(exception, nameof(exception));

            this.Class = exception.GetType().FullName;
            this.Message = exception.Message;
        }

        /// <summary>
        /// Gets the class.
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        [JsonProperty("class", Required = Required.Always)]
        public string? Class { get; private set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Description { get; set; }
    }
}
