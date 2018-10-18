[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;

    internal class PayloadQueue
    {
        private readonly object _syncLock = null;
        private readonly Queue<Payload> _queue = null;
        private readonly RollbarLogger _logger = null;
        private readonly RollbarClient _client = null;
        private bool _isReleased;

        private PayloadQueue()
        {
        }

        public PayloadQueue(RollbarLogger logger, RollbarClient client)
        {
            Assumption.AssertNotNull(logger, nameof(logger));
            Assumption.AssertNotNull(client, nameof(client));
            Assumption.AssertTrue(object.ReferenceEquals(logger.Config, client.Config), nameof(client.Config));

            this._logger = logger;
            this._syncLock = new object();
            this._queue = new Queue<Payload>();
            this._client = client;
        }

        public void Release()
        {
            Assumption.AssertFalse(this._isReleased, nameof(this._isReleased));
            this._isReleased = true;
        }

        public bool IsReleased { get { return this._isReleased; } }

        public DateTimeOffset NextDequeueTime { get; internal set; }

        public RollbarLogger Logger
        {
            get { return this._logger; }
        }

        public RollbarClient Client
        {
            get { return this._client; }
        }

        public void Enqueue(Payload payload)
        {
            Assumption.AssertNotNull(payload, nameof(payload));

            lock (this._syncLock)
            {
                if (this._logger.Config.ReportingQueueDepth == this._queue.Count)
                {
                    this._queue.Dequeue();
                }
                this._queue.Enqueue(payload);
            }
        }

        public Payload Peek()
        {
            lock(this._syncLock)
            {
                Payload result = null;

                if (this._queue.Count > 0)
                {
                    result = this._queue.Peek();
                }

                return result;
            }
        }

        public Payload Dequeue()
        {
            lock (this._syncLock)
            {
                Payload result = null;

                if (this._queue.Count > 0)
                {
                    result = this._queue.Dequeue();

                    TimeSpan delta = TimeSpan.FromTicks(
                        TimeSpan.FromMinutes(1).Ticks / this.Logger.Config.MaxReportsPerMinute
                        );
                    this.NextDequeueTime = DateTimeOffset.Now.Add(delta);
                }

                return result;
            }
        }

        public int GetPayloadCount()
        {
            lock (this._syncLock)
            {
                return this._queue.Count;
            }
        }

        public void Flush()
        {
            lock (this._syncLock)
            {
                this._queue.Clear();
            }
        }
    }
}
