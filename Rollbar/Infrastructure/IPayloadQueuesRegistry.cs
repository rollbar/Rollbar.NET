namespace Rollbar.Infrastructure
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface IPayloadQueuesRegistry
    /// </summary>
    internal interface IPayloadQueuesRegistry
    {
        /// <summary>
        /// Registers the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <returns><c>true</c> if the queue was actually reqistered during this call, <c>false</c> if the queue was already registered prior to this call.</returns>
        bool Register(PayloadQueue queue);

        /// <summary>
        /// Unregisters the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <returns><c>true</c> if the queue was actually unregistered, <c>false</c> if the queue was not even registered in the first place.</returns>
        bool Unregister(PayloadQueue queue);

        /// <summary>
        /// Gets the payload queues.
        /// </summary>
        /// <value>The payload queues.</value>
        IReadOnlyCollection<PayloadQueue> GetPayloadQueues();

        /// <summary>
        /// Gets the payload queues count.
        /// </summary>
        /// <value>The payload queues count.</value>
        int PayloadQueuesCount { get; }
    }
}
