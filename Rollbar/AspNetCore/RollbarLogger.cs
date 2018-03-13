#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Reflection;
    using mslogging = Microsoft.Extensions.Logging;
    using Rollbar.Common;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Implements RollbarLogger.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILogger" />
    /// <seealso cref="System.IDisposable" />
    public class RollbarLogger
            : mslogging.ILogger
            , IDisposable
    {
        private readonly string _name = null;
        private readonly RollbarOptions _rollbarOptions = null;

        private readonly IRollbar _rollbar = null;

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarLogger" /> class from being created.
        /// </summary>
        private RollbarLogger()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLogger" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="rollbarOptions">The options.</param>
        public RollbarLogger(string name, IRollbarConfig rollbarConfig, RollbarOptions rollbarOptions)
        {
            this._name = name;
            this._rollbarOptions = rollbarOptions;

            this._rollbar = RollbarFactory.CreateNew(false).Configure(rollbarConfig);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get { return this._name; } }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns>
        ///   <c>true</c> if enabled.
        /// </returns>
        public bool IsEnabled(mslogging.LogLevel logLevel)
        {
            return _rollbarOptions.Filter(_name, logLevel);
        }

        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <c>string</c> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
        public void Log<TState>(
            mslogging.LogLevel logLevel
            , mslogging.EventId eventId
            , TState state
            , Exception exception
            , Func<TState, Exception, string> formatter
            )
        {
            if (!this.IsEnabled(logLevel))
                return;

            if (state == null && exception == null)
                return;

            var currentContext = GetCurrentContext();

            // let's custom build the Data object that includes the exception 
            // along with the current HTTP request context:

            string message = null;
            if (formatter != null)
            {
                message = formatter(state, exception);
            }

            Rollbar.DTOs.Body payloadBody = null;
            if (!string.IsNullOrWhiteSpace(message))
            {
                payloadBody = new DTOs.Body(new DTOs.Message(message));
            }
            else
            {
                payloadBody = new DTOs.Body(exception);
            }

            Dictionary<string, object> customProperties = new Dictionary<string, object>();
            customProperties.Add(
                "LogEventID"
                , $"{eventId.Id}" + (string.IsNullOrWhiteSpace(eventId.Name) ? string.Empty : $" ({eventId.Name})")
                );
            if (exception != null && message != null)
            {
                customProperties.Add("LogMessage", message);
            }

            var customRequestFields = new Dictionary<string, object>();
            customRequestFields.Add("httpRequestTimestamp", currentContext.Timestamp);
            customRequestFields.Add("httpRequestID", currentContext.HttpAttributes.RequestID);
            customRequestFields.Add("statusCode", currentContext.HttpAttributes.StatusCode);
            customRequestFields.Add("scheme", currentContext.HttpAttributes.Scheme);
            customRequestFields.Add("protocol", currentContext.HttpAttributes.Protocol);

            var requestDto = new Rollbar.DTOs.Request(customRequestFields, currentContext.HttpAttributes);

            Rollbar.DTOs.Data dataDto = new Rollbar.DTOs.Data(
                config: RollbarLocator.RollbarInstance.Config
                , body: payloadBody
                , custom: customProperties
                , request: requestDto
                )
            {
                Level = RollbarLogger.Convert(logLevel),
            };

            // log the Data object (the exception + the HTTP request data):
            Rollbar.RollbarLocator.RollbarInstance.Log(dataDto);
        }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>
        /// An IDisposable that ends the logical operation scope on dispose.
        /// </returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            Assumption.AssertTrue(state != null, nameof(state));

            var scope = new RollbarScope(_name, state);
            scope.HttpContext = RollbarScope.Current?.HttpContext ?? new RollbarHttpContext();
            return RollbarScope.Push(scope);
        }

        private RollbarHttpContext GetCurrentContext()
        {
            return RollbarScope.Current?.HttpContext ?? new RollbarHttpContext();
        }

        private static ErrorLevel Convert(mslogging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case mslogging.LogLevel.Critical:
                    return ErrorLevel.Critical;
                case mslogging.LogLevel.Error:
                    return ErrorLevel.Error;
                case mslogging.LogLevel.Warning:
                    return ErrorLevel.Warning;
                case mslogging.LogLevel.Trace:
                case mslogging.LogLevel.Debug:
                    return ErrorLevel.Debug;
                case mslogging.LogLevel.Information:
                case mslogging.LogLevel.None:
                default:
                    return ErrorLevel.Info;
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; 
        ///   <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                    this._rollbar.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarLogger() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}

#endif
