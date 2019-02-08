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
            //throw new NotImplementedException();

            //RollbarLocator.RollbarInstance.Info("Next will be an exception!");
            //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromHours(10)).Critical(filterContext.Exception);

            //var data = new ExceptionContextPackagingStrategy(filterContext).PackageAsRollbarData();

            RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromHours(10)).Critical(new ExceptionContextPackagingStrategy(filterContext));
        }
    }
}

#endif
