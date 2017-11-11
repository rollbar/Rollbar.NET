namespace Rollbar
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class AccessTokenQueuesMetadata
    {
        internal static readonly TimeSpan accessTokenInitialDelay = TimeSpan.FromSeconds(30);

        private readonly HashSet<PayloadQueue> _queues = new HashSet<PayloadQueue>();

        private AccessTokenQueuesMetadata()
        {

        }

        public AccessTokenQueuesMetadata(string accessToken)
        {
            Assumption.AssertNotNullOrWhiteSpace(accessToken, nameof(accessToken));

            this.AccessToken = accessToken;
        }

        public string AccessToken { get; private set; }

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
        public DateTimeOffset? NextTimeTokenUsage { get; private set; }

        public TimeSpan TokenUsageDelay { get; private set; }

        public void IncrementTokenUsageDelay()
        {
            this.TokenUsageDelay += AccessTokenQueuesMetadata.accessTokenInitialDelay;
            this.NextTimeTokenUsage = DateTimeOffset.Now.Add(this.TokenUsageDelay);
        }

        public void ResetTokenUsageDelay()
        {
            this.TokenUsageDelay = TimeSpan.Zero;
            this.NextTimeTokenUsage = null;
        }
    }
}
