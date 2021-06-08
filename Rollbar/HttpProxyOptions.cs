namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    public class HttpProxyOptions
        : ReconfigurableBase<HttpProxyOptions, IHttpProxyOptions>
        , IHttpProxyOptions
    {
        internal HttpProxyOptions()
            : this(null, null, null)
        {
        }

        public HttpProxyOptions(string proxyAddress, string proxyUsername = null, string proxyPassword = null)
        {
            this.ProxyAddress = proxyAddress;
            this.ProxyUsername = proxyUsername;
            this.ProxyPassword = proxyPassword;
        }

        public string ProxyAddress
        {
            get; 
            set;
        }

        public string ProxyUsername
        {
            get; 
            set;
        }

        public string ProxyPassword
        {
            get;
            set;
        }

        IHttpProxyOptions IReconfigurable<IHttpProxyOptions,IHttpProxyOptions>.Reconfigure(IHttpProxyOptions likeMe)
        {
            return this.Reconfigure(likeMe);
        }

        public override Validator GetValidator()
        {
            return null;
        }

    }
}
