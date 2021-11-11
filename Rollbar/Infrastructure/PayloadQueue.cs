namespace Rollbar.Infrastructure
{
    using System;
    using System.Collections.Generic;

    using Rollbar.Diagnostics;

    /// <summary>
    /// Class PayloadQueue.
    /// </summary>
    internal class PayloadQueue
        : IPayloadQueue
    {
        /// <summary>
        /// The synchronize lock
        /// </summary>
        private readonly object _syncLock;

        /// <summary>
        /// The queue
        /// </summary>
        private readonly Queue<PayloadBundle> _queue;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly RollbarLogger? _logger;

        /// <summary>
        /// The client
        /// </summary>
        private RollbarClient? _client;

        /// <summary>
        /// The is released
        /// </summary>
        private bool _isReleased;

        /// <summary>
        /// Prevents a default instance of the <see cref="PayloadQueue"/> class from being created.
        /// </summary>
        private PayloadQueue()
        {
            this._syncLock = new object();
            this._queue = new Queue<PayloadBundle>();
            this._isReleased = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadQueue"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="client">The client.</param>
        public PayloadQueue(RollbarLogger logger, RollbarClient client)
        {
            Assumption.AssertNotNull(logger, nameof(logger));
            Assumption.AssertNotNull(client, nameof(client));
            Assumption.AssertTrue(object.ReferenceEquals(logger.Config, client.Config), nameof(client.Config));

            this._syncLock = new object();
            this._queue = new Queue<PayloadBundle>();
            this._logger = logger;
            this._client = client;
            this._isReleased = false;
        }

        /// <summary>
        /// Releases this instance.
        /// </summary>
        public void Release()
        {
            Assumption.AssertFalse(this._isReleased, nameof(this._isReleased));
            this._isReleased = true;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is released.
        /// </summary>
        /// <value><c>true</c> if this instance is released; otherwise, <c>false</c>.</value>
        public bool IsReleased { get { return this._isReleased; } }

        /// <summary>
        /// Gets or sets the next dequeue time.
        /// </summary>
        /// <value>The next dequeue time.</value>
        public DateTimeOffset NextDequeueTime { get; internal set; } = DateTimeOffset.Now;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public RollbarLogger Logger
        {
            get { return this._logger!; }
        }

        /// <summary>
        /// Updates the client.
        /// </summary>
        /// <param name="client">The client.</param>
        public void UpdateClient(RollbarClient client)
        {
            if (this._client == client)
            {
                return;
            }

            lock(this._syncLock)
            {
                this._client = client;
            }
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>The client.</value>
        public RollbarClient Client
        {
            get { return this._client!; }
        }

        /// <summary>
        /// Enqueues the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        public void Enqueue(PayloadBundle payload)
        {
            Assumption.AssertNotNull(payload, nameof(payload));

            if (payload == null)
            {
                return; // no one needs to enqueue "nothing"... 
            }

            lock (this._syncLock)
            {
                if (RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.ReportingQueueDepth == this._queue.Count)
                {
                    this._queue.Dequeue();
                }
                this._queue.Enqueue(payload);
            }
        }

        /// <summary>
        /// Peeks this instance.
        /// </summary>
        /// <returns>PayloadBundle, if any, otherwise null.</returns>
        public PayloadBundle? Peek()
        {
            lock(this._syncLock)
            {
                PayloadBundle? result = null;

                if (this._queue.Count > 0)
                {
                    result = this._queue.Peek();
                }

                return result;
            }
        }

        /// <summary>
        /// Dequeues this instance.
        /// </summary>
        /// <returns>PayloadBundle, if any, otherwise null.</returns>
        public PayloadBundle? Dequeue()
        {
            lock (this._syncLock)
            {
                PayloadBundle? result = null;

                if (this._queue.Count > 0)
                {
                    result = this._queue.Dequeue();

                    TimeSpan delta = RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.MaxReportsPerMinute.HasValue ? 
                        TimeSpan.FromTicks(TimeSpan.FromMinutes(1).Ticks / RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.MaxReportsPerMinute.Value)
                        : TimeSpan.Zero;
                    this.NextDequeueTime = DateTimeOffset.Now.Add(delta);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the payload count.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetPayloadCount()
        {
            lock (this._syncLock)
            {
                return this._queue.Count;
            }
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        /// <returns>IEnumerable&lt;PayloadBundle&gt; flushed payload bundles.</returns>
        public IEnumerable<PayloadBundle> Flush()
        {
            lock (this._syncLock)
            {
                IEnumerable<PayloadBundle> flushedBundles = 
                    new List<PayloadBundle>(this._queue.ToArray());

                this._queue.Clear();

                return flushedBundles;
            }
        }

        /// <summary>
        /// Gets or sets the access token queues metadata.
        /// </summary>
        /// <value>The access token queues metadata.</value>
        public AccessTokenQueuesMetadata? AccessTokenQueuesMetadata { get; set; }

        /// <summary>
        /// Gets the items to persist.
        /// </summary>
        /// <returns>PayloadBundle[].</returns>
        public PayloadBundle[] GetItemsToPersist()
        {
            PayloadBundle[] queueItems;

            lock (this._syncLock)
            {
                queueItems = this._queue.ToArray();
                this._queue.Clear();
            }

            return queueItems;
        }
    }
}
