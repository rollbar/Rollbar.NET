namespace Rollbar
{
    using System;

    /// <summary>
    /// Interface IRollbarInfrastructure
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    public interface IRollbarInfrastructure
    {
        /// <summary>
        /// Initializes the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        void Init(IRollbarInfrastructureConfig config);

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        bool IsInitialized
        {
            get;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the instance.
        /// </summary>
        /// <param name="immediately">if set to <c>true</c> stops this instance immediately without waiting to stop gracefully.</param>
        void Stop(bool immediately);

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        IRollbarInfrastructureConfig? Config
        {
            get;
        }

        /// <summary>
        /// Gets the queue controller.
        /// </summary>
        /// <value>The queue controller.</value>
        IRollbarQueueController? QueueController
        {
            get;
        }

        /// <summary>
        /// Gets the telemetry collector.
        /// </summary>
        /// <value>The telemetry collector.</value>
        IRollbarTelemetryCollector? TelemetryCollector
        {
            get;
        }

        /// <summary>
        /// Gets the connectivity monitor.
        /// </summary>
        /// <value>The connectivity monitor.</value>
        IRollbarConnectivityMonitor? ConnectivityMonitor
        {
            get;
        }
    }
}