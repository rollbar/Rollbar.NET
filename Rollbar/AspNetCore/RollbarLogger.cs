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

    public class RollbarLogger
        : mslogging.ILogger
        , IDisposable
    {
        private readonly RollbarLoggerProvider _loggerProvider = null;

        //[ThreadStatic]
        //private static readonly IRollbar rollbar = RollbarFactory.CreateNew(false);
        //[ThreadStatic]
        //private int perThreadInstanceCount = 0;
        private readonly IRollbar _rollbar = null;

        private RollbarLogger()
        {
        }

        public RollbarLogger(string name, IRollbarConfig rollbarConfig)
        {
            this.Name = name;

            //perThreadInstanceCount++;
            //if (rollbar.Config == null || string.IsNullOrWhiteSpace(rollbar.Config.AccessToken))
            //{
            //    rollbar.Configure(rollbarConfig);
            //}
            this._rollbar = RollbarFactory.CreateNew(false).Configure(rollbarConfig);
        }

        public string Name { get; }

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
            //if (state == null)
            //{
            //    throw new ArgumentNullException(nameof(state));
            //}

            //return ScopeProperties.CreateFromState(state);

            return EmptyDisposable.Instance;
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns>
        ///   <c>true</c> if enabled.
        /// </returns>
        public bool IsEnabled(mslogging.LogLevel logLevel)
        {
            return true;
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
        public void Log<TState>(mslogging.LogLevel logLevel, mslogging.EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!this.IsEnabled(logLevel))
                return;

            // let's custom build the Data object that includes the exception 
            // along with the current HTTP request context:

            string message = null;
            if (formatter != null)
            {
                message = formatter(state, exception);
            }

            Rollbar.DTOs.Body payloadBody = null;
            if (exception != null)
                payloadBody = new DTOs.Body(exception);
            else if (message != null)
                payloadBody = new DTOs.Body(new DTOs.Message(message));

            Dictionary<string, object> customProperties = new Dictionary<string, object>();
            customProperties.Add(
                "LogEventID"
                , $"{eventId.Id}" + (string.IsNullOrWhiteSpace(eventId.Name) ? string.Empty : $" ({eventId.Name})")
                );
            if (exception != null && message != null)
            {
                customProperties.Add("LogMessage", message);
            }

            Rollbar.DTOs.Data data = new Rollbar.DTOs.Data(
                config: RollbarLocator.RollbarInstance.Config,
                body: payloadBody,
                custom: customProperties
                //request: new Rollbar.DTOs.Request(null, context.Request)
                )
            {
                Level = RollbarLogger.Convert(logLevel),
            };

            // log the Data object (the exception + the HTTP request data):
            Rollbar.RollbarLocator.RollbarInstance.Log(data);

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

        private class ScopeProperty
            : IDisposable
        {
            private string _key;
            private object _value;

            public ScopeProperty(string key, object value)
            {
                this._key = key;
                this._value = value;
            }

            #region IDisposable Support

            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).
                        this._key = null;
                        this._value = null;
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            // ~ScopeProperty() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //   Dispose(false);
            // }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }

            #endregion

        }

        private class ScopeProperties
            : IDisposable
        {
            private List<object> _properties = new List<object>();

            public List<object> Properties { get { return this._properties; } }

            public static IDisposable CreateFromState<TState>(TState state)
            {
                ScopeProperties scope = new ScopeProperties();

                if (state is IEnumerable<KeyValuePair<string, object>> messageProperties)
                {
                    foreach (var property in messageProperties)
                    {
                        if (String.IsNullOrEmpty(property.Key))
                            continue;

                        scope.AddProperty(property.Key, property.Value);
                    }
                }
                else
                {
                    scope.AddProperty("Scope", state);
                }

                return scope;
            }

            public void AddProperty(object property)
            {
                Properties.Add(property);
            }

            public void AddProperty(string key, object value)
            {
                AddProperty(new ScopeProperty(key, value));
            }

            public void Dispose()
            {
                var properties = _properties;
                if (properties != null)
                {
                    _properties = null;
                    foreach (var property in properties)
                    {
                        IDisposable disposable = property as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                    //perThreadInstanceCount--;
                    //if (perThreadInstanceCount == 0)
                    //{
                    //    this.rollbar.Dispose();
                    //}
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

        // This code added to correctly implement the disposable pattern.
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
