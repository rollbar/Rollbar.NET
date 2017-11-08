namespace Rollbar
{
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;

    internal class PayloadQueue
    {
        private readonly ConcurrentQueue<Payload> _queue = new ConcurrentQueue<Payload>();
        private const int maxRetries = 3;

        public void Enqueue(Payload payload)
        {
            Assumption.AssertNotNull(payload, nameof(payload));

            this._queue.Enqueue(payload);
        }

        public Payload Peek()
        {
            Payload result = null;

            int retriesCount = maxRetries;
            while (retriesCount > 0)
            {
                if (this._queue.TryPeek(out result))
                {
                    return result;
                }
                retriesCount--;
            }

            return null;
        }

        public Payload Dequeue()
        {
            Payload result = null;

            int retriesCount = maxRetries;
            while (retriesCount > 0)
            {
                if (this._queue.TryDequeue(out result))
                {
                    return result;
                }
                retriesCount--;
            }

            return null;
        }
    }
}
