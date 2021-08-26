namespace Rollbar
{
    using System.Collections.Generic;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Fixed depth queue of telemetry items.
    /// </summary>
    public class TelemetryQueue
    {
        private readonly object _syncLock;
        private readonly Queue<Telemetry> _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryQueue"/> class.
        /// </summary>
        public TelemetryQueue()
        {
            this._syncLock = new object();
            this._queue = new Queue<Telemetry>();
        }

        /// <summary>
        /// Gets or sets the queue depth.
        /// </summary>
        /// <value>
        /// The queue depth.
        /// </value>
        public int QueueDepth { get; set; } = 5;

        /// <summary>
        /// Gets the content of the queue.
        /// </summary>
        /// <returns></returns>
        public Telemetry[] GetQueueContent()
        {
            lock(this._syncLock)
            {
                return this._queue.ToArray();
            }
        }

        /// <summary>
        /// Enqueues the specified telemetry.
        /// </summary>
        /// <param name="telemetry">The telemetry.</param>
        internal void Enqueue(Telemetry telemetry)
        {
            Assumption.AssertNotNull(telemetry, nameof(telemetry));

            lock (this._syncLock)
            {
                while (this._queue.Count >= this.QueueDepth)
                {
                    this._queue.Dequeue();
                }
                this._queue.Enqueue(telemetry);
            }
        }

        /// <summary>
        /// Gets the items count.
        /// </summary>
        /// <returns></returns>
        public int GetItemsCount()
        {
            lock (this._syncLock)
            {
                return this._queue.Count;
            }
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public void Flush()
        {
            lock (this._syncLock)
            {
                this._queue.Clear();
            }
        }

        /// <summary>
        /// Peeks this instance.
        /// </summary>
        /// <returns></returns>
        internal Telemetry? Peek()
        {
            lock (this._syncLock)
            {
                Telemetry? result = null;

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
        /// <returns></returns>
        internal Telemetry? Dequeue()
        {
            lock (this._syncLock)
            {
                Telemetry? result = null;

                if (this._queue.Count > 0)
                {
                    result = this._queue.Dequeue();
                }

                return result;
            }
        }

    }

}
