#if NETFX

namespace Rollbar.AspNet.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public class RollbarExceptionFilter
        : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            throw new NotImplementedException();
        }
    }
}

#endif
