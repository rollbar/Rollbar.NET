namespace Rollbar
{

    using Rollbar.Common;

    /// <summary>
    /// Class HttpProxyOptions.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// Implements the <see cref="Rollbar.IHttpProxyOptions" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// <seealso cref="Rollbar.IHttpProxyOptions" />
    public class HttpProxyOptions
        : ReconfigurableBase<HttpProxyOptions, IHttpProxyOptions>
        , IHttpProxyOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpProxyOptions"/> class.
        /// </summary>
        internal HttpProxyOptions()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpProxyOptions"/> class.
        /// </summary>
        /// <param name="proxyAddress">The proxy address.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        public HttpProxyOptions(string? proxyAddress, string? proxyUsername = null, string? proxyPassword = null)
        {
            this.ProxyAddress = proxyAddress;
            this.ProxyUsername = proxyUsername;
            this.ProxyPassword = proxyPassword;
        }

        /// <summary>
        /// Gets the proxy address.
        /// </summary>
        /// <value>The proxy address.</value>
        public string? ProxyAddress
        {
            get; 
            set;
        }

        /// <summary>
        /// Gets the proxy username.
        /// </summary>
        /// <value>The proxy username.</value>
        public string? ProxyUsername
        {
            get; 
            set;
        }

        /// <summary>
        /// Gets the proxy password.
        /// </summary>
        /// <value>The proxy password.</value>
        public string? ProxyPassword
        {
            get;
            set;
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        public override HttpProxyOptions Reconfigure(IHttpProxyOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator? GetValidator()
        {
            return null;
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        IHttpProxyOptions IReconfigurable<IHttpProxyOptions, IHttpProxyOptions>.Reconfigure(IHttpProxyOptions likeMe)
        {
            return this.Reconfigure(likeMe);
        }
    }
}
