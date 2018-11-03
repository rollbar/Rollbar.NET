[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// Implements disposable implementation of IRollbar.
    /// 
    /// All the logging methods implemented in synchronous "blocking" fashion.
    /// Hence, the payload is either delivered or a method timed out when
    /// the methods return.
    /// 
    /// </summary>
    internal class RollbarLoggerBlockingWrapper
        : IRollbar
        , IDisposable
    {
        private readonly RollbarLogger _asyncLogger = null;
        private readonly TimeSpan _timeout;


        private void Report(System.Exception e, ErrorLevel level, IDictionary<string, object> custom = null)
        {
            using (var signal = this.CreateSignalObject())
            {
                this._asyncLogger.Report(e, level, custom, this._timeout, signal);

                WaitAndCompleteReport(signal);
            }
        }

        private void Report(string message, ErrorLevel level, IDictionary<string, object> custom = null)
        {
            using (var signal = this.CreateSignalObject())
            {
                this._asyncLogger.Report(message, level, custom, this._timeout, signal);

                WaitAndCompleteReport(signal);
            }
        }

        private void Send(Data data)
        {
            using (var signal = this.CreateSignalObject())
            {
                this._asyncLogger.Report(data, data.Level.HasValue ? data.Level.Value : ErrorLevel.Info, null, this._timeout, signal);

                WaitAndCompleteReport(signal);
            }
        }

        private SemaphoreSlim CreateSignalObject()
        {
            SemaphoreSlim signal = 
                new SemaphoreSlim(initialCount: 0, maxCount: 1);

            return signal;
        }

        private void WaitAndCompleteReport(SemaphoreSlim signal)
        {
            if (!signal.Wait(this._timeout))
            {
                throw new TimeoutException("Posting a payload to the Rollbar API Service timed-out");
            }

            return;
        }

        public RollbarLoggerBlockingWrapper(RollbarLogger asyncLogger, TimeSpan timeout)
        {
            Assumption.AssertNotNull(asyncLogger, nameof(asyncLogger));

            this._asyncLogger = asyncLogger;
            this._timeout = timeout;
        }

        #region ILogger

        public ILogger AsBlockingLogger(TimeSpan timeout)
        {
            return new RollbarLoggerBlockingWrapper(this._asyncLogger, timeout);
        }

        public ILogger Log(Data data)
        {
            this.Send(data);

            return this;
        }

        public ILogger Log(ErrorLevel level, object obj, IDictionary<string, object> custom = null)
        {
            if (this.Config.LogLevel.HasValue && level < this.Config.LogLevel.Value)
            {
                // nice shortcut:
                return this;
            }

            return RollbarUtil.LogUsingProperObjectDiscovery(this, level, obj, custom);
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
            this.Report(error, ErrorLevel.Warning, custom);

            return this;
        }

        public ILogger Info(System.Exception error, IDictionary<string, object> custom = null)
        {
            this.Report(error, ErrorLevel.Info, custom);

            return this;
        }

        public ILogger Debug(System.Exception error, IDictionary<string, object> custom = null)
        {
            this.Report(error, ErrorLevel.Debug, custom);

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

        #region IRollbar

        public ILogger Logger => this;

        public IRollbarConfig Config
        {
            get { return this._asyncLogger.Config; }
        }

        public IRollbar Configure(IRollbarConfig settings)
        {
            Assumption.AssertNotNull(settings, nameof(settings));
            this._asyncLogger.Config.Reconfigure(settings);

            return this;
        }

        public IRollbar Configure(string accessToken)
        {
            return this.Configure(new RollbarConfig(accessToken));
        }

        public event EventHandler<RollbarEventArgs> InternalEvent
        {
            add { this._asyncLogger.InternalEvent += value; }
            remove { this._asyncLogger.InternalEvent -= value; }
        }

        #endregion IRollbar

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this._asyncLogger.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarLoggerBlockingWrapper() {
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
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

    }
}
