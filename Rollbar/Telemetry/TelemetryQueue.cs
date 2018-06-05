[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar.Telemetry
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;

    public class TelemetryQueue
    {
        private readonly object _syncLock = null;
        private readonly Queue<TelemetryData> _queue = null;
        public int QueueDepth { get; set; } = 5;

        public TelemetryQueue()
        {
            this._syncLock = new object();
            this._queue = new Queue<TelemetryData>();
        }

        public IEnumerable<TelemetryData> GetQueueContent()
        {
            lock(this._syncLock)
            {
                return this._queue.ToArray();
            }
        }

        public void Enqueue(TelemetryData telemetryData)
        {
            Assumption.AssertNotNull(telemetryData, nameof(telemetryData));

            lock (this._syncLock)
            {
                if (this._queue.Count == this.QueueDepth)
                {
                    this._queue.Dequeue();
                }
                this._queue.Enqueue(telemetryData);
            }
        }

        public TelemetryData Peek()
        {
            lock (this._syncLock)
            {
                TelemetryData result = null;

                if (this._queue.Count > 0)
                {
                    result = this._queue.Peek();
                }

                return result;
            }
        }

        public TelemetryData Dequeue()
        {
            lock (this._syncLock)
            {
                TelemetryData result = null;

                if (this._queue.Count > 0)
                {
                    result = this._queue.Dequeue();
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
