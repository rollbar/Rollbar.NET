namespace Rollbar.Common
{
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// Class HttpClientUtility.
    /// </summary>
    public static class HttpClientUtility
    {
        /// <summary>
        /// Creates the HTTP client.
        /// </summary>
        /// <returns>HttpClient.</returns>
        public static HttpClient CreateHttpClient()
        {
            return HttpClientUtility.CreateHttpClient(null);
        }

        /// <summary>
        /// Creates the HTTP client.
        /// </summary>
        /// <param name="proxyAddress">The proxy address.</param>
        /// <returns>HttpClient.</returns>
        public static HttpClient CreateHttpClient(string? proxyAddress)
        {
            return HttpClientUtility.CreateHttpClient(proxyAddress, null, null);
        }

        /// <summary>
        /// Creates the HTTP client.
        /// </summary>
        /// <param name="proxyAddress">The proxy settings.</param>
        /// <param name="proxyUsername">The proxy user name.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <returns>HttpClient.</returns>
        public static  HttpClient CreateHttpClient(string? proxyAddress, string? proxyUsername, string? proxyPassword)
        {
            if (string.IsNullOrWhiteSpace(proxyAddress))
            {
                // create new instance with no proxy settings:
                return new HttpClient();
            }

            ICredentials? credentials = null;
            if (!string.IsNullOrWhiteSpace(proxyUsername) && !string.IsNullOrWhiteSpace(proxyPassword))
            {
                credentials = new NetworkCredential(proxyUsername, proxyPassword);
            }

            // create new instance with proxy settings:
            var webProxy = (credentials != null) ? new WebProxy(proxyAddress, true, null, credentials) : new WebProxy(proxyAddress);

            var httpHandler = new HttpClientHandler();
            httpHandler.Proxy = webProxy;
            httpHandler.PreAuthenticate = true;
            httpHandler.UseDefaultCredentials = false;

            return new HttpClient(httpHandler);
        }
    }
}
