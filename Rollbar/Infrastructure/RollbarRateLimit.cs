namespace Rollbar.Infrastructure
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
        private static readonly TimeSpan defaultClientSuspendTimeSpan = TimeSpan.FromMinutes(1);

        private const int defaultPayloadsPerWindow = 5000;

        private readonly DateTimeOffset _timeStamp = DateTimeOffset.Now;

        /// <summary>
        /// Class RollbarTateLimitHeaders.
        /// </summary>
        internal static class RollbarRateLimitHeaders
        {
            /// <summary>
            /// The limit
            /// </summary>
            public const string Limit = "x-rate-limit-limit";
            /// <summary>
            /// The remaining count until the limit is reached
            /// </summary>
            public const string Remaining = "x-rate-limit-remaining";
            /// <summary>
            /// The reset time for the current limit window
            /// </summary>
            public const string Reset = "x-rate-limit-reset";

            /// <summary>
            /// The remaining seconds of the current limit window
            /// </summary>
            public const string RemainingSeconds = "x-rate-limit-remaining-seconds";
        }

        /// <summary>
        /// The window limit
        /// </summary>
        internal readonly int WindowLimit = 
            RollbarRateLimit.defaultPayloadsPerWindow;
        
        /// <summary>
        /// The current limit window end time
        /// </summary>
        internal readonly DateTimeOffset WindowEnd = 
            DateTimeOffset.Now.Add(RollbarRateLimit.defaultClientSuspendTimeSpan);

        /// <summary>
        /// The window remaining time span
        /// </summary>
        public readonly TimeSpan WindowRemainingTimeSpan = TimeSpan.Zero;

        /// <summary>
        /// The window remaining count until the limit is reached
        /// </summary>
        public readonly int WindowRemaining =
            RollbarRateLimit.defaultPayloadsPerWindow;

        /// <summary>
        /// Gets the client suspension time span.
        /// </summary>
        /// <value>The client suspension time span.</value>
        public TimeSpan ClientSuspensionTimeSpan { get; private set; }

        /// <summary>
        /// Gets the client suspension end.
        /// </summary>
        /// <value>The client suspension end.</value>
        public DateTimeOffset ClientSuspensionEnd { get; private set; }

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
            // Here we are relying on the preset initial/default values
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
            int windowRemainigSeconds = Convert.ToInt32(this.WindowRemainingTimeSpan.TotalSeconds);
            windowRemainigSeconds = 
                httpHeaders.ParseHeaderValueSafelyOrDefault<int>(
                    RollbarRateLimitHeaders.RemainingSeconds,
                    int.TryParse,
                    windowRemainigSeconds
                    );
            this.WindowRemainingTimeSpan = TimeSpan.FromSeconds(windowRemainigSeconds);

            // Now, let's perform calculations of rate limiting factors needed by a Rollbar notifier:

            this.ClientSuspensionTimeSpan = 
                RollbarRateLimit.defaultClientSuspendTimeSpan < this.WindowRemainingTimeSpan 
                ? RollbarRateLimit.defaultClientSuspendTimeSpan 
                : this.WindowRemainingTimeSpan;

            this.ClientSuspensionEnd = this._timeStamp.Add(this.ClientSuspensionTimeSpan);
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarRateLimit"/> class from being created.
        /// </summary>
        private RollbarRateLimit()
        {
        }
    }
}
