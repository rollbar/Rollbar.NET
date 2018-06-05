namespace Rollbar.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;

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
                return TelemetryCollector.Instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="TelemetryCollector"/> class from being created.
        /// </summary>
        private TelemetryCollector()
        {
            this.Start();
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

        public TelemetryConfig Config { get; private set; } = new TelemetryConfig();

        public TelemetryQueue TelemetryQueue { get; } = new TelemetryQueue();

        private void CollectTelemetry()
        {
        }

        private void KeepCollectingTelemetry(object data)
        {
            CancellationToken cancellationToken = (CancellationToken)data;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    this.CollectTelemetry();
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (System.Threading.ThreadAbortException tae)
                {
                    return;
                }
                catch (System.Exception ex)
                {
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

        private void Start()
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

        private void CompleteProcessing()
        {
            Debug.WriteLine("Entering " + this.GetType().FullName + "." + nameof(this.CompleteProcessing) + "() method...");
            this._cancellationTokenSource.Dispose();
            this._cancellationTokenSource = null;
            this._telemetryThread = null;
        }

        public void Stop(bool immediate)
        {
            if (!immediate && this._cancellationTokenSource != null)
            {
                this._cancellationTokenSource.Cancel();
                return;
            }

            this._cancellationTokenSource.Cancel();
            if (this._telemetryThread != null)
            {
                this._telemetryThread.Join(TimeSpan.FromSeconds(60));
                CompleteProcessing();
            }
        }

    }
}
