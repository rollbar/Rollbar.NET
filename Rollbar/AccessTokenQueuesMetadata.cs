[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Models metadata needed to keep track of a given Rollbar access token usage.
    /// </summary>
    internal class AccessTokenQueuesMetadata
    {
        internal static readonly TimeSpan accessTokenInitialDelay = TimeSpan.FromSeconds(30);

        private readonly HashSet<PayloadQueue> _queues = new HashSet<PayloadQueue>();

        /// <summary>
        /// Prevents a default instance of the <see cref="AccessTokenQueuesMetadata"/> class from being created.
        /// </summary>
        private AccessTokenQueuesMetadata()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenQueuesMetadata"/> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        public AccessTokenQueuesMetadata(string accessToken)
        {
            Assumption.AssertNotNullOrWhiteSpace(accessToken, nameof(accessToken));

            this.AccessToken = accessToken;
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the queues associated with a given Rollbar access token.
        /// </summary>
        /// <value>
        /// The queues.
        /// </value>
        public HashSet<PayloadQueue> Queues { get { return this._queues; } }

        /// <summary>
        /// Gets the next time token usage.
        /// </summary>
        /// <value>
        /// The next time token usage.
        /// </value>
        /// <remarks>
        /// This property, when has a value, places a restriction on the token usage 
        /// until or past the time specified by the property value.
        /// </remarks>
        public DateTimeOffset NextTimeTokenUsage { get; private set; } = DateTimeOffset.Now;

        /// <summary>
        /// Gets the token usage delay.
        /// </summary>
        /// <value>
        /// The token usage delay.
        /// </value>
        public TimeSpan TokenUsageDelay { get; private set; }

        /// <summary>
        /// Increments the token usage delay.
        /// </summary>
        public void IncrementTokenUsageDelay()
        {
            this.TokenUsageDelay += AccessTokenQueuesMetadata.accessTokenInitialDelay;
            DateTimeOffset nextTimeTokenUsageCandidate = DateTimeOffset.Now.Add(this.TokenUsageDelay);
            if (nextTimeTokenUsageCandidate > this.NextTimeTokenUsage)
            {
                this.NextTimeTokenUsage = nextTimeTokenUsageCandidate;
            }
        }

        /// <summary>
        /// Resets the token usage delay.
        /// </summary>
        public void ResetTokenUsageDelay()
        {
            this.TokenUsageDelay = TimeSpan.Zero;
            //this.NextTimeTokenUsage = DateTimeOffset.Now;
        }

        /// <summary>
        /// Updates the next time token usage.
        /// </summary>
        /// <param name="rollbarRateLimit">The rollbar rate limit.</param>
        public void UpdateNextTimeTokenUsage(RollbarRateLimit rollbarRateLimit)
        {
            if (rollbarRateLimit.WindowRemaining > 0)
            {
                this.NextTimeTokenUsage = DateTimeOffset.Now;
            }
            else
            {
                this.NextTimeTokenUsage = rollbarRateLimit.WindowEnd;
            }
        }
    }
}
