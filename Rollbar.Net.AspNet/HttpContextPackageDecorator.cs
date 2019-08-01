namespace Rollbar.Net.AspNet
{
    using System.Web;
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
        private readonly HttpContextBase _httpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpContext">The HTTP context.</param>
        public HttpContextPackageDecorator(IRollbarPackage packageToDecorate, HttpContext httpContext)
            : this(packageToDecorate, new HttpContextWrapper(httpContext), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public HttpContextPackageDecorator(IRollbarPackage packageToDecorate, HttpContext httpContext, bool mustApplySynchronously)
            : this(packageToDecorate, new HttpContextWrapper(httpContext), mustApplySynchronously)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpContext">The HTTP context.</param>
        public HttpContextPackageDecorator(IRollbarPackage packageToDecorate, HttpContextBase httpContext)
            : this(packageToDecorate,  httpContext, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public HttpContextPackageDecorator(IRollbarPackage packageToDecorate, HttpContextBase httpContext, bool mustApplySynchronously)
                    : base(packageToDecorate, mustApplySynchronously)
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
            IRollbarPackage package = this._packageToDecorate;

            if (this._httpContext?.Request != null)
            {
                // here we essentially piggy-back on capabilities of 
                // already implemented HttpRequestPackageDecorator 
                // instead of this decorator:
                package =
                    new HttpRequestPackageDecorator(package, this._httpContext.Request);
            }

            if (this._httpContext?.Response != null)
            {
                // here we essentially piggy-back on capabilities of 
                // already implemented HttpResponsePackageDecorator 
                // instead of this decorator:
                package =
                    new HttpResponsePackageDecorator(package, this._httpContext.Response);
            }

            if (package != this._packageToDecorate)
            {
                package.PackageAsRollbarData();
            }
        }
    }
}
