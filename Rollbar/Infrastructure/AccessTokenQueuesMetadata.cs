namespace Rollbar.Infrastructure
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class AccessTokenQueuesMetadata.
    /// Implements the <see cref="Rollbar.Infrastructure.IPayloadQueuesRegistry" /> with thread-safety.
    /// </summary>
    /// Implements the <see cref="Rollbar.Infrastructure.IPayloadQueuesRegistry" /> with thread-safety.
    /// <seealso cref="Rollbar.Infrastructure.IPayloadQueuesRegistry" />
    internal class AccessTokenQueuesMetadata
        : IPayloadQueuesRegistry
    {

        /// <summary>
        /// The queues
        /// </summary>
        private readonly HashSet<PayloadQueue> _queues = new HashSet<PayloadQueue>();

        private readonly object _queuesSyncLock = new object();

        /// <summary>
        /// Prevents a default instance of the <see cref="AccessTokenQueuesMetadata"/> class from being created.
        /// </summary>
        private AccessTokenQueuesMetadata()
        {
            this.AccessToken = string.Empty;
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
        /// Updates the next time token usage.
        /// </summary>
        /// <param name="rollbarRateLimit">The rollbar rate limit.</param>
        public void UpdateNextTimeTokenUsage(RollbarRateLimit? rollbarRateLimit)
        {
            if (rollbarRateLimit == null)
            {
                //this is a special case when the payload transmission was configured to be omitted:
                this.IsTransmissionSuspended = false;
                this.NextTimeTokenUsage = DateTimeOffset.Now;
                return;
            }

            if (rollbarRateLimit.WindowRemaining > 0)
            {
                this.IsTransmissionSuspended = false;
                this.NextTimeTokenUsage = DateTimeOffset.Now;
            }
            else
            {
                this.IsTransmissionSuspended = true;
                this.NextTimeTokenUsage = rollbarRateLimit.ClientSuspensionEnd;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is transmission suspended.
        /// </summary>
        /// <value><c>true</c> if this instance is transmission suspended; otherwise, <c>false</c>.</value>
        public bool IsTransmissionSuspended { get; private set; }


        #region IPayloadQueuesRegistry

        /// <summary>
        /// Registers the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <returns><c>true</c> if the queue was actually registered during this call, <c>false</c> if the queue was already registered prior to this call.</returns>
        public bool Register(PayloadQueue? queue)
        {
            if(queue == null)
            {
                return true; // nothing to register - nothing bad...
            }

            lock(this._queuesSyncLock)
            {
                if (this._queues.Add(queue))
                {
                    queue.AccessTokenQueuesMetadata = this;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Unregisters the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <returns><c>true</c> if the queue was actually unregistered, <c>false</c> if the queue was not even registered in the first place.</returns>
        public bool Unregister(PayloadQueue? queue)
        {
            if(queue == null)
            {
                return true; // nothing to unregister - nothing bad...
            }

            lock(this._queuesSyncLock)
            {
                if (this._queues.Remove(queue))
                {
                    queue.AccessTokenQueuesMetadata = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the payload queues.
        /// </summary>
        /// <value>The payload queues.</value>
        public IReadOnlyCollection<PayloadQueue> GetPayloadQueues()
        {
            lock (this._queuesSyncLock)
            {
                return this._queues.ToArray();
            }
        }

        /// <summary>
        /// Gets the payload queues count.
        /// </summary>
        /// <value>The payload queues count.</value>
        public int PayloadQueuesCount
         {
            get
            {
                lock(this._queuesSyncLock)
                {
                    return this._queues.Count;
                }
            }
        }

        #endregion IPayloadQueuesRegistry
    }
}
