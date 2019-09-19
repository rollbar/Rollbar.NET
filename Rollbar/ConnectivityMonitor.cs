namespace Rollbar
{
    using System;
    using System.Net;
    using System.Net.NetworkInformation;

    /// <summary>
    /// Class ConnectivityMonitor.
    /// </summary>
    internal static class ConnectivityMonitor
    {
        /// <summary>
        /// The timeout milliseconds
        /// </summary>
        private const int timeoutMilliseconds = 500;

        /// <summary>
        /// Tests the internet ping.
        /// </summary>
        /// <returns><c>true</c> if the test succeeded, <c>false</c> otherwise.</returns>
        public static bool TestInternetPing()
        {
            Ping ping = new Ping();

            try
            {
                PingReply response = ping.Send(IPAddress.Parse("8.8.8.8"), timeoutMilliseconds);
                bool result = ((response != null) && (response.Status == IPStatus.Success));
                return result;
            }
            catch (Exception e)
            {
                return false;
            }

        }

/*
        static ConnectivityMonitor()
        {
            //NOTE: this event does not seem to be called at all regardless of multiple network cable disconnections/connections!!!
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;

            //NOTE: this event seems to be only called once regardless of multiple network cable disconnections/connections!!!
            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
        }

        private static void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            //ConnectivityMonitor.ConnectivityAvailable = e.IsAvailable;
            throw new NotImplementedException();
        }

        public static bool ConnectivityAvailable { get; private set; }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204")) 
                    return true; 
            }
            catch
            {
                return false;
            }
        }

*/

    }
}
