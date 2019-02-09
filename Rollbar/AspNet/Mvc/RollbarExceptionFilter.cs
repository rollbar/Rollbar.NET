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
        private readonly string _commonRollbarDataTitle;

        public RollbarExceptionFilter()
            : this(null)
        {

        }

        public RollbarExceptionFilter(string commonRollbarDataTitle)
        {
            this._commonRollbarDataTitle = commonRollbarDataTitle;
        }

        public void OnException(ExceptionContext filterContext)
        {
            RollbarLocator.RollbarInstance.Critical(new ExceptionContextPackagingStrategy(filterContext, this._commonRollbarDataTitle));
        }
    }
}

#endif
