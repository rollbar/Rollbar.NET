namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;

    using Rollbar.Diagnostics;
    using Rollbar.PayloadStore;

    /// <summary>
    /// Class RollbarInfrastructure.
    /// Implements the <see cref="System.IDisposable" />
    /// 
    /// This is a process-wide service operating in the background on its own dedicated thread(s).
    /// 
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class RollbarInfrastructure
        : IRollbarInfrastructure
        , IDisposable
    {
        private static readonly TraceSource traceSource = new TraceSource(typeof(RollbarInfrastructure).FullName);

        internal readonly TimeSpan _sleepInterval = TimeSpan.FromMilliseconds(25);

        private readonly object _syncLock = new object();

        private Thread _infrastructureThread;

        private CancellationTokenSource _cancellationTokenSource;

        private bool _isInitialized = false;

        private IRollbarInfrastructureConfig _config = null;

        private IPayloadStoreRepository _storeRepository = null;

        #region singleton implementation

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static RollbarInfrastructure Instance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarInfrastructure" /> class from being created.
        /// </summary>
        private RollbarInfrastructure()
        {
            traceSource.TraceInformation($"Creating the {typeof(RollbarInfrastructure).Name}...");
        }

        /// <summary>
        /// Class NestedSingleInstance. This class cannot be inherited.
        /// </summary>
        private sealed class NestedSingleInstance
        {
            /// <summary>
            /// Prevents a default instance of the <see cref="NestedSingleInstance"/> class from being created.
            /// </summary>
            private NestedSingleInstance()
            {
            }

            /// <summary>
            /// The instance
            /// </summary>
            internal static readonly RollbarInfrastructure Instance =
                new RollbarInfrastructure();
        }

        #endregion singleton implementation

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        public bool IsInitialized
        {
            get
            {
                return this._isInitialized;
            }
        }

        /// <summary>
        /// Gets the queue controller.
        /// </summary>
        /// <value>The queue controller.</value>
        public IRollbarQueueController? QueueController
        {
            get
            {
                return RollbarQueueController.Instance;
            }
        }

        /// <summary>
        /// Gets the telemetry collector.
        /// </summary>
        /// <value>The telemetry collector.</value>
        public IRollbarTelemetryCollector? TelemetryCollector
        {
            get
            {
                return RollbarTelemetryCollector.Instance;
            }
        }


        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IRollbarInfrastructureConfig Config
        {
            get
            {
                return this._config;
            }
        }

        /// <summary>
        /// Initializes the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <exception cref="RollbarException">
        /// Exception while initializing the internal services!
        /// </exception>
        public void Init(IRollbarInfrastructureConfig config)
        {
            Assumption.AssertNotNull(config, nameof(config));

            lock(this._syncLock)
            {
                if(this._isInitialized)
                {
                    string msg = $"{typeof(RollbarInfrastructure).Name} can not be initialized more than once!";
                    traceSource.TraceInformation(msg);
                    throw new RollbarException(
                        InternalRollbarError.InfrastructureError,
                        msg
                        );
                }

                this._config = config;
                this._config.Reconfigured += _config_Reconfigured;
                this._isInitialized = true;
                try
                {
                    RollbarQueueController.Instance!.Init(config);
                    RollbarTelemetryCollector.Instance!.Init(config.RollbarTelemetryOptions);
                    //TODO: RollbarConfig
                    // - init ConnectivityMonitor service as needed
                    //   NOTE: It should be sufficient to make ConnectivityMonitor as internal class
                    //         It is only used by RollbarClient and RollbarQueueController that are 
                    //         already properly deactivated in single-threaded environments.
                }
                catch(Exception ex)
                {
                    this._isInitialized = false;

                    throw new RollbarException(
                        InternalRollbarError.InfrastructureError,
                        "Exception while initializing the internal services!",
                        ex
                        );
                }

            }
        }

        private void _config_Reconfigured(object sender, EventArgs e)
        {
            //TODO: RollbarConfig - implement
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            traceSource.TraceInformation($"Starting the {typeof(RollbarInfrastructure).Name}...");
        }

        /// <summary>
        /// Stops the infrustructure operation.
        /// </summary>
        /// <param name="immediately">if set to <c>true</c> it, essentially, aborts the operation without gracefully shutting it down.</param>
        public void Stop(bool immediately)
        {
            traceSource.TraceInformation($"Stopping the {typeof(RollbarInfrastructure).Name}...");

            if(!immediately && this._cancellationTokenSource != null)
            {
                this._cancellationTokenSource.Cancel();
                return;
            }

            this._cancellationTokenSource?.Cancel();
            if(this._infrastructureThread != null)
            {

                if(!this._infrastructureThread.Join(TimeSpan.FromSeconds(60)))
                {
                    this._infrastructureThread.Abort();
                }

                CompleteProcessing();
            }
        }
        private void CompleteProcessing()
        {
            Debug.WriteLine("Entering " + this.GetType().FullName + "." + nameof(this.CompleteProcessing) + "() method...");
            traceSource.TraceInformation($"{typeof(RollbarInfrastructure).Name} is completing processing...");
        }


        #region IDisposable Support

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    CompleteProcessing();
                    this._config.Reconfigured -= _config_Reconfigured;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarQueueController() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>This code added to correctly implement the disposable pattern.</remarks>
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
