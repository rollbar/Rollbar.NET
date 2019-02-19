#if NETFX

namespace Rollbar.AspNet.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    /// <summary>
    /// Class RollbarExceptionFilter.
    /// Implements the <see cref="System.Web.Mvc.IExceptionFilter" /></summary>
    /// <seealso cref="System.Web.Mvc.IExceptionFilter" />
    public class RollbarExceptionFilter
            : IExceptionFilter
    {
        /// <summary>
        /// The common rollbar data title
        /// </summary>
        private readonly string _commonRollbarDataTitle;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarExceptionFilter" /> class.
        /// </summary>
        public RollbarExceptionFilter()
                    : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarExceptionFilter" /> class.
        /// </summary>
        /// <param name="commonRollbarDataTitle">The common rollbar data title.</param>
        public RollbarExceptionFilter(string commonRollbarDataTitle)
        {
            this._commonRollbarDataTitle = commonRollbarDataTitle;
        }

        /// <summary>
        /// Called when an exception occurs.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnException(ExceptionContext filterContext)
        {
            RollbarLocator.RollbarInstance.Critical(
                new ExceptionContextPackage(filterContext, this._commonRollbarDataTitle)
                );
        }
    }
}

#endif
