#if NETFX

namespace Rollbar.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;

    /// <summary>
    /// Class RollbarHttpModule.
    /// Implements the <see cref="System.Web.IHttpModule" /></summary>
    /// <seealso cref="System.Web.IHttpModule" />
    public class RollbarHttpModule
            : IHttpModule
    {
        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule" />.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication" /> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            throw new NotImplementedException();
        }
    }
}

#endif
