#if NETFX

namespace Rollbar.AspNet.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Hosting;
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Class HttpContextPackageDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackageDecoratorBase" /></summary>
    /// <seealso cref="Rollbar.RollbarPackageDecoratorBase" />
    public class HttpContextPackageDecorator
                : RollbarPackageDecoratorBase
    {
        /// <summary>
        /// The strategy to decorate
        /// </summary>
        private readonly IRollbarPackage _packageToDecorate;
        /// <summary>
        /// The HTTP context
        /// </summary>
        private readonly HttpContext _httpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextPackageDecorator" /> class.
        /// </summary>
        /// <param name="strategyToDecorate">The strategy to decorate.</param>
        /// <param name="httpContext">The HTTP context.</param>
        public HttpContextPackageDecorator(IRollbarPackage packageToDecorate, HttpContext httpContext)
                    : base(packageToDecorate, false)
        {
            Assumption.AssertNotNull(httpContext, nameof(httpContext));

            this._httpContext = httpContext;

            this._packageToDecorate = packageToDecorate;
        }

        /// <summary>
        /// Decorates the specified rollbar data.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        protected override void Decorate(Data rollbarData)
        {
            if (this._httpContext?.Request != null)
            {
                // here we essentially piggy-back on capabilities of 
                // already implemented HttpRequestPackageDecorator 
                // instead of this decorator:
                HttpRequestPackageDecorator strategy =
                    new HttpRequestPackageDecorator(this._packageToDecorate, new HttpRequestWrapper(this._httpContext.Request));
                strategy.PackageAsRollbarData();
            }
        }
    }
}

#endif
