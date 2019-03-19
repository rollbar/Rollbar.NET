namespace Rollbar.NetCore.AspNet
{
    using System;
    using System.Collections.Generic;
    using mslogging = Microsoft.Extensions.Logging;
    using Rollbar.Diagnostics;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Implements RollbarLogger.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILogger" />
    /// <seealso cref="System.IDisposable" />
    public class RollbarLogger
            : mslogging.ILogger
            , IDisposable
    {
        private readonly string _name;

        private readonly RollbarOptions _rollbarOptions;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IRollbar _rollbar;

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
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public RollbarLogger(string name
            , IRollbarConfig rollbarConfig
            , RollbarOptions rollbarOptions
            , IHttpContextAccessor httpContextAccessor
            )
        {
            this._name = name;
            this._rollbarOptions = rollbarOptions;
            this._httpContextAccessor = httpContextAccessor;

            this._rollbar = RollbarFactory.CreateNew(rollbarConfig);
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
            {
                return;
            }

            if (object.Equals(state, default(TState)) && exception == null)
            {
                return;
            }

            if (RollbarScope.Current != null 
                && RollbarLocator.RollbarInstance.Config.MaxItems > 0
                )
            {
                RollbarScope.Current.IncrementLogItemsCount();
                if (RollbarScope.Current.LogItemsCount == RollbarLocator.RollbarInstance.Config.MaxItems)
                {
                    // the Rollbar SDK just reached MaxItems limit, report this fact and pause further logging within this scope: 
                    RollbarLocator.RollbarInstance.Warning(RollbarScope.MaxItemsReachedWarning);
                    return;
                }
                else if (RollbarScope.Current.LogItemsCount > RollbarLocator.RollbarInstance.Config.MaxItems)
                {
                    // the Rollbar SDK already exceeded MaxItems limit, do not log for this scope:
                    return;
                }
            }

            // let's custom build the Data object that includes the exception 
            // along with the current HTTP request context:

            string message = null;
            if (formatter != null)
            {
                message = formatter(state, exception);
            }

            IRollbarPackage rollbarPackage = null;
            if (exception != null)
            {
                rollbarPackage = new ExceptionPackage(exception, exception.Message);
            }
            else if (!string.IsNullOrWhiteSpace(message))
            {
                rollbarPackage = new MessagePackage(message, message);
            }
            else
            {
                return; //nothing to report...
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
            if (customProperties.Count > 0)
            {
                rollbarPackage = new CustomKeyValuePackageDecorator(rollbarPackage, customProperties);
            }

            var currentContext = GetCurrentContext();
            if (currentContext != null)
            {
                rollbarPackage = new RollbarHttpContextPackageDecorator(rollbarPackage, currentContext, true);
            }

            var rollbarErrorLevel = RollbarLogger.Convert(logLevel);

            RollbarLocator.RollbarInstance.Log(rollbarErrorLevel, rollbarPackage);
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
            Assumption.AssertTrue(!object.Equals(state, default(TState)), nameof(state));

            var scope = new RollbarScope(_name, state);
            scope.HttpContext = RollbarScope.Current?.HttpContext ?? new RollbarHttpContext();
            return RollbarScope.Push(scope);
        }

        private RollbarHttpContext GetCurrentContext()
        {
            var context = RollbarScope.Current?.HttpContext ?? new RollbarHttpContext();

            if (context.HttpAttributes == null 
                && this._httpContextAccessor != null 
                && this._httpContextAccessor.HttpContext != null
                )
            {
                context.HttpAttributes = new RollbarHttpAttributes(this._httpContextAccessor.HttpContext);
            }

            return context;
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
#pragma warning disable CA1063 // Implement IDisposable Correctly
        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
