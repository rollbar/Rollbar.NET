namespace Rollbar
{
    using Rollbar.Common;
    using System;
    using System.Collections.Generic;
    using System.Net.Http.Headers;
    using System.Text;

    /// <summary>
    /// Class RollbarRateLimit.
    /// </summary>
    public class RollbarRateLimit
    {
        /// <summary>
        /// Class RollbarTateLimitHeaders.
        /// </summary>
        public static class RollbarRateLimitHeaders
        {
            /// <summary>
            /// The limit
            /// </summary>
            public const string Limit = "X-Rate-Limit-Limit";
            /// <summary>
            /// The remaining count until the limit is reached
            /// </summary>
            public const string Remaining = "X-Rate-Limit-Remaining";
            /// <summary>
            /// The reset time for the current limit window
            /// </summary>
            public const string Reset = "X-Rate-Limit-Reset";
        }

        /// <summary>
        /// The window limit
        /// </summary>
        public readonly int WindowLimit = 5000;
        /// <summary>
        /// The window remaining count until the limit is reached
        /// </summary>
        public readonly int WindowRemaining = 5000;
        /// <summary>
        /// The current limit window end time
        /// </summary>
        public readonly DateTimeOffset WindowEnd = DateTimeOffset.Now.AddMinutes(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarRateLimit"/> class.
        /// </summary>
        /// <param name="httpResponseHeaders">The HTTP response headers.</param>
        public RollbarRateLimit(HttpResponseHeaders httpResponseHeaders)
            : this(httpResponseHeaders as HttpHeaders)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarRateLimit"/> class.
        /// </summary>
        /// <param name="httpHeaders">The HTTP headers.</param>
        internal RollbarRateLimit(HttpHeaders httpHeaders)
        {
            // here we are relying on the preset initial/default values
            // of the rate limiting parameters in case any header we are
            // looking for is missing:

            this.WindowLimit =
                httpHeaders.ParseHeaderValueSafelyOrDefault<int>(
                    RollbarRateLimitHeaders.Limit, 
                    int.TryParse, 
                    this.WindowLimit
                    );
            this.WindowRemaining =
                httpHeaders.ParseHeaderValueSafelyOrDefault<int>(
                    RollbarRateLimitHeaders.Remaining, 
                    int.TryParse, 
                    this.WindowRemaining
                    );
            this.WindowEnd =
                httpHeaders.ParseHeaderValueSafelyOrDefault<DateTimeOffset>(
                    RollbarRateLimitHeaders.Reset, 
                    DateTimeUtil.TryParseFromUnixTimestampInSecondsString, 
                    this.WindowEnd
                    );
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarRateLimit"/> class from being created.
        /// </summary>
        private RollbarRateLimit()
        {
        }
    }
}
