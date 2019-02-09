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

    public class HttpContextPackagingStrategyDecorator
            : RollbarPackagingStrategyDecoratorBase
    {
        private readonly IRollbarPackagingStrategy _strategyToDecorate;
        private readonly HttpContext _httpContext;

        public HttpContextPackagingStrategyDecorator(IRollbarPackagingStrategy strategyToDecorate, HttpContext httpContext)
            : base(strategyToDecorate)
        {
            Assumption.AssertNotNull(httpContext, nameof(httpContext));

            this._httpContext = httpContext;

            this._strategyToDecorate = strategyToDecorate;
        }

        public override Data PackageAsRollbarData()
        {
            if (this._httpContext?.Request != null)
            {
                // here we essentially piggy-back on capabilities of already implemented HttpRequestPackagingStrategyDecorator instead of this decorator:
                IRollbarPackagingStrategy strategy = new HttpRequestPackagingStrategyDecorator(this._strategyToDecorate, new HttpRequestWrapper(this._httpContext.Request));
                return strategy.PackageAsRollbarData();
            }

            return base.PackageAsRollbarData();
        }
    }
}

#endif
