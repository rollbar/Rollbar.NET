namespace Rollbar
{
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Class ConnectivityMonitor.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class ConnectivityMonitor
        : IDisposable
    {
        private readonly object _connectivityStatusSyncLock = new object();
        private TimeSpan _currentMonitoringInterval;
        private Timer _monitoringTimer;
        private readonly TimeSpan _initialDelay;
        private readonly TimeSpan _minMonitoringInterval;
        private readonly TimeSpan _maxMonitoringInterval;

        #region singleton implementation

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ConnectivityMonitor Instance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ConnectivityMonitor"/> class from being created.
        /// </summary>
        private ConnectivityMonitor()
        {
            this._initialDelay = TimeSpan.Zero;
            this._minMonitoringInterval = TimeSpan.FromSeconds(10);
            this._maxMonitoringInterval = TimeSpan.FromMinutes(5);

            this.IsConnectivityOn = false;
            this._currentMonitoringInterval = this._minMonitoringInterval;

            this._monitoringTimer = new Timer(
                CheckConnectivityStatus, 
                null, 
                this._initialDelay, 
                this._currentMonitoringInterval
            );

            this.CheckConnectivityStatus(null);
        }

        /// <summary>
        /// Class NestedSingleInstance. This class cannot be inherited.
        /// </summary>
        private sealed class NestedSingleInstance
        {
            /// <summary>
            /// Prevents a default instance of the <see cref="NestedSingleInstance" /> class from being created.
            /// </summary>
            private NestedSingleInstance()
            {
            }

            /// <summary>
            /// The instance
            /// </summary>
            internal static readonly ConnectivityMonitor Instance =
                new ConnectivityMonitor();
        }

        #endregion singleton implementation

        /// <summary>
        /// Gets a value indicating whether this instance is connectivity on.
        /// </summary>
        /// <value><c>true</c> if this instance is connectivity on; otherwise, <c>false</c>.</value>
        public bool IsConnectivityOn 
        { 
            get;
            private set;
        }

        /// <summary>
        /// Overrides as offline.
        /// </summary>
        public void OverrideAsOffline()
        {
            if (this.IsConnectivityOn) 
            {
                lock (this._connectivityStatusSyncLock)
                {
                    // we do not want to override anything as offline 
                    // if the monitoring timer does not exist:
                    if (this._monitoringTimer != null)
                    {
                        this.IsConnectivityOn = false;
                    }
                }
            }
        }

        /// <summary>
        /// Disables this instance.
        /// </summary>
        public void Disable()
        {
            lock (this._connectivityStatusSyncLock)
            {
                if (this._monitoringTimer != null)
                {
                    this._monitoringTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    this._monitoringTimer.Dispose();
                    this._monitoringTimer = null;
                    this.IsConnectivityOn = true;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is disabled.
        /// </summary>
        /// <value><c>true</c> if this instance is disabled; otherwise, <c>false</c>.</value>
        public bool IsDisabled
        {
            get { return (this._monitoringTimer == null);}
        }

        /// <summary>
        /// Checks the connectivity status.
        /// </summary>
        /// <param name="state">The state.</param>
        private void CheckConnectivityStatus(object state)
        {
            try
            {
                DoCheckConnectivityStatus();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch(Exception ex)
            {
                RollbarErrorUtility.Report(
                    null, 
                    null, 
                    InternalRollbarError.ConnectivityMonitorError, 
                    null, 
                    ex, 
                    null
                    );
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        /// <summary>
        /// Does the check connectivity status.
        /// </summary>
        private void DoCheckConnectivityStatus()
        {
            lock (this._connectivityStatusSyncLock)
            {
                if (this._monitoringTimer == null)
                {
                    this.IsConnectivityOn = true;
                    return;
                }

                bool isConnectedNow = TestApiServer();

                if (isConnectedNow)
                {
                    this.ResetMonitoringInterval();
                }
                else
                {
                    this.AdoptOfflineMonitoringInterval();
                }

                this.IsConnectivityOn = isConnectedNow;
            }
        }

        /// <summary>
        /// Adopts the offline monitoring interval.
        /// </summary>
        private void AdoptOfflineMonitoringInterval()
        {
            // Keep incrementing the monitoring interval until max monitoring interval is reached.
            // Then, keep the max monitoring interval:
            if (this._monitoringTimer != null
                && !this.IsConnectivityOn 
                && this._currentMonitoringInterval < this._maxMonitoringInterval)
            {
                this._currentMonitoringInterval = 
                    TimeSpan.FromTicks(Math.Min(this._maxMonitoringInterval.Ticks, 2 * this._currentMonitoringInterval.Ticks));

                this._monitoringTimer.Change(this._currentMonitoringInterval, this._currentMonitoringInterval);
            }
        }

        /// <summary>
        /// Resets the monitoring interval.
        /// </summary>
        private void ResetMonitoringInterval()
        {
            // Making sure we are using shortest monitoring interval:
            if (this._monitoringTimer != null 
                && this._currentMonitoringInterval > this._minMonitoringInterval)
            {
                this._currentMonitoringInterval = this._minMonitoringInterval;
                this._monitoringTimer.Change(this._currentMonitoringInterval, this._currentMonitoringInterval);
            }
        }

        /// <summary>
        /// Tests the internet ping.
        /// NOTE: this function will always retun 'false' when executed
        /// behind a web proxy server requiring configuration custom settings!!!
        /// </summary>
        /// <returns><c>true</c> if the test succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>https://stackoverflow.com/questions/35066981/how-to-use-proxy-with-tcpclient-connectasync</remarks>
        public static bool TestApiServer()
        {
            bool result = false;
            TcpClient client = null;
            try
            {
                client = new TcpClient("www.rollbar.com",80);
                result = true;
            }
            catch (SocketException ex)
            {
                Debug.WriteLine($"EXCEPTION: {ex}");
                result = false;
            }
            finally
            {
                client?.Close();
                (client as IDisposable)?.Dispose();
            }

            return result;
        }

        #region IDisposable Support

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool _disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (this._monitoringTimer != null)
                    {
                        this._monitoringTimer.Dispose();
                        this._monitoringTimer = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ConnectivityMonitor() {
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
