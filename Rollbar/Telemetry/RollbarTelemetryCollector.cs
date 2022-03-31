namespace Rollbar
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using Rollbar.DTOs;

    /// <summary>
    /// Implements Rollbar telemetry collector service.
    /// </summary>
    internal sealed class RollbarTelemetryCollector
        : IRollbarTelemetryCollector
        , IDisposable
    {
        private static readonly TraceSource traceSource = 
            new TraceSource(typeof(RollbarTelemetryCollector).FullName ?? "RollbarTelemetryCollector");

        #region singleton implementation

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static RollbarTelemetryCollector? Instance
        {
            get
            {
                return RollbarInfrastructure.Instance.TelemetryCollector as RollbarTelemetryCollector;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarTelemetryCollector"/> class from being created.
        /// </summary>
        internal RollbarTelemetryCollector()
        {
            traceSource.TraceInformation($"Creating the {typeof(RollbarTelemetryCollector).Name}...");

            this._telemetryQueue.QueueDepth = 0; // since we do not have valid telemetry config assigned yet...
        }

        #endregion singleton implementation

        internal void Init(IRollbarTelemetryOptions options)
        {
            if(this._config != null)
            {
                this.StopAutocollection(false);
                this.FlushQueue();
                this._config.Reconfigured -= _config_Reconfigured;
            }

            this._config = options;

            if (this._config != null)
            {
                // let's resync with relevant config settings: 
                this._telemetryQueue.QueueDepth = this._config.TelemetryQueueDepth;

                this._config.Reconfigured += _config_Reconfigured;
                this.StartAutocollection();
            }
            else
            {
                this._telemetryQueue.QueueDepth = 0;
            }
        }

        /// <summary>
        /// Captures the specified telemetry.
        /// </summary>
        /// <param name="telemetry">The telemetry.</param>
        /// <returns></returns>
        public RollbarTelemetryCollector Capture(Telemetry telemetry)
        {
            if (this.Config != null && this.Config.TelemetryEnabled)
            {
                this._telemetryQueue.Enqueue(telemetry);
            }
            return this;
        }

        /// <summary>
        /// Captures the specified telemetry.
        /// </summary>
        /// <param name="telemetry">The telemetry.</param>
        /// <returns></returns>
        IRollbarTelemetryCollector IRollbarTelemetryCollector.Capture(Telemetry telemetry)
        {
            return this.Capture(telemetry);
        }

        /// <summary>
        /// Gets the items count.
        /// </summary>
        /// <returns></returns>
        public int GetItemsCount()
        {
            return this._telemetryQueue.GetItemsCount();
        }

        /// <summary>
        /// Gets the content of the queue.
        /// </summary>
        /// <returns></returns>
        public Telemetry[] GetQueueContent()
        {
            return this._telemetryQueue.GetQueueContent();
        }

        /// <summary>
        /// Flushes the queue.
        /// </summary>
        /// <returns></returns>
        public IRollbarTelemetryCollector FlushQueue()
        {
            this._telemetryQueue.Flush();
            return this;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IRollbarTelemetryOptions? Config { get { return this._config; } }

        /// <summary>
        /// Starts the auto-collection.
        /// </summary>
        public IRollbarTelemetryCollector StartAutocollection()
        {
            if(this.Config == null)
            {
                return this;
            }

            if (!this.Config.TelemetryEnabled)
            {
                return this; // no need to start at all...
            }
            if (this.Config.TelemetryAutoCollectionTypes == TelemetryType.None)
            {
                return this; // nothing really to collect...
            }
            if (this.Config.TelemetryAutoCollectionInterval == TimeSpan.Zero)
            {
                return this; // this, essentially, means the auto-collection is off...
            }

            lock (_syncRoot)
            {
                if (this._telemetryThread == null)
                {
                    this._telemetryThread = new Thread(new ParameterizedThreadStart(this.KeepCollectingTelemetry))
                    {
                        IsBackground = true,
                        Name = "RollbarAutoTelemetry Thread"
                    };

                    this._cancellationTokenSource = new CancellationTokenSource();
                    this._telemetryThread.Start(_cancellationTokenSource.Token);
                }
            }

            return this;
        }

        /// <summary>
        /// Stops the auto-collection.
        /// </summary>
        /// <param name="immediate">if set to <c>true</c> [immediate].</param>
        public IRollbarTelemetryCollector StopAutocollection(bool immediate)
        {
            lock(_syncRoot)
            {
                if (this._cancellationTokenSource == null)
                {
                    return this;
                }

                this._cancellationTokenSource.Cancel();
                if (immediate && this._telemetryThread != null)
                {
                    this._telemetryThread.Join(TimeSpan.FromSeconds(60));
                    CompleteProcessing();
                }
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is auto-collecting.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is auto-collecting; otherwise, <c>false</c>.
        /// </value>
        public bool IsAutocollecting
        {
            get
            {
                lock(this._syncRoot)
                {
                    return !(this._cancellationTokenSource == null || this._cancellationTokenSource.IsCancellationRequested);
                }
            }
        }

        /// <summary>
        /// Gets the queue depth.
        /// </summary>
        /// <value>
        /// The queue depth.
        /// </value>
        public int QueueDepth { get { return this._telemetryQueue.QueueDepth; } }

        private Thread? _telemetryThread;
        private CancellationTokenSource? _cancellationTokenSource;
        private IRollbarTelemetryOptions? _config;
        private readonly TelemetryQueue _telemetryQueue = new TelemetryQueue();
        private readonly object _syncRoot = new object();

        private void _config_Reconfigured(object? sender, EventArgs e)
        {
            if(this._config == null)
            {
                this.StopAutocollection(false);
                return;
            }

            this._telemetryQueue.QueueDepth = this._config.TelemetryQueueDepth;

            if (this.IsAutocollecting 
                && (!this._config.TelemetryEnabled 
                    || this._config.TelemetryAutoCollectionInterval == TimeSpan.Zero 
                    || this._config.TelemetryAutoCollectionTypes == TelemetryType.None
                    )
               )
            {
                this.StopAutocollection(false);
            }
        }

        private void CompleteProcessing()
        {
            if (this._cancellationTokenSource == null)
            {
                return;
            }

            Debug.WriteLine("Entering " + this.GetType().FullName + "." + nameof(this.CompleteProcessing) + "() method...");
            this._cancellationTokenSource.Dispose();
            this._cancellationTokenSource = null;
            this._telemetryThread = null;
        }

        private void AutoCollectTelemetry()
        {
            //TBD...
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "To be implemented...")]
        private void KeepCollectingTelemetry(object? data)
        {
            //for now, until AutoCollectTelemetry() is implemented:
            return;

#pragma warning disable CS0162 // Unreachable code detected
            CancellationToken cancellationToken = (CancellationToken)data;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    this.AutoCollectTelemetry();
                }
                catch (System.Threading.ThreadAbortException tae)
                {
                    traceSource.TraceEvent(TraceEventType.Warning, 0, $"{nameof(KeepCollectingTelemetry)}(...):{Environment.NewLine}{tae}");
                    return;
                }
                catch (System.Exception ex)
                {
                    string msg = ex.Message;
                    traceSource.TraceEvent(TraceEventType.Error, 0, $"{nameof(KeepCollectingTelemetry)}(...):{Environment.NewLine}{msg}");
                    traceSource.TraceEvent(TraceEventType.Error, 0, $"{nameof(KeepCollectingTelemetry)}(...):{Environment.NewLine}{ex}");
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(this.Config.TelemetryAutoCollectionInterval);
            }

            CompleteProcessing();
#pragma warning restore CS0162 // Unreachable code detected
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    CompleteProcessing();
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, 
        /// or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// This code added to correctly implement the disposable pattern.
        /// </remarks>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
