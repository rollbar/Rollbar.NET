namespace Rollbar
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines ILogger interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Log(ErrorLevel level, object obj, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="msg">The message.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Log(ErrorLevel level, string msg, IDictionary<string, object> custom = null);


        /// <summary>
        /// Logs the specified message as critical.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Critical(string msg, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified message as error.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Error(string msg, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified message as warning.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Warning(string msg, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified message as info.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Info(string msg, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified message as debug.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Debug(string msg, IDictionary<string, object> custom = null);

        /// <summary>
        /// Logs the specified error as critical.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Critical(Exception error, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified error as error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Error(Exception error, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified error as warning.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Warning(Exception error, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified error as info.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Info(Exception error, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified error as debug.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Debug(Exception error, IDictionary<string, object> custom = null);

        /// <summary>
        /// Logs the specified object as critical.
        /// </summary>
        /// <param name="traceableObj">The traceable object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Critical(ITraceable traceableObj, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified object as error.
        /// </summary>
        /// <param name="traceableObj">The traceable object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Error(ITraceable traceableObj, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified object as warning.
        /// </summary>
        /// <param name="traceableObj">The traceable object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Warning(ITraceable traceableObj, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified object as info.
        /// </summary>
        /// <param name="traceableObj">The traceable object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Info(ITraceable traceableObj, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified object as debug.
        /// </summary>
        /// <param name="traceableObj">The traceable object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Debug(ITraceable traceableObj, IDictionary<string, object> custom = null);


        /// <summary>
        /// Logs the specified object as critical.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Critical(object obj, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified object as error.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Error(object obj, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified object as warning.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Warning(object obj, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified object as info.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Info(object obj, IDictionary<string, object> custom = null);
        /// <summary>
        /// Logs the specified object as debug.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns></returns>
        ILogger Debug(object obj, IDictionary<string, object> custom = null);
    }
}
