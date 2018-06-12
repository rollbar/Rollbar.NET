[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using Rollbar.Telemetry;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements disposable implementation of IRollbar.
    /// 
    /// All the logging methods implemented in async "fire-and-forget" fashion.
    /// Hence, the payload is not yet delivered to the Rollbar API service when
    /// the methods return.
    /// 
    /// </summary>
    /// <seealso cref="Rollbar.IRollbar" />
    /// <seealso cref="System.IDisposable" />
    internal class RollbarLogger
        : IRollbar
        , IDisposable
    {
        private readonly object _syncRoot = new object();
        private readonly TaskScheduler _nativeTaskScheduler = null;

        private readonly RollbarConfig _config = null;
        private readonly PayloadQueue _payloadQueue = null;

        public event EventHandler<RollbarEventArgs> InternalEvent;

        internal RollbarLogger(bool isSingleton)
        {
            try
            {
                this._nativeTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch(InvalidOperationException ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                // it could be a valid case in some environments:
                this._nativeTaskScheduler = null;
            }

            if (!TelemetryCollector.Instance.IsAutocollecting)
            {
                TelemetryCollector.Instance.StartAutocollection();
            }

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

        public IRollbar Configure(IRollbarConfig settings)
        {
            this._config.Reconfigure(settings);

            return this;
        }

        public IRollbar Configure(string accessToken)
        {
            return this.Configure(new RollbarConfig(accessToken));
        }

        #endregion IRollbar

        #region ILogger

        public ILogger AsBlockingLogger(TimeSpan timeout)
        {
            return new RollbarLoggerBlockingWrapper(this, timeout);
        }


        public ILogger Log(Data data)
        {
            SendAsync(data);

            return this;
        }

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

        #region IRollbar explicitly

        IRollbarConfig IRollbar.Config { get { return this.Config; } }

        ILogger IRollbar.Logger { get { return this; } }

        IRollbar IRollbar.Configure(IRollbarConfig settings)
        {
            return this.Configure(settings);
        }

        IRollbar IRollbar.Configure(string accessToken)
        {
            return this.Configure(accessToken);
        }

        event EventHandler<RollbarEventArgs> IRollbar.InternalEvent
        {
            add
            {
                this.InternalEvent += value;
            }

            remove
            {
                this.InternalEvent -= value;
            }
        }

        #endregion IRollbar explicitly

        #region ILogger explicitly

        ILogger ILogger.AsBlockingLogger(TimeSpan timeout)
        {
            return this.AsBlockingLogger(timeout);
        }

        ILogger ILogger.Log(Data data)
        {
            return this.Log(data);
        }

        ILogger ILogger.Log(ErrorLevel level, object obj, IDictionary<string, object> custom)
        {
            return this.Log(level, obj, custom);
        }

        ILogger ILogger.Log(ErrorLevel level, string msg, IDictionary<string, object> custom)
        {
            return this.Log(level, msg, custom);
        }

        ILogger ILogger.Critical(string msg, IDictionary<string, object> custom)
        {
            return this.Critical(msg, custom);
        }

        ILogger ILogger.Error(string msg, IDictionary<string, object> custom)
        {
            return this.Error(msg, custom);
        }

        ILogger ILogger.Warning(string msg, IDictionary<string, object> custom)
        {
            return this.Warning(msg, custom);
        }

        ILogger ILogger.Info(string msg, IDictionary<string, object> custom)
        {
            return this.Info(msg, custom);
        }

        ILogger ILogger.Debug(string msg, IDictionary<string, object> custom)
        {
            return this.Debug(msg, custom);
        }

        ILogger ILogger.Critical(System.Exception error, IDictionary<string, object> custom)
        {
            return this.Critical(error, custom);
        }

        ILogger ILogger.Error(System.Exception error, IDictionary<string, object> custom)
        {
            return this.Error(error, custom);
        }

        ILogger ILogger.Warning(System.Exception error, IDictionary<string, object> custom)
        {
            return this.Warning(error, custom);
        }

        ILogger ILogger.Info(System.Exception error, IDictionary<string, object> custom)
        {
            return this.Info(error, custom);
        }

        ILogger ILogger.Debug(System.Exception error, IDictionary<string, object> custom)
        {
            return this.Debug(error, custom);
        }

        ILogger ILogger.Critical(ITraceable traceableObj, IDictionary<string, object> custom)
        {
            return this.Critical(traceableObj, custom);
        }

        ILogger ILogger.Error(ITraceable traceableObj, IDictionary<string, object> custom)
        {
            return this.Error(traceableObj, custom);
        }

        ILogger ILogger.Warning(ITraceable traceableObj, IDictionary<string, object> custom)
        {
            return this.Warning(traceableObj, custom);
        }

        ILogger ILogger.Info(ITraceable traceableObj, IDictionary<string, object> custom)
        {
            return this.Info(traceableObj, custom);
        }

        ILogger ILogger.Debug(ITraceable traceableObj, IDictionary<string, object> custom)
        {
            return this.Debug(traceableObj, custom);
        }

        ILogger ILogger.Critical(object obj, IDictionary<string, object> custom)
        {
            return this.Critical(obj, custom);
        }

        ILogger ILogger.Error(object obj, IDictionary<string, object> custom)
        {
            return this.Error(obj, custom);
        }

        ILogger ILogger.Warning(object obj, IDictionary<string, object> custom)
        {
            return this.Warning(obj, custom);
        }

        ILogger ILogger.Info(object obj, IDictionary<string, object> custom)
        {
            return this.Info(obj, custom);
        }

        ILogger ILogger.Debug(object obj, IDictionary<string, object> custom)
        {
            return this.Debug(obj, custom);
        }

        #endregion ILogger explicitly 

        #region IDisposable explicitly

        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        #endregion IDisposable explicitly

        internal void Report(
            System.Exception e, 
            ErrorLevel? level = ErrorLevel.Error, 
            IDictionary<string, object> custom = null, 
            TimeSpan? timeout = null,
            SemaphoreSlim signal = null
            )
        {
            SnapExceptionDataAsCustomData(e, ref custom);
            SendAsync(new Body(e), level, custom, timeout, signal);
        }

        internal void Report(
            string message, 
            ErrorLevel? level = ErrorLevel.Error, 
            IDictionary<string, object> custom = null,
            TimeSpan? timeout = null,
            SemaphoreSlim signal = null
            )
        {
            SendAsync(new Body(new Message(message)), level, custom, timeout, signal);
        }

        internal void SendAsync(
            Data data,
            TimeSpan? timeout = null,
            SemaphoreSlim signal = null
            )
        {
            DateTime? timeoutAt = null;
            if (timeout.HasValue)
            {
                timeoutAt = DateTime.Now.Add(timeout.Value);
            }
            // we are taking here a fire-and-forget approach:
            Task.Factory.StartNew(() => Send(data, timeoutAt, signal));
        }

        private void SendAsync(
            Body body, 
            ErrorLevel? level, 
            IDictionary<string, object> custom,
            TimeSpan? timeout = null,
            SemaphoreSlim signal = null
            )
        {
            DateTime? timeoutAt = null;
            if (timeout.HasValue)
            {
                timeoutAt = DateTime.Now.Add(timeout.Value);
            }
            // we are taking here a fire-and-forget approach:
            Task.Factory.StartNew(() => Send(body, level, custom, timeoutAt, signal));
        }

        private void DoSend(Payload payload)
        {
            //lock (this._syncRoot)
            {
                if (string.IsNullOrWhiteSpace(this._config.AccessToken)
                    || this._config.Enabled == false
                    )
                {
                    return;
                }

                if (TelemetryCollector.Instance.Config.TelemetryEnabled)
                {
                    payload.Data.Body.Telemetry = 
                        TelemetryCollector.Instance.TelemetryQueue.GetQueueContent();
                }

                if (this._config.Server != null)
                {
                    payload.Data.Server = this._config.Server;
                }

                try
                {
                    if (this._config.CheckIgnore != null
                        && this._config.CheckIgnore.Invoke(payload)
                        )
                    {
                        return;
                    }
                }
                catch (System.Exception ex)
                {
                    OnRollbarEvent(new InternalErrorEventArgs(this, payload, ex, "While  check-ignoring a payload..."));
                }

                try
                {
                    this._config.Transform?.Invoke(payload);
                }
                catch (System.Exception ex)
                {
                    OnRollbarEvent(new InternalErrorEventArgs(this, payload, ex, "While  transforming a payload..."));
                }

                try
                {
                    this._config.Truncate?.Invoke(payload);
                }
                catch (System.Exception ex)
                {
                    OnRollbarEvent(new InternalErrorEventArgs(this, payload, ex, "While  truncating a payload..."));
                }

                this._payloadQueue.Enqueue(payload);

                return;
            }
        }

        private void Send(
            Data data,
            DateTime? timeoutAt = null,
            SemaphoreSlim signal = null
            )
        {
            lock (this._syncRoot)
            {
                var payload = new Payload(this._config.AccessToken, data, timeoutAt, signal);
                DoSend(payload);
            }
        }

        private void Send(
            Body body,
            ErrorLevel? level,
            IDictionary<string, object> custom,
            DateTime? timeoutAt = null,
            SemaphoreSlim signal = null
            )
        {
            lock (this._syncRoot)
            {
                var data = new Data(this._config, body, custom);
                if (level.HasValue)
                {
                    data.Level = level;
                }
                Send(data, timeoutAt, signal);
            }
        }

        internal virtual void OnRollbarEvent(RollbarEventArgs e)
        {
            EventHandler<RollbarEventArgs> handler = InternalEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal static void SnapExceptionDataAsCustomData(
            System.Exception e,
            ref IDictionary<string, object> custom
            )
        {
            if (custom == null)
            {
                custom = 
                    new Dictionary<string, object>(capacity: e.Data != null ? e.Data.Count : 0);
            }

            const string nullObjPresentation = "<null>";
            if (e.Data != null)
            {
                string customKeyPrefix = $"{e.GetType().Name}.Data.";
                foreach(var key in e.Data.Keys)
                {
                    // Some of the null-checks here may look unnecessary for the way an IDictionary
                    // is implemented today. 
                    // But the things could change tomorrow and we want to stay safe always:
                    object valueObj = e.Data[key];
                    if (valueObj == null && key == null)
                    {
                        continue;
                    }
                    string keyName = (key != null) ? key.ToString() : nullObjPresentation;
                    string customKey = $"{customKeyPrefix}{keyName}";
                    custom[customKey] = valueObj ?? nullObjPresentation;
                }
            }

            if (e.InnerException != null)
            {
                SnapExceptionDataAsCustomData(e.InnerException, ref custom);
            }

            // there could be more Data to capture in case of an AggregateException:
            AggregateException aggregateException = e as AggregateException;
            if (aggregateException != null && aggregateException.InnerExceptions != null)
            {
                foreach(var aggregatedException in aggregateException.InnerExceptions)
                {
                    SnapExceptionDataAsCustomData(aggregatedException, ref custom);
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
