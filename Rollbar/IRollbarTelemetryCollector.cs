namespace Rollbar
{
    using Rollbar.DTOs;

    /// <summary>
    /// Defines interface of Rollbar telemetry collector.
    /// </summary>
    public interface IRollbarTelemetryCollector
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        IRollbarTelemetryOptions? Config { get; }

        /// <summary>
        /// Captures the specified telemetry.
        /// </summary>
        /// <param name="telemetry">The telemetry.</param>
        /// <returns></returns>
        IRollbarTelemetryCollector Capture(Telemetry telemetry);

        /// <summary>
        /// Gets the items count.
        /// </summary>
        /// <returns></returns>
        int GetItemsCount();

        /// <summary>
        /// Gets the content of the queue.
        /// </summary>
        /// <returns></returns>
        Telemetry[] GetQueueContent();

        /// <summary>
        /// Gets the queue depth.
        /// </summary>
        /// <value>
        /// The queue depth.
        /// </value>
        int QueueDepth { get; }

        /// <summary>
        /// Flushes the queue.
        /// </summary>
        IRollbarTelemetryCollector FlushQueue();

        /// <summary>
        /// Starts the auto-collection.
        /// </summary>
        IRollbarTelemetryCollector StartAutocollection();

        /// <summary>
        /// Stops the auto-collection.
        /// </summary>
        /// <param name="immediate">if set to <c>true</c> [immediate].</param>
        IRollbarTelemetryCollector StopAutocollection(bool immediate);

        /// <summary>
        /// Gets a value indicating whether this instance is auto-collecting.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is auto-collecting; otherwise, <c>false</c>.
        /// </value>
        bool IsAutocollecting { get; }
    }
}
