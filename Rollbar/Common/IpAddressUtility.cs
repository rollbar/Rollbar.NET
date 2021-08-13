namespace Rollbar.Common
{
    using System;

    /// <summary>
    /// A utility class aiding in working with IP addresses.
    /// </summary>
    public static class IpAddressUtility
    {
        /// <summary>
        /// Anonymizes the given IP address.
        /// </summary>
        /// <param name="exactIpAddress">The specified IP address to anonymize.</param>
        /// <returns></returns>
        public static string? Anonymize(string exactIpAddress)
        {
            if (string.IsNullOrEmpty(exactIpAddress))
            {
                return exactIpAddress;
            }

            string[]? components = null;

            // try IPv4 format:
            const char ipv4ComponentsSeparator = '.';
            const int ipv4TotalComponents = 4;
            const string ipv4Anonymizer = "0/24";
            components = exactIpAddress.Split(ipv4ComponentsSeparator);
            if (components != null 
                && components.Length == ipv4TotalComponents
                )
            {
                components[ipv4TotalComponents - 1] = ipv4Anonymizer;
                return string.Join($"{ipv4ComponentsSeparator}", components);
            }

            // try IPv6 format:
            const char ipv6CompontsSeparator = ':';
            const string ipv6Anonymizer = "...";
            components = exactIpAddress.Split(ipv6CompontsSeparator);
            if (components != null 
                && components.Length > 0
                )
            {
                const int ipv6CutOffLength = 12;
                if (exactIpAddress.Length > ipv6CutOffLength)
                {
                    return exactIpAddress.Substring(0, ipv6CutOffLength) + ipv6Anonymizer;
                }
                else
                {
                    return exactIpAddress;
                }
            }

            return null;
        }
    }
}
