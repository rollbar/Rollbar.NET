#if NETFX

namespace Rollbar.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;

    public class RollbarHttpModule
        : IHttpModule
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            throw new NotImplementedException();
        }
    }
}

#endif
