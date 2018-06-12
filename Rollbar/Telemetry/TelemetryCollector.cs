[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar.Telemetry
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Rollbar.DTOs;

    public class TelemetryCollector
    {
        #region singleton implementation

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static TelemetryCollector Instance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="TelemetryCollector"/> class from being created.
        /// </summary>
        private TelemetryCollector()
        {
            this._config = new TelemetryConfig();
            this._config.Reconfigured += _config_Reconfigured;

            this.StartAutocollection();
        }

        private void _config_Reconfigured(object sender, EventArgs e)
        {
            this.TelemetryQueue.QueueDepth = this._config.TelemetryQueueDepth;
        }

        private sealed class NestedSingleInstance
        {
            static NestedSingleInstance()
            {
            }

            internal static readonly TelemetryCollector Instance =
                new TelemetryCollector();
        }

        #endregion singleton implementation

        /// <summary>
        /// Captures the specified telemetry.
        /// </summary>
        /// <param name="telemetry">The telemetry.</param>
        /// <returns></returns>
        public TelemetryCollector Capture(Telemetry telemetry)
        {
            if (this.Config.TelemetryEnabled)
            {
                this.TelemetryQueue.Enqueue(telemetry);
            }
            return this;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public TelemetryConfig Config { get { return this._config; } }

        /// <summary>
        /// Gets the telemetry queue.
        /// </summary>
        /// <value>
        /// The telemetry queue.
        /// </value>
        public TelemetryQueue TelemetryQueue { get; } = new TelemetryQueue();

        /// <summary>
        /// Starts the auto-collection.
        /// </summary>
        public void StartAutocollection()
        {
            if (!this.Config.TelemetryEnabled)
            {
                return; // no need to start at all...
            }
            if (this.Config.TelemetryAutoCollectionTypes == TelemetryType.None)
            {
                return; // nothing really to collect...
            }

            // let's resync with relevant config settings: 
            this.TelemetryQueue.QueueDepth = this.Config.TelemetryQueueDepth;

            lock (_syncRoot)
            {
                if (this._telemetryThread == null)
                {
                    this._telemetryThread = new Thread(new ParameterizedThreadStart(this.KeepCollectingTelemetry))
                    {
                        IsBackground = true,
                        Name = "Rollbar Telemetry Thread"
                    };

                    this._cancellationTokenSource = new CancellationTokenSource();
                    this._telemetryThread.Start(_cancellationTokenSource.Token);
                }
            }
        }

        /// <summary>
        /// Stops the auto-collection.
        /// </summary>
        /// <param name="immediate">if set to <c>true</c> [immediate].</param>
        public void StopAutocollection(bool immediate)
        {
            lock(_syncRoot)
            {
                if (this._cancellationTokenSource == null)
                {
                    return;
                }

                this._cancellationTokenSource.Cancel();
                if (immediate && this._telemetryThread != null)
                {
                    this._telemetryThread.Join(TimeSpan.FromSeconds(60));
                    CompleteProcessing();
                }
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

        private readonly TelemetryConfig _config;
        private Thread _telemetryThread = null;
        private CancellationTokenSource _cancellationTokenSource = null;
        private readonly object _syncRoot = new object();

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

        private void KeepCollectingTelemetry(object data)
        {
            //for now, until AutoCollectTelemetry() is implemented:
            return;

            CancellationToken cancellationToken = (CancellationToken)data;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    this.AutoCollectTelemetry();
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (System.Threading.ThreadAbortException tae)
                {
                    return;
                }
                catch (System.Exception ex)
                {
                    string msg = ex.Message;
                    //TODO: do we want to direct the exception 
                    //      to some kind of Rollbar notifier maintenance "access token"?
                }
#pragma warning restore CS0168 // Variable is declared but never used

                if (cancellationToken.IsCancellationRequested)
                    break;

                Thread.Sleep(this.Config.TelemetryAutoCollectionInterval);
            }

            CompleteProcessing();
        }

    }
}
