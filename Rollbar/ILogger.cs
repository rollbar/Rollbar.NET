namespace Rollbar
{
    using dto = Rollbar.DTOs;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// Defines ILogger interface.
    /// 
    /// NOTE: 
    /// 
    /// All the logging methods of this interface imply asynchronous/non-blocking implementation.
    /// However, the interface defines the method (ILogger AsBlockingLogger(TimeSpan timeout))
    /// that returns a synchronous implementation of ILogger.
    /// This approach allows for easier code refactoring when switching between 
    /// asynchronous and synchronous uses of the logger.
    /// 
    /// Normally, you would want to use asynchronous logging since it has virtually no instrumentation 
    /// overhead on your application execution performance at runtime. It has "fire and forget"
    /// approach to logging. However, in some specific situations, for example, while logging right before 
    /// exiting an application, you may want to use it synchronously so that the application
    /// does not quit before the logging completes.
    /// 
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Returns blocking/synchronous implementation of this ILogger.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>
        /// Blocking (fully synchronous) instance of an ILogger. 
        /// It either completes logging calls within the specified timeout
        /// or throws a TimeoutException.
        /// </returns>
        ILogger AsBlockingLogger(TimeSpan timeout);

        /// <summary>
        /// Logs the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger Log(dto.Data data);

        /// <summary>
        /// Logs using the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger Log(ErrorLevel level, object obj, IDictionary<string, object?>? custom = null);

        /// <summary>
        /// Logs the specified object as critical.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger Critical(object obj, IDictionary<string, object?>? custom = null);
        /// <summary>
        /// Logs the specified object as error.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger Error(object obj, IDictionary<string, object?>? custom = null);
        /// <summary>
        /// Logs the specified object as warning.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger Warning(object obj, IDictionary<string, object?>? custom = null);
        /// <summary>
        /// Logs the specified object as info.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger Info(object obj, IDictionary<string, object?>? custom = null);
        /// <summary>
        /// Logs the specified object as debug.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger Debug(object obj, IDictionary<string, object?>? custom = null);
    }
}
