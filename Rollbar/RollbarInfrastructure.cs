namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;

    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.Infrastructure;
    using Rollbar.PayloadStore;

    /// <summary>
    /// Class RollbarInfrastructure.
    /// Implements the <see cref="System.IDisposable" />
    /// 
    /// This is a process-wide service operating in the background on its own dedicated thread(s).
    /// 
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class RollbarInfrastructure
        : IRollbarInfrastructure
        , IDisposable
    {
        private static readonly TraceSource traceSource = 
            new TraceSource(typeof(RollbarInfrastructure).FullName ?? "RollbarInfrastructure");

        internal readonly TimeSpan _sleepInterval = 
            TimeSpan.FromMilliseconds(25);

        private readonly object _syncLock = new object();

        private bool _initializedOnce = false;

        private IRollbarInfrastructureConfig? _config;

        #region singleton implementation

        private static readonly Lazy<RollbarInfrastructure> lazySingleton =
            new Lazy<RollbarInfrastructure>(() => new RollbarInfrastructure());

        //private static readonly object classLock = new object();
        //private static volatile RollbarInfrastructure? singleton;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static RollbarInfrastructure Instance
        {
            get
            {
                //if (singleton == null)
                //{
                //    lock (classLock)
                //    {
                //        if (singleton == null)
                //        {
                //            singleton = new RollbarInfrastructure();
                //        }
                //    }
                //}
                //return singleton;

                return lazySingleton.Value;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarInfrastructure" /> class from being created.
        /// </summary>
        private RollbarInfrastructure()
        {
            traceSource.TraceInformation($"Creating the {typeof(RollbarInfrastructure).Name}...");
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
                return this._initializedOnce;
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
        /// Gets the connectivity monitor.
        /// </summary>
        /// <value>The connectivity monitor.</value>
        public IRollbarConnectivityMonitor? ConnectivityMonitor
        {
            get
            {
                return RollbarConnectivityMonitor.Instance;
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
                this.ValidateConfiguration();

                return this._config!;
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
            Assumption.AssertNotNull(RollbarInfrastructure.Instance, nameof(RollbarInfrastructure.Instance));
            Assumption.AssertFalse(RollbarInfrastructure.Instance.IsInitialized, nameof(RollbarInfrastructure.Instance.IsInitialized));


            lock(this._syncLock)
            {
                if(this._initializedOnce)
                {
                    string msg = $"{typeof(RollbarInfrastructure).Name} can not be initialized more than once!";
                    traceSource.TraceInformation(msg);
                    throw new RollbarException(
                        InternalRollbarError.InfrastructureError,
                        msg
                        );
                }

                // now, since the basic infrastructure seems to be good and initialized,
                // let's initialize all the dependent services of the infrastructure:
                try
                {
                    this._config = config;
                    this.ValidateConfiguration();
                    this._config.Reconfigured += _config_Reconfigured;
                    this._initializedOnce = true;
                    Assumption.AssertTrue(RollbarInfrastructure.Instance.IsInitialized, nameof(RollbarInfrastructure.Instance.IsInitialized));

                    RollbarQueueController.Instance?.Init(config);
                    RollbarTelemetryCollector.Instance?.Init(config.RollbarTelemetryOptions);
                    // NOTE: RollbarConfig
                    // - init ConnectivityMonitor service as needed
                    //   NOTE: It should be sufficient to make ConnectivityMonitor as internal class
                    //         It is only used by RollbarClient and RollbarQueueController that are 
                    //         already properly deactivated in single-threaded environments.
                }
                catch(Exception ex)
                {
                    this._initializedOnce = false;

                    throw new RollbarException(
                        InternalRollbarError.InfrastructureError,
                        "Exception while initializing the internal services!",
                        ex
                        );
                }

            }
        }

        private void ValidateConfiguration()
        {
            if(this._initializedOnce)
            {
                return;
            }

            if(this._config == null)
            {
                RollbarException exception = new RollbarException(InternalRollbarError.ConfigurationError, "Rollbar configuration is never assigned!");
                throw exception;
            }

            IValidatable? validatable = this._config as IValidatable;
            Validator.Validate(validatable, InternalRollbarError.ConfigurationError, "Failed to configure using invalid configuration prototype!");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Will be implemented later...")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Info Code Smell", "S1135:Track uses of \"TODO\" tags", Justification = "Will be implemented later...")]
        private void _config_Reconfigured(object? sender, EventArgs e)
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
        }
        private void CompleteProcessing()
        {
            Debug.WriteLine("Entering " + this.GetType().FullName + "." + nameof(this.CompleteProcessing) + "() method...");
            traceSource.TraceInformation($"{typeof(RollbarInfrastructure).Name} is completing processing...");
        }

        #region IDisposable Support

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
                    // dispose managed state (managed objects).
                    CompleteProcessing();
                    if(this._config != null)
                    {
                        this._config.Reconfigured -= _config_Reconfigured;
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>This code added to correctly implement the disposable pattern.</remarks>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

    }
}
