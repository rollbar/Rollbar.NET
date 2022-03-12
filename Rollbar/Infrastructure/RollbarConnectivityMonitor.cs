namespace Rollbar.Infrastructure
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
    internal sealed class RollbarConnectivityMonitor
        : IRollbarConnectivityMonitor
        , IDisposable
    {
        private readonly object _connectivityStatusSyncLock = new();
        private readonly TimeSpan _minMonitoringInterval;
        private readonly TimeSpan _maxMonitoringInterval;
        private TimeSpan _currentMonitoringInterval;
        private Timer? _monitoringTimer;

        #region singleton implementation

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static RollbarConnectivityMonitor? Instance
        {
            get
            {
                return RollbarInfrastructure.Instance.ConnectivityMonitor as RollbarConnectivityMonitor;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarConnectivityMonitor"/> class from being created.
        /// </summary>
        internal RollbarConnectivityMonitor()
        {
            TimeSpan initialDelay = TimeSpan.Zero;
            this._minMonitoringInterval = TimeSpan.FromSeconds(10);
            this._maxMonitoringInterval = TimeSpan.FromMinutes(5);

            this.IsConnectivityOn = false;
            this._currentMonitoringInterval = this._minMonitoringInterval;

            this._monitoringTimer = new Timer(
                CheckConnectivityStatus,
                null,
                initialDelay,
                this._currentMonitoringInterval
            );

            this.CheckConnectivityStatus(null);
        }

        #endregion singleton implementation

        /// <summary>
        /// Gets a value indicating whether this instance is connectivity on.
        /// </summary>
        /// <value><c>true</c> if this instance is connectivity on; otherwise, <c>false</c>.</value>
        public bool IsConnectivityOn
        {
            get; // NOTE: Simplest way to fake no-connectivity is to always return false from this getter
            private set;
        }

        /// <summary>
        /// Overrides as offline.
        /// </summary>
        public void OverrideAsOffline()
        {
            if(this.IsConnectivityOn)
            {
                lock(this._connectivityStatusSyncLock)
                {
                    // we do not want to override anything as offline 
                    // if the monitoring timer does not exist:
                    if(this._monitoringTimer != null)
                    {
                        this.IsConnectivityOn = false;
                    }
                }
            }
        }

        /// <summary>
        /// Disables this instance.
        /// </summary>
        /// <remarks>Any concrete Connectivity Monitor implementation may not be 100% accurate for all the possible
        /// network environments. So, you may have to disable it in case it does not properly detect
        /// specific network conditions. If disabled it will be assumed to always have its
        /// IsConnectivityOn property returning true.</remarks>
        public void Disable()
        {
            lock(this._connectivityStatusSyncLock)
            {
                if(this._monitoringTimer != null)
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
            get
            {
                return (this._monitoringTimer == null);
            }
        }

        /// <summary>
        /// Checks the connectivity status.
        /// </summary>
        /// <param name="state">The state.</param>
        private void CheckConnectivityStatus(object? state)
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
            lock(this._connectivityStatusSyncLock)
            {
                if(this._monitoringTimer == null)
                {
                    this.IsConnectivityOn = true;
                    return;
                }

                bool isConnectedNow = TestApiServer();

                if(isConnectedNow)
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
            if(this._monitoringTimer != null
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
            if(this._monitoringTimer != null
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
            TcpClient? client = null;
            try
            {
                client = new TcpClient(@"api.rollbar.com", 443);
                result = true;
            }
            catch(SocketException ex)
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

        private bool _disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1066:Collapsible \"if\" statements should be merged", Justification = "Promotes better code structure.")]
        private void Dispose(bool disposing)
        {
            if(!_disposedValue)
            {
                if(disposing)
                {
                    // dispose managed state (managed objects).
                    if(this._monitoringTimer != null)
                    {
                        this._monitoringTimer.Dispose();
                        this._monitoringTimer = null;
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
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
