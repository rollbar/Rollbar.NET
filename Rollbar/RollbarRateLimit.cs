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
        private static class RollbarTateLimitHeaders
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
        {
            this.WindowLimit = 
                httpResponseHeaders.ParseHeaderValueSafelyOrDefault<int>(
                    RollbarTateLimitHeaders.Limit, 
                    int.TryParse, 
                    this.WindowLimit
                    );
            this.WindowRemaining = 
                httpResponseHeaders.ParseHeaderValueSafelyOrDefault<int>(
                    RollbarTateLimitHeaders.Remaining, 
                    int.TryParse, 
                    this.WindowRemaining
                    );
            this.WindowEnd =
                httpResponseHeaders.ParseHeaderValueSafelyOrDefault<DateTimeOffset>(
                    RollbarTateLimitHeaders.Reset, 
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
