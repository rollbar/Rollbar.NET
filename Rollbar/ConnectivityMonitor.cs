using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class ConnectivityMonitor
    {
        static ConnectivityMonitor()
        {
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
        }

        private static void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            ConnectivityMonitor.ConnectivityAvailable = e.IsAvailable;
        }

        public static bool ConnectivityAvailable { get; private set; }
    }
}
