namespace Rollbar
{
    using System.Collections.Generic;

    using Rollbar.DTOs;
    using Rollbar.Diagnostics;
    using Rollbar.Infrastructure;

    /// <summary>
    /// Models a Rollbar API error event.
    /// </summary>
    /// <seealso cref="Rollbar.CommunicationEventArgs" />
    public class RollbarApiErrorEventArgs
        : CommunicationEventArgs
    {
        /// <summary>
        /// Known error codes.
        /// </summary>
        public enum RollbarError
        {
            /// <summary>
            /// Not an error.
            /// </summary>
            None = 0,
            /// <summary>
            /// The bad request
            /// </summary>
            BadRequest = 400,
            /// <summary>
            /// The access denied
            /// </summary>
            AccessDenied = 403,
            /// <summary>
            /// Resource not found
            /// </summary>
            NotFound = 404,
            /// <summary>
            /// The request entity too large
            /// </summary>
            RequestEntityTooLarge = 413,
            /// <summary>
            /// The unprocessable entity
            /// </summary>
            UnprocessableEntity = 422,
            /// <summary>
            /// Too many requests
            /// </summary>
            TooManyRequests = 429,
        }

        /// <summary>
        /// 
        /// </summary>
        public class RollbarErrorDetails
        {
            private RollbarErrorDetails()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RollbarErrorDetails"/> class.
            /// </summary>
            /// <param name="error">The error.</param>
            /// <param name="description">The description.</param>
            public RollbarErrorDetails(RollbarError error, string description)
            {
                this.Error = error;
                this.Description = description;
            }

            /// <summary>
            /// Gets the error.
            /// </summary>
            /// <value>
            /// The error.
            /// </value>
            public RollbarError Error { get; private set; }

            /// <summary>
            /// Gets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string? Description { get; private set; }
        }

        private static readonly Dictionary<int, RollbarErrorDetails> errorDetailsByCode;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3963:\"static\" fields should be initialized inline", Justification = "Keeps code more maintainable.")]
        static RollbarApiErrorEventArgs()
        {
            RollbarErrorDetails[] errorDetails = new []
            {
                new RollbarErrorDetails(
                    RollbarError.BadRequest,
                    "The request was malformed and could not be parsed."
                    ),
                new RollbarErrorDetails(
                    RollbarError.AccessDenied,
                    "Access token was missing, invalid, or does not have the necessary permissions."
                    ),
                new RollbarErrorDetails(
                    RollbarError.NotFound,
                    "The requested resource was not found. This response will be returned if the URL is entirely invalid (i.e. /asdf), or if it is a URL that could be valid but is referencing something that does not exist (i.e. /item/12345)."
                    ),
                new RollbarErrorDetails(
                    RollbarError.RequestEntityTooLarge,
                    "The request exceeded the maximum size of 128KB."
                    ),
                new RollbarErrorDetails(
                    RollbarError.UnprocessableEntity,
                    "The request was parseable (i.e. valid JSON), but some parameters were missing or otherwise invalid."
                    ),
                new RollbarErrorDetails(
                    RollbarError.TooManyRequests,
                    "If rate limiting is enabled for your access token, this return code signifies that the rate limit has been reached and the item was not processed."
                    ),
            };

            errorDetailsByCode = new Dictionary<int, RollbarErrorDetails>(errorDetails.Length);

            foreach (var item in errorDetails)
            {
                errorDetailsByCode.Add((int)item.Error, item);
            }
        }

        internal RollbarApiErrorEventArgs(RollbarLogger logger, Payload? payload, RollbarResponse response)
            : base(logger, payload, response)
        {
            Assumption.AssertNotNull(response, nameof(response));
            Assumption.AssertGreaterThan(response.Error, 0, nameof(response.Error));
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public int ErrorCode { get { return this.Response.Error; } }

        /// <summary>
        /// Gets the type of the error.
        /// </summary>
        /// <value>
        /// The type of the error.
        /// </value>
        public RollbarError? ErrorType
        {
            get
            {
                RollbarErrorDetails? error = GetErrorDetailsByCode(this.ErrorCode);

                if (error != null)
                {
                    return error.Error;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the error description.
        /// </summary>
        /// <value>
        /// The error description.
        /// </value>
        public string? ErrorDescription
        {
            get
            {
                RollbarErrorDetails? error = GetErrorDetailsByCode(this.ErrorCode);

                if (error != null)
                {
                    return error.Description;
                }

                return string.Empty;
            }
        }

        private static RollbarErrorDetails? GetErrorDetailsByCode(int code)
        {
            if (errorDetailsByCode.TryGetValue(code, out RollbarErrorDetails? error))
            {
                return error;
            }

            return null;
        }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>
        /// String rendering of this instance.
        /// </returns>
        public override string TraceAsString(string indent)
        {
            return base.TraceAsString(indent);
        }
    }
}
