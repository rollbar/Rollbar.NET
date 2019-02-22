#if NETFX

namespace Rollbar.AspNet.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Filters;

    public class RollbarExceptionFilterAttribute
        : ExceptionFilterAttribute
        , IExceptionFilter
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);
        }
    }
}

#endif
