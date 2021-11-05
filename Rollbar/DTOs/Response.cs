namespace Rollbar.DTOs
{
    using System.Collections.Generic;

    /// <summary>
    /// Class Response.
    /// Implements the <see cref="Rollbar.DTOs.ExtendableDtoBase" />
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.ExtendableDtoBase" />
    public class Response
        : ExtendableDtoBase
    {
        /// <summary>
        /// Class ReservedProperties.
        /// </summary>
        public static class ReservedProperties
        {
            /// <summary>
            /// The status code
            /// </summary>
            public static readonly string StatusCode = "status_code";
            /// <summary>
            /// The headers
            /// </summary>
            public static readonly string Headers = "headers";

            /// <summary>
            /// The body
            /// </summary>
            public static readonly string Body = "body";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Response"/> class.
        /// </summary>
        public Response()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Response"/> class.
        /// </summary>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public Response(IDictionary<string, object?>? arbitraryKeyValuePairs) 
            : base(arbitraryKeyValuePairs)
        {
        }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>The status code.</value>
        public int? StatusCode
        {
            get
            {
                if (this.TryGetValue(ReservedProperties.StatusCode, out object? statusCode) 
                    && statusCode != null
                    )
                {
                    return (int) statusCode;
                }

                return null;
            }
            set { this[ReservedProperties.StatusCode] = value; }
        }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>The headers.</value>
        public IDictionary<string, string>? Headers
        {
            get { return this[ReservedProperties.Headers] as IDictionary<string, string>; }
            set { this[ReservedProperties.Headers] = value; }
        }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <value>The body.</value>
        public object? Body
        {
            get { return this[ReservedProperties.Body]; }
            set { this[ReservedProperties.Body] = value; }
        }

    }
}
