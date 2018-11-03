[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using Rollbar.Telemetry;
    using System;
    using System.Collections.Concurrent;
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
            var rollbarClient = new RollbarClient(
                this._config
                , RollbarQueueController.Instance.ProvideHttpClient(this._config.ProxyAddress, this._config.ProxyUsername, this._config.ProxyPassword)
                );
            this._payloadQueue = new PayloadQueue(this, rollbarClient);
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
            if (this._config.LogLevel.HasValue && data.Level < this._config.LogLevel.Value)
            {
                // nice shortcut:
                return this;
            }

            this.Report(data, data.Level.HasValue ? data.Level.Value : ErrorLevel.Info);

            return this;
        }

        public ILogger Log(ErrorLevel level, object obj, IDictionary<string, object> custom = null)
        {
            if (this.Config.LogLevel.HasValue && level < this.Config.LogLevel.Value)
            {
                // nice shortcut:
                return this;
            }

            this.Report(obj, level, custom);

            return this;
        }

        public ILogger Log(ErrorLevel level, string msg, IDictionary<string, object> custom = null)
        {
            if (this._config.LogLevel.HasValue && level < this._config.LogLevel.Value)
            {
                // nice shortcut:
                return this;
            }

            this.Report(msg, level, custom);

            return this;
        }


        public ILogger Critical(string msg, IDictionary<string, object> custom = null)
        {
            this.Report(msg, ErrorLevel.Critical, custom);

            return this;
        }

        public ILogger Error(string msg, IDictionary<string, object> custom = null)
        {
            this.Report(msg, ErrorLevel.Error, custom);

            return this;
        }

        public ILogger Warning(string msg, IDictionary<string, object> custom = null)
        {
            this.Report(msg, ErrorLevel.Warning, custom);

            return this;
        }

        public ILogger Info(string msg, IDictionary<string, object> custom = null)
        {
            this.Report(msg, ErrorLevel.Info, custom);

            return this;
        }

        public ILogger Debug(string msg, IDictionary<string, object> custom = null)
        {
            this.Report(msg, ErrorLevel.Debug, custom);

            return this;
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
            this.Report(traceableObj, ErrorLevel.Critical, custom);

            return this;
        }

        public ILogger Error(ITraceable traceableObj, IDictionary<string, object> custom = null)
        {
            this.Report(traceableObj, ErrorLevel.Error, custom);

            return this;
        }

        public ILogger Warning(ITraceable traceableObj, IDictionary<string, object> custom = null)
        {
            this.Report(traceableObj, ErrorLevel.Warning, custom);

            return this;
        }

        public ILogger Info(ITraceable traceableObj, IDictionary<string, object> custom = null)
        {
            this.Report(traceableObj, ErrorLevel.Info, custom);

            return this;
        }

        public ILogger Debug(ITraceable traceableObj, IDictionary<string, object> custom = null)
        {
            this.Report(traceableObj, ErrorLevel.Debug, custom);

            return this;
        }

        public ILogger Critical(object obj, IDictionary<string, object> custom = null)
        {
            this.Report(obj, ErrorLevel.Critical, custom);

            return this;
        }

        public ILogger Error(object obj, IDictionary<string, object> custom = null)
        {
            this.Report(obj, ErrorLevel.Error, custom);

            return this;
        }

        public ILogger Warning(object obj, IDictionary<string, object> custom = null)
        {
            this.Report(obj, ErrorLevel.Warning, custom);

            return this;
        }

        public ILogger Info(object obj, IDictionary<string, object> custom = null)
        {
            this.Report(obj, ErrorLevel.Info, custom);

            return this;
        }

        public ILogger Debug(object obj, IDictionary<string, object> custom = null)
        {
            this.Report(obj, ErrorLevel.Debug, custom);

            return this;
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
            object data,
            ErrorLevel level,
            IDictionary<string, object> custom = null,
            TimeSpan? timeout = null,
            SemaphoreSlim signal = null
            )
        {
            EnqueueAsync(data, level, custom, timeout, signal);
        }


        //internal void Report(
        //    System.Exception e, 
        //    ErrorLevel? level = ErrorLevel.Error, 
        //    IDictionary<string, object> custom = null, 
        //    TimeSpan? timeout = null,
        //    SemaphoreSlim signal = null
        //    )
        //{
        //    SnapExceptionDataAsCustomData(e, ref custom);
        //    SendAsync(new Body(e), level, custom, timeout, signal);
        //}

        //internal void Report(
        //    string message, 
        //    ErrorLevel? level = ErrorLevel.Error, 
        //    IDictionary<string, object> custom = null,
        //    TimeSpan? timeout = null,
        //    SemaphoreSlim signal = null
        //    )
        //{
        //    SendAsync(new Body(new Message(message)), level, custom, timeout, signal);
        //}

        //internal void SendAsync(
        //    Data data,
        //    TimeSpan? timeout = null,
        //    SemaphoreSlim signal = null
        //    )
        //{
        //    DateTime? timeoutAt = null;
        //    if (timeout.HasValue)
        //    {
        //        timeoutAt = DateTime.Now.Add(timeout.Value);
        //    }
        //    // we are taking here a fire-and-forget approach:
        //    Task.Factory.StartNew(() => Send(data, timeoutAt, signal));
        //}

        //private void SendAsync(
        //    Body body, 
        //    ErrorLevel? level, 
        //    IDictionary<string, object> custom,
        //    TimeSpan? timeout = null,
        //    SemaphoreSlim signal = null
        //    )
        //{
        //    DateTime? timeoutAt = null;
        //    if (timeout.HasValue)
        //    {
        //        timeoutAt = DateTime.Now.Add(timeout.Value);
        //    }
        //    // we are taking here a fire-and-forget approach:
        //    Task task = Task.Factory.StartNew(() => Send(body, level, custom, timeoutAt, signal));
        //    bool success = false;
        //    while(!success)
        //    {
        //        success = this._pendingTasks.TryAdd(task, task);
        //    }
        //    task.ContinueWith(RemovePendingTask);
        //}


        internal void EnqueueAsync(
            object dataObject,
            ErrorLevel level,
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
            Task task = new Task(state => Enqueue(dataObject, level, custom, timeoutAt, signal), "EnqueueAsync");// Task.Factory.StartNew(state => Enqueue(dataObject, level, custom, timeoutAt, signal), "EnqueueAsync");
            bool success = false;
            do
            {
                success = this._pendingTasks.TryAdd(task, task);
            } while (!success);

            task.ContinueWith(RemovePendingTask)
                .ContinueWith(p => {
                    OnRollbarEvent(new InternalErrorEventArgs(this, null, p.Exception, "While performing EnqueueAsync(...)..."));
                    System.Diagnostics.Trace.TraceError(p.Exception.ToString());
                    }, 
                    TaskContinuationOptions.OnlyOnFaulted
                    );
            task.Start();
        }








        private readonly ConcurrentDictionary<Task, Task> _pendingTasks = new ConcurrentDictionary<Task, Task>();
        private void RemovePendingTask(Task task)
        {
            bool success = false;
            do
            {
                success = this._pendingTasks.TryRemove(task, out Task taskOut);
            } while (!success);
        }

        private void DoSend(Payload payload)
        {
            //lock (this._syncRoot)
            {
                // here is the last chance to decide if we need to actually send this payload
                // based on the current config settings:
                if (string.IsNullOrWhiteSpace(this._config.AccessToken)
                    || this._config.Enabled == false
                    || (this._config.LogLevel.HasValue && payload.Data.Level < this._config.LogLevel.Value)
                    )
                {
                    return;
                }

                if (TelemetryCollector.Instance.Config.TelemetryEnabled)
                {
                    payload.Data.Body.Telemetry = 
                        TelemetryCollector.Instance.GetQueueContent();
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

        //private void Send(
        //    Data data,
        //    DateTime? timeoutAt = null,
        //    SemaphoreSlim signal = null
        //    )
        //{
        //    lock (this._syncRoot)
        //    {
        //        var payload = new Payload(this._config.AccessToken, data, timeoutAt, signal);
        //        DoSend(payload);
        //    }
        //}

        //private void Send(
        //    Body body,
        //    ErrorLevel? level,
        //    IDictionary<string, object> custom,
        //    DateTime? timeoutAt = null,
        //    SemaphoreSlim signal = null
        //    )
        //{
        //    lock (this._syncRoot)
        //    {
        //        var data = new Data(this._config, body, custom);
        //        if (level.HasValue)
        //        {
        //            data.Level = level;
        //        }
        //        Send(data, timeoutAt, signal);
        //    }
        //}


        private void Enqueue(
            object dataObject,
            ErrorLevel level,
            IDictionary<string, object> custom,
            DateTime? timeoutAt = null,
            SemaphoreSlim signal = null
            )
        {
            lock (this._syncRoot)
            {
                var data = RollbarUtil.PackageAsPayloadData(this.Config, level, dataObject, custom); //new Data(this._config, body, custom);
                var payload = new Payload(this._config.AccessToken, data, timeoutAt, signal);
                DoSend(payload);
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

        //internal static void SnapExceptionDataAsCustomData(
        //    System.Exception e,
        //    ref IDictionary<string, object> custom
        //    )
        //{
        //    if (custom == null)
        //    {
        //        custom = 
        //            new Dictionary<string, object>(capacity: e.Data != null ? e.Data.Count : 0);
        //    }

        //    const string nullObjPresentation = "<null>";
        //    if (e.Data != null)
        //    {
        //        string customKeyPrefix = $"{e.GetType().Name}.Data.";
        //        foreach(var key in e.Data.Keys)
        //        {
        //            // Some of the null-checks here may look unnecessary for the way an IDictionary
        //            // is implemented today. 
        //            // But the things could change tomorrow and we want to stay safe always:
        //            object valueObj = e.Data[key];
        //            if (valueObj == null && key == null)
        //            {
        //                continue;
        //            }
        //            string keyName = (key != null) ? key.ToString() : nullObjPresentation;
        //            string customKey = $"{customKeyPrefix}{keyName}";
        //            custom[customKey] = valueObj ?? nullObjPresentation;
        //        }
        //    }

        //    if (e.InnerException != null)
        //    {
        //        SnapExceptionDataAsCustomData(e.InnerException, ref custom);
        //    }

        //    // there could be more Data to capture in case of an AggregateException:
        //    AggregateException aggregateException = e as AggregateException;
        //    if (aggregateException != null && aggregateException.InnerExceptions != null)
        //    {
        //        foreach(var aggregatedException in aggregateException.InnerExceptions)
        //        {
        //            SnapExceptionDataAsCustomData(aggregatedException, ref custom);
        //        }
        //    }
        //}

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Task.WaitAll(this._pendingTasks.Values.ToArray(), TimeSpan.FromMilliseconds(500));
                    this._payloadQueue.Release();
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
