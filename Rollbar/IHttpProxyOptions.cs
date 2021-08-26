namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    /// <summary>
    /// Interface IHttpProxyOptions
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{T, TBase}" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IReconfigurable{T, TBase}" />
    public interface IHttpProxyOptions
        : IReconfigurable<IHttpProxyOptions, IHttpProxyOptions>
    {
        /// <summary>
        /// Gets the proxy address.
        /// </summary>
        /// <value>The proxy address.</value>
        string? ProxyAddress
        {
            get;
        }

        /// <summary>
        /// Gets the proxy username.
        /// </summary>
        /// <value>The proxy username.</value>
        string? ProxyUsername
        {
            get;
        }

        /// <summary>
        /// Gets the proxy password.
        /// </summary>
        /// <value>The proxy password.</value>
        string? ProxyPassword
        {
            get;
        }
    }
}
