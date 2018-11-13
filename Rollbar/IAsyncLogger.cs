namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface IAsyncLogger
    /// </summary>
    /// <remarks>
    /// NOTE:
    /// All the logging methods of this interface return Task "promise".
    /// Hence, they can be either used as "fire-and-forget" logging
    /// or to be a/waited for before proceeding with the next processing step.
    /// The "a/waiting for" guarantees that the incoming/logged data is processed/packaged into
    /// a Rollbar Payload DTO and placed into a transmission queue for subsequent delivery 
    /// to the Rollbar API.
    /// 
    /// If you need to make sure that the logged data is actually delivered 
    /// (or could be given-up on the delivery due to a timeout) before your code proceeds further,
    /// use ILogger methods of a Blocking Logger returned by 
    /// IAsyncLogger.AsBlockingLogger(TimeSpan timeout) method.
    /// </remarks>
    public interface IAsyncLogger
    {
        /// <summary>
        /// Ases the blocking logger.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>ILogger.</returns>
        ILogger AsBlockingLogger(TimeSpan timeout);

        /// <summary>
        /// Logs the specified Rollbar Data DTO.
        /// </summary>
        /// <param name="rollbarData">The Rollbar Data DTO.</param>
        /// <returns>Task.</returns>
        Task Log(DTOs.Data rollbarData);

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom.</param>
        /// <returns>Task.</returns>
        Task Log(ErrorLevel level, object obj, IDictionary<string, object> custom = null);

        #region Convenience methods

        /// <summary>
        /// Logs the specified object as using critical level.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom.</param>
        /// <returns>Task.</returns>
        Task Critical(object obj, IDictionary<string, object> custom = null);

        /// <summary>
        /// Logs the specified object as using error level.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom.</param>
        /// <returns>Task.</returns>
        Task Error(object obj, IDictionary<string, object> custom = null);

        /// <summary>
        /// Logs the specified object as using warning level.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom.</param>
        /// <returns>Task.</returns>
        Task Warning(object obj, IDictionary<string, object> custom = null);

        /// <summary>
        /// Logs the specified object as using informational level.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom.</param>
        /// <returns>Task.</returns>
        Task Info(object obj, IDictionary<string, object> custom = null);

        /// <summary>
        /// Logs the specified object as using debug level.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom.</param>
        /// <returns>Task.</returns>
        Task Debug(object obj, IDictionary<string, object> custom = null);

        #endregion Convenience methods
    }
}
