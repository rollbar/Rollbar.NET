using Rollbar.Diagnostics;
using Rollbar.DTOs;
using System;
using System.Collections.Generic;
using System.Net;

namespace Rollbar
{
    /// <summary>
    /// Implements disposable implementation of IRollbar.
    /// </summary>
    /// <seealso cref="Rollbar.IRollbar" />
    /// <seealso cref="System.IDisposable" />
    internal class RollbarLogger
        : IRollbar
        , IDisposable
    {
        private readonly RollbarConfig _config = null;
        private readonly PayloadQueue _payloadQueue = null;

        public event EventHandler<RollbarEventArgs> InternalEvent;

        internal RollbarLogger(bool isSingleton)
        {
            this.IsSingleton = isSingleton;
            this._config = new RollbarConfig(this);
            this._payloadQueue = new PayloadQueue(this);
            RollbarQueueController.Instance.Register(this._payloadQueue);
        }

        internal bool IsSingleton { get; private set; }

        internal PayloadQueue Queue
        {
            get { return this._payloadQueue; }
        }

        #region IRollbar

        public ILogger Logger => this;

        public RollbarConfig Config
        {
            get { return this._config; }
        }

        public IRollbar Configure(RollbarConfig settings)
        {
            this._config.Reconfigure(settings);

            return this;
        }

        public IRollbar Configure(string accessToken)
        {
            this._config.Reconfigure(new RollbarConfig(accessToken));

            return this;
        }

        #endregion IRollbar

        #region ILogger

        public ILogger Log(ErrorLevel level, object obj, IDictionary<string, object> custom = null)
        {
            return this.Log(level, obj.ToString(), custom);
        }

        public ILogger Log(ErrorLevel level, string msg, IDictionary<string, object> custom = null)
        {
            this.Report(msg, level, custom);

            return this;
        }


        public ILogger Critical(string msg, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Critical, msg, custom);
        }

        public ILogger Error(string msg, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Error, msg, custom);
        }

        public ILogger Warning(string msg, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Warning, msg, custom);
        }

        public ILogger Info(string msg, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Info, msg, custom);
        }

        public ILogger Debug(string msg, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Debug, msg, custom);
        }


        public ILogger Critical(System.Exception error, IDictionary<string, object> custom = null)
        {
            this.Report(error, ErrorLevel.Critical, custom);

            return this;
        }

        public ILogger Error(System.Exception error, IDictionary<string, object> custom = null)
        {
            this.Report(error, ErrorLevel.Error, custom);

            return this;
        }

        public ILogger Warning(System.Exception error, IDictionary<string, object> custom = null)
        {
            this.Report(error, ErrorLevel.Error, custom);

            return this;
        }

        public ILogger Info(System.Exception error, IDictionary<string, object> custom = null)
        {
            this.Report(error, ErrorLevel.Error, custom);

            return this;
        }

        public ILogger Debug(System.Exception error, IDictionary<string, object> custom = null)
        {
            this.Report(error, ErrorLevel.Error, custom);

            return this;
        }


        public ILogger Critical(ITraceable traceableObj, IDictionary<string, object> custom = null)
        {
            return this.Critical(traceableObj.TraceAsString(), custom);
        }

        public ILogger Error(ITraceable traceableObj, IDictionary<string, object> custom = null)
        {
            return this.Error(traceableObj.TraceAsString(), custom);
        }

        public ILogger Warning(ITraceable traceableObj, IDictionary<string, object> custom = null)
        {
            return this.Warning(traceableObj.TraceAsString(), custom);
        }

        public ILogger Info(ITraceable traceableObj, IDictionary<string, object> custom = null)
        {
            return this.Info(traceableObj.TraceAsString(), custom);
        }

        public ILogger Debug(ITraceable traceableObj, IDictionary<string, object> custom = null)
        {
            return this.Debug(traceableObj.TraceAsString(), custom);
        }



        public ILogger Critical(object obj, IDictionary<string, object> custom = null)
        {
            return this.Critical(obj.ToString(), custom);
        }

        public ILogger Error(object obj, IDictionary<string, object> custom = null)
        {
            return this.Error(obj.ToString(), custom);
        }

        public ILogger Warning(object obj, IDictionary<string, object> custom = null)
        {
            return this.Warning(obj.ToString(), custom);
        }

        public ILogger Info(object obj, IDictionary<string, object> custom = null)
        {
            return this.Info(obj.ToString(), custom);
        }

        public ILogger Debug(object obj, IDictionary<string, object> custom = null)
        {
            return this.Debug(obj.ToString(), custom);
        }

        #endregion ILogger

        private Guid? Report(System.Exception e, ErrorLevel? level = ErrorLevel.Error, IDictionary<string, object> custom = null)
        {
            return SendBody(new Body(e), level, custom);
        }

        private Guid? Report(string message, ErrorLevel? level = ErrorLevel.Error, IDictionary<string, object> custom = null)
        {
            return SendBody(new Body(new Message(message)), level, custom);
        }

        private Guid? SendBody(Body body, ErrorLevel? level, IDictionary<string, object> custom)
        {
            if (string.IsNullOrWhiteSpace(this._config.AccessToken) 
                || this._config.Enabled == false
                )
            {
                return null;
            }

            var guid = Guid.NewGuid();

            var data = new Data(this._config.Environment, body)
            {
                Custom = custom,
                Level = level ?? this._config.LogLevel
            };

            var payload = new Payload(this._config.AccessToken, data);
            payload.Data.GuidUuid = guid;
            payload.Data.Person = this._config.Person;

            if (this._config.Server != null)
            {
                payload.Data.Server = this._config.Server;
            }

            if (this._config.CheckIgnore != null 
                && this._config.CheckIgnore.Invoke(payload)
                )
            {
                return null;
            }

            this._config.Transform?.Invoke(payload);

            this._config.Truncate?.Invoke(payload);

            this._payloadQueue.Enqueue(payload);

            return guid;
        }

        protected virtual void OnRollbarEvent(RollbarEventArgs e)
        {
            EventHandler<RollbarEventArgs> handler = InternalEvent;

            if (handler != null)
            {
                handler(this, e);
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
                    RollbarQueueController.Instance.Unregister(this._payloadQueue);
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
        /// <remarks>
        /// This code added to correctly implement the disposable pattern.
        /// </remarks>
        public void Dispose()
        {
            // RollbarLogger type supports both paradigms: singleton-like (via RollbarLocator) and
            // multiple disposable instances (via RollbarFactory).
            // Here we want to make sure that the singleton instance is never disposed:
            Assumption.AssertTrue(!this.IsSingleton, nameof(this.IsSingleton));

            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

    }
}
