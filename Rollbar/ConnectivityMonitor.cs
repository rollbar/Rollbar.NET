namespace Rollbar
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Class ConnectivityMonitor.
    /// </summary>
    internal class ConnectivityMonitor
    {
        private readonly object _connectivityStatusSyncLock = new object();
        private bool _isConnectivityOn;
        private TimeSpan _currentMonitoringInterval;
        private readonly Timer _monitoringTimer;
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

            this.CheckConnectivityStatus(null);

            this._monitoringTimer = new Timer(
                CheckConnectivityStatus, 
                null, 
                this._initialDelay, 
                this._currentMonitoringInterval
            );
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

        public bool IsConnectivityOn
        {
            get { return this._isConnectivityOn; }
        }

        public void OverrideAsOffline()
        {
            if (this._isConnectivityOn) 
            {
                lock (this._connectivityStatusSyncLock)
                {
                    this._isConnectivityOn = false;
                    //this.ResetMonitoringInterval();
                }
            }
        }

        private bool IsConnectivityAvailable()
        {
            // only recognizes changes related to Internet adapters
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }

            // however, this will include all adapters -- filter by operational status and activity
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            return (from networkInterface in interfaces
                where networkInterface.OperationalStatus == OperationalStatus.Up
                where (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel) && (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                where (!(networkInterface.Name.ToLower().Contains("virtual") || networkInterface.Description.ToLower().Contains("virtual")))
                select networkInterface.GetIPv4Statistics()).Any(statistics => (statistics.BytesReceived > 0) && (statistics.BytesSent > 0));
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

/*
        /// <summary>
        /// The timeout milliseconds
        /// </summary>
        private const int timeoutMilliseconds = 500;

        /// <summary>
        /// The google DNS ping target
        /// </summary>
        private static readonly IPAddress googleDnsPingTarget = IPAddress.Parse("8.8.8.8");

        /// <summary>
        /// Tests the internet ping.
        /// </summary>
        /// <returns><c>true</c> if the test succeeded, <c>false</c> otherwise.</returns>
        public static bool TestInternetPing()
        {
            Ping ping = new Ping();

            try
            {
                PingReply response = ping.Send(googleDnsPingTarget, timeoutMilliseconds);
                bool result = ((response != null) && (response.Status == IPStatus.Success));
                return result;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Determines whether [is available network active].
        /// </summary>
        /// <returns><c>true</c> if [is available network active]; otherwise, <c>false</c>.</returns>
        public static bool IsAvailableNetworkActive()
        {
            //return true;

            // only recognizes changes related to Internet adapters
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }

            // however, this will include all adapters -- filter by opstatus and activity
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            return (from networkInterface in interfaces
                where networkInterface.OperationalStatus == OperationalStatus.Up
                where (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel) && (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                where (!(networkInterface.Name.ToLower().Contains("virtual") || networkInterface.Description.ToLower().Contains("virtual")))
                select networkInterface.GetIPv4Statistics()).Any(statistics => (statistics.BytesReceived > 0) && (statistics.BytesSent > 0));
        }

        public static bool IsAnyNetwork() 
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }


*/

        /// <summary>
        /// The timeout milliseconds
        /// </summary>
        private const int timeoutMilliseconds = 500;

        /// <summary>
        /// The google DNS ping target
        /// </summary>
        private static readonly IPAddress googleDnsPingTarget = IPAddress.Parse("8.8.8.8");

        /// <summary>
        /// Tests the internet ping.
        /// </summary>
        /// <returns><c>true</c> if the test succeeded, <c>false</c> otherwise.</returns>
        public bool TestApiServer()
        {
            try
            {
                using (var client = new TcpClient("www.rollbar.com", 80))
                    return true;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }

    }
}
