namespace Rollbar.Infrastructure
{
    using System;
    using System.Collections.Generic;

    internal interface IPayloadQueue
    {
        /// <summary>
        /// Enqueues the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        void Enqueue(PayloadBundle payload);

        /// <summary>
        /// Peeks this instance.
        /// </summary>
        /// <returns>PayloadBundle, if any, otherwise null.</returns>
        PayloadBundle? Peek();

        /// <summary>
        /// Dequeues this instance.
        /// </summary>
        /// <returns>PayloadBundle, if any, otherwise null.</returns>
        PayloadBundle? Dequeue();

        /// <summary>
        /// Gets the payload count.
        /// </summary>
        /// <returns>System.Int32.</returns>
        int GetPayloadCount();

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        /// <returns>IEnumerable&lt;PayloadBundle&gt; flushed payload bundles.</returns>
        IEnumerable<PayloadBundle> Flush();

        
        /// <summary>
        /// Gets the items to persist.
        /// </summary>
        /// <returns>PayloadBundle[].</returns>
        PayloadBundle[] GetItemsToPersist();


        /// <summary>
        /// Gets or sets the access token queues metadata.
        /// </summary>
        /// <value>The access token queues metadata.</value>
        AccessTokenQueuesMetadata? AccessTokenQueuesMetadata { get; set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        RollbarLogger Logger { get; }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>The client.</value>
        RollbarClient Client { get; }

        /// <summary>
        /// Updates the client.
        /// </summary>
        /// <param name="client">The client.</param>
        void UpdateClient(RollbarClient client);


        /// <summary>
        /// Gets or sets the next dequeue time.
        /// </summary>
        /// <value>The next dequeue time.</value>
        DateTimeOffset NextDequeueTime { get; }
        
        /// <summary>
        /// Releases this instance.
        /// </summary>
        void Release();

        /// <summary>
        /// Gets a value indicating whether this instance is released.
        /// </summary>
        /// <value><c>true</c> if this instance is released; otherwise, <c>false</c>.</value>
        bool IsReleased { get; }

        
    }
}