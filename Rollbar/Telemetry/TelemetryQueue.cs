[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar.Telemetry
{
    using System;
    using System.Collections.Generic;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Fixed depth queue of telemetry items.
    /// </summary>
    public class TelemetryQueue
    {
        private readonly object _syncLock = null;
        private readonly Queue<Telemetry> _queue = null;
        public int QueueDepth { get; set; } = 5;

        public TelemetryQueue()
        {
            this._syncLock = new object();
            this._queue = new Queue<Telemetry>();
        }

        public IEnumerable<Telemetry> GetQueueContent()
        {
            lock(this._syncLock)
            {
                return this._queue.ToArray();
            }
        }

        internal void Enqueue(Telemetry telemetry)
        {
            Assumption.AssertNotNull(telemetry, nameof(telemetry));

            lock (this._syncLock)
            {
                if (this._queue.Count == this.QueueDepth)
                {
                    this._queue.Dequeue();
                }
                this._queue.Enqueue(telemetry);
            }
        }

        internal Telemetry Peek()
        {
            lock (this._syncLock)
            {
                Telemetry result = null;

                if (this._queue.Count > 0)
                {
                    result = this._queue.Peek();
                }

                return result;
            }
        }

        internal Telemetry Dequeue()
        {
            lock (this._syncLock)
            {
                Telemetry result = null;

                if (this._queue.Count > 0)
                {
                    result = this._queue.Dequeue();
                }

                return result;
            }
        }

        public int GetItemsCount()
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
