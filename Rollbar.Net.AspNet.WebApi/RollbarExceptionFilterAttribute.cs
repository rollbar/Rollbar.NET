namespace Rollbar.Net.AspNet.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Filters;

    /// <summary>
    /// Class RollbarExceptionFilterAttribute.
    /// Implements the <see cref="System.Web.Http.Filters.ExceptionFilterAttribute" />
    /// Implements the <see cref="System.Web.Http.Filters.IExceptionFilter" /></summary>
    /// <seealso cref="System.Web.Http.Filters.ExceptionFilterAttribute" />
    /// <seealso cref="System.Web.Http.Filters.IExceptionFilter" />
    public class RollbarExceptionFilterAttribute
            : ExceptionFilterAttribute
            , IExceptionFilter
    {
        /// <summary>
        /// Called when [exception].
        /// </summary>
        /// <param name="actionExecutedContext">The action executed context.</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            IRollbarPackage rollbarPackage = 
                new ExceptionPackage(actionExecutedContext.Exception, nameof(this.OnException));

            RollbarLocator.RollbarInstance.Critical(rollbarPackage);

            base.OnException(actionExecutedContext);
        }
    }
}
