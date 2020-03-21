namespace Rollbar
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Class ConnectivityMonitor.
    /// </summary>
    public class ConnectivityMonitor
    {
        private readonly object _connectivityStatusSyncLock = new object();
        private bool _isConnectivityOn;
        private TimeSpan _currentMonitoringInterval;
        private  Timer _monitoringTimer;
        private readonly TimeSpan _initialDelay;
        private readonly TimeSpan _minMonitoringInterval;
        private readonly TimeSpan _maxMonitoringInterval;

        #region singleton implementation

        public static ConnectivityMonitor Instance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        private ConnectivityMonitor()
        {
            this._initialDelay = TimeSpan.Zero;
            this._minMonitoringInterval = TimeSpan.FromSeconds(10);
            this._maxMonitoringInterval = TimeSpan.FromMinutes(5);

            this._isConnectivityOn = false;
            this._currentMonitoringInterval = this._minMonitoringInterval;

            this._monitoringTimer = new Timer(
                CheckConnectivityStatus, 
                null, 
                this._initialDelay, 
                this._currentMonitoringInterval
            );

            this.CheckConnectivityStatus(null);

        }

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
            get { return this._isConnectivityOn; }
        }

        /// <summary>
        /// Overrides as offline.
        /// </summary>
        public void OverrideAsOffline()
        {
            if (this._isConnectivityOn) 
            {
                lock (this._connectivityStatusSyncLock)
                {
                    // we do not want to override anything as offline 
                    // if the monitoring timer does not exist:
                    if (this._monitoringTimer != null)
                    {
                        this._isConnectivityOn = false;
                        //this.ResetMonitoringInterval();
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
                    this._monitoringTimer.Dispose();
                    this._monitoringTimer = null;
                    this._isConnectivityOn = true;
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

        private void CheckConnectivityStatus(object state)
        {
            //bool isConnectedNow = this.IsConnectivityAvailable();
            bool isConnectedNow = this.TestApiServer();

            lock (this._connectivityStatusSyncLock)
            {
                if (isConnectedNow)
                {
                    this.ResetMonitoringInterval();
                }
                else
                {
                    this.AdoptOfflineMonitoringInterval();
                }

                this._isConnectivityOn = isConnectedNow;
            }
        }

        private void AdoptOfflineMonitoringInterval()
        {
            // Keep incrementing the monitoring interval until max monitoring interval is reached.
            // Then, keep the max monitoring interval:
            if (!this._isConnectivityOn 
                && this._currentMonitoringInterval < this._maxMonitoringInterval)
            {
                this._currentMonitoringInterval = 
                    TimeSpan.FromTicks(Math.Min(this._maxMonitoringInterval.Ticks, 2 * this._currentMonitoringInterval.Ticks));

                this._monitoringTimer.Change(this._currentMonitoringInterval, this._currentMonitoringInterval);
            }
        }

        private void ResetMonitoringInterval()
        {
            // Making sure we are using shortest monitoring interval:
            if (this._currentMonitoringInterval > this._minMonitoringInterval)
            {
                this._currentMonitoringInterval = this._minMonitoringInterval;
                this._monitoringTimer.Change(this._currentMonitoringInterval, this._currentMonitoringInterval);
            }
        }

        /// <summary>
        /// Tests the internet ping.
        /// 
        /// NOTE: this function will always retun 'false' when executed 
        ///       behind a web proxy server requiring configuration custom settings!!!
        /// </summary>
        /// <returns><c>true</c> if the test succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/35066981/how-to-use-proxy-with-tcpclient-connectasync
        /// </remarks>
        public bool TestApiServer()
        {
            //return false;
            try
            {
                using (var client = new TcpClient("www.rollbar.com", 80))
                {
                    return true;
                }
            }
            catch (SocketException ex)
            {
                Debug.WriteLine($"EXCEPTION: {ex}");
                return false;
            }
        }

        //private bool IsConnectivityAvailable()
        //{
        //    // only recognizes changes related to Internet adapters
        //    if (!NetworkInterface.GetIsNetworkAvailable())
        //    {
        //        return false;
        //    }

        //    // however, this will include all adapters -- filter by operational status and activity
        //    NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
        //    return (from networkInterface in interfaces
        //        where networkInterface.OperationalStatus == OperationalStatus.Up
        //        where (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel) && (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        //        where (!(networkInterface.Name.ToLower().Contains("virtual") || networkInterface.Description.ToLower().Contains("virtual")))
        //        select networkInterface.GetIPv4Statistics()).Any(statistics => (statistics.BytesReceived > 0) && (statistics.BytesSent > 0));
        //}

    }
}
