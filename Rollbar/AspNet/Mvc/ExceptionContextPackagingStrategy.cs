#if NETFX

namespace Rollbar.AspNet.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Rollbar.DTOs;

    /// <summary>
    /// Class ExceptionContextPackagingStrategy.
    /// Implements the <see cref="Rollbar.RollbarPackagingStrategyBase" /></summary>
    /// <seealso cref="Rollbar.RollbarPackagingStrategyBase" />
    public class ExceptionContextPackagingStrategy
            : RollbarPackagingStrategyBase
    {
        /// <summary>
        /// The exception context
        /// </summary>
        private readonly ExceptionContext _exceptionContext;

        /// <summary>
        /// Prevents a default instance of the <see cref="ExceptionContextPackagingStrategy" /> class from being created.
        /// </summary>
        private ExceptionContextPackagingStrategy()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContextPackagingStrategy" /> class.
        /// </summary>
        /// <param name="exceptionContext">The exception context.</param>
        public ExceptionContextPackagingStrategy(ExceptionContext exceptionContext)
        {
            this._exceptionContext = exceptionContext;
        }

        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar.DTOs.Data.</returns>
        public override Data PackageAsRollbarData()
        {
            if (this._exceptionContext == null)
            {
                return null;
            }

            Body rollbarBody = new Body(this._exceptionContext.Exception);
            IDictionary<string, object> custom = null;
            RollbarUtility.SnapExceptionDataAsCustomData(this._exceptionContext.Exception, ref custom);
            Data rollbarData = new Data(rollbarBody, custom);

            return rollbarData;
        }
    }
}

#endif
