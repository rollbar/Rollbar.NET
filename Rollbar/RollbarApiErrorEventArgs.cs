namespace Rollbar
{
    using System.Collections.Generic;
    using Rollbar.DTOs;
    using Rollbar.Diagnostics;

    public class RollbarApiErrorEventArgs
        : CommunicationEventArgs
    {
        public enum RollbarError
        {
            None = 0,
            BadRequest = 400,
            AccessDenied = 403,
            NotFound = 404,
            RequestEntityTooLarge = 413,
            UnprocessableEntity = 422,
            TooManyRequests = 429,
        }

        public class RollbarErrorDetails
        {
            private RollbarErrorDetails()
            {
            }

            public RollbarErrorDetails(RollbarError error, string description)
            {

            }

            public RollbarError Error { get; private set; }

            public string Description { get; private set; }
        }

        private static readonly Dictionary<int, RollbarErrorDetails> errorDetailsByCode = null;

        static RollbarApiErrorEventArgs()
        {
            RollbarErrorDetails[] errorDetails = new RollbarErrorDetails[]
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
                errorDetailsByCode.Add((int) item.Error, item);
            }
        }

        public RollbarApiErrorEventArgs(RollbarConfig config, Payload payload, RollbarResponse response) 
            : base(config, payload, response)
        {
            Assumption.AssertNotNull(response, nameof(response));
            Assumption.AssertGreaterThan(response.Error, 0, nameof(response.Error));
        }

        public int ErrorCode { get { return this.Response.Error; } }

        public RollbarError? ErrorType
        {
            get
            {
                RollbarErrorDetails error = GetErrorDetailsByCode(this.ErrorCode);

                if (error != null)
                {
                    return error.Error;
                }

                return null;
            }
        }

        public string ErrorDescription
        {
            get
            {
                RollbarErrorDetails error = GetErrorDetailsByCode(this.ErrorCode);

                if (error != null)
                {
                    return error.Description;
                }

                return string.Empty;
            }
        }

        private static RollbarErrorDetails GetErrorDetailsByCode(int code)
        {
            RollbarErrorDetails error = null;
            if (errorDetailsByCode.TryGetValue(code, out error))
            {
                return error;
            }

            return null;
        }

        public override string TraceAsString(string indent = "")
        {
            return base.TraceAsString(indent);
        }
    }
}
