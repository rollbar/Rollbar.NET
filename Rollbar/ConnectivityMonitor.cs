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
            catch (Exception e)
            {
                return false;
            }

        }

    }
}
