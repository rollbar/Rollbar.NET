namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IHttpProxyOptions
    {
        /// <summary>
        /// Gets the proxy address.
        /// </summary>
        /// <value>
        /// The proxy address.
        /// </value>
        string ProxyAddress
        {
            get;
        }

        /// <summary>
        /// Gets the proxy username.
        /// </summary>
        /// <value>The proxy username.</value>
        string ProxyUsername
        {
            get;
        }

        /// <summary>
        /// Gets the proxy password.
        /// </summary>
        /// <value>The proxy password.</value>
        string ProxyPassword
        {
            get;
        }

    }
}
