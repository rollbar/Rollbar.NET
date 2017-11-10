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

    }
}
