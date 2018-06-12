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
            this.StartAutocollection();
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

        public TelemetryCollector Capture(Telemetry telemetry)
        {
            if (this.Config.TelemetryEnabled)
            {
                this.TelemetryQueue.Enqueue(telemetry);
            }
            return this;
        }

        public TelemetryConfig Config { get; } = new TelemetryConfig();

        public TelemetryQueue TelemetryQueue { get; } = new TelemetryQueue();

        private void CaptureEvent(Telemetry telemetry)
        {
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

                Thread.Sleep(this.Config.TelemetryCollectionInterval);
            }

            CompleteProcessing();
        }

        private Thread _telemetryThread = null;
        private CancellationTokenSource _cancellationTokenSource = null;
        private readonly object _syncRoot = new object();

        public void StartAutocollection()
        {
            if (!this.Config.TelemetryEnabled)
            {
                return; // no need to start at all...
            }
            if (this.Config.TelemetrySettings == TelemetrySettings.None)
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

    }
}
