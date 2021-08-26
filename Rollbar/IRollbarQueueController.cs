namespace Rollbar
{
    using System;

    /// <summary>
    /// Interface IRollbarQueueController
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    public interface IRollbarQueueController
    {
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
        /// Flushes the queues.
        /// </summary>
        void FlushQueues();
        /// <summary>
        /// Gets the total payload count.
        /// </summary>
        /// <returns>System.Int32.</returns>
        int GetTotalPayloadCount();
        /// <summary>
        /// Gets the payload count.
        /// </summary>
        /// <param name="rollbar">The rollbar.</param>
        /// <returns>System.Int32.</returns>
        int GetPayloadCount(IRollbar rollbar);
        /// <summary>
        /// Gets the payload count.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns>System.Int32.</returns>
        int GetPayloadCount(string accessToken);
        /// <summary>
        /// Gets the recommended timeout.
        /// </summary>
        /// <returns>TimeSpan.</returns>
        TimeSpan GetRecommendedTimeout();
        /// <summary>
        /// Gets the recommended timeout.
        /// </summary>
        /// <param name="rollbar">The rollbar.</param>
        /// <returns>TimeSpan.</returns>
        TimeSpan GetRecommendedTimeout(IRollbar rollbar);
        /// <summary>
        /// Gets the recommended timeout.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns>TimeSpan.</returns>
        TimeSpan GetRecommendedTimeout(string accessToken);

        /// <summary>
        /// Occurs when [internal event].
        /// </summary>
        event EventHandler<RollbarEventArgs>? InternalEvent;
    }
}