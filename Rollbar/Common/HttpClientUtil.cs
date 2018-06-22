namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;

    /// <summary>
    /// HttpClient utility class.
    /// </summary>
    public static class HttpClientUtil
    {
        /// <summary>
        /// Creates the HTTP client.
        /// </summary>
        /// <param name="proxySettings">The proxy settings.</param>
        /// <returns></returns>
        public static  HttpClient CreateHttpClient(string proxySettings = null)
        {
            if (string.IsNullOrWhiteSpace(proxySettings))
            {
                // create new instance with no proxy settings:
                return new HttpClient();
            }

            // create new instance with proxy settings:
            var webProxy = new WebProxy(proxySettings);

            var httpHandler = new HttpClientHandler();
            httpHandler.Proxy = webProxy;
            httpHandler.PreAuthenticate = true;
            httpHandler.UseDefaultCredentials = false;

            return new HttpClient(httpHandler);
        }
    }
}
