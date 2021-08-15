namespace Rollbar.Net.AspNet
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Class HttpResponsePackageDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackageDecoratorBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageDecoratorBase" />
    public class HttpResponsePackageDecorator
        : RollbarPackageDecoratorBase
    {
        /// <summary>
        /// The HTTP response
        /// </summary>
        private readonly HttpResponseBase _httpResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponsePackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpResponse">The HTTP response.</param>
        public HttpResponsePackageDecorator(IRollbarPackage packageToDecorate, HttpResponse httpResponse)
            : this(packageToDecorate, httpResponse, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponsePackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpResponse">The HTTP response.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public HttpResponsePackageDecorator(IRollbarPackage packageToDecorate, HttpResponse httpResponse, bool mustApplySynchronously)
            : this(packageToDecorate, new HttpResponseWrapper(httpResponse), mustApplySynchronously)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponsePackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpResponse">The HTTP response.</param>
        public HttpResponsePackageDecorator(IRollbarPackage packageToDecorate, HttpResponseBase httpResponse)
            : this(packageToDecorate, httpResponse, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponsePackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpResponse">The HTTP response.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public HttpResponsePackageDecorator(IRollbarPackage packageToDecorate, HttpResponseBase httpResponse, bool mustApplySynchronously) 
            : base(packageToDecorate, mustApplySynchronously)
        {
            Assumption.AssertNotNull(httpResponse, nameof(httpResponse));

            this._httpResponse = httpResponse;
        }

        /// <summary>
        /// Decorates the specified rollbar data.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        protected override void Decorate(Data? rollbarData)
        {
            if(rollbarData == null)
            {
                return;
            }

            if(this._httpResponse == null || rollbarData == null)
            {
                return; //nothing to do...
            }

            if (rollbarData.Response == null)
            {
                rollbarData.Response = new Response();
            }

            rollbarData.Response.StatusCode = this._httpResponse.StatusCode;
            rollbarData.Response.Headers = this._httpResponse.Headers?.ToStringDictionary();

            // some custom fields goodies:
            rollbarData.Response.Add("sub_status_code", this._httpResponse.SubStatusCode);
            if (!string.IsNullOrWhiteSpace(this._httpResponse.Status))
            {
                rollbarData.Response.Add("status", this._httpResponse.Status);
            }
            if (!string.IsNullOrWhiteSpace(this._httpResponse.Charset))
            {
                rollbarData.Response.Add("charset", this._httpResponse.Charset);
            }
            if (!string.IsNullOrWhiteSpace(this._httpResponse.RedirectLocation))
            {
                rollbarData.Response.Add("redirect_location", this._httpResponse.RedirectLocation);
            }
            if (!string.IsNullOrWhiteSpace(this._httpResponse.StatusDescription))
            {
                rollbarData.Response.Add("status_description", this._httpResponse.StatusDescription);
            }
        }
    }
}
