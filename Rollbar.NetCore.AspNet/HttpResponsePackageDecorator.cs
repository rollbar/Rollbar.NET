namespace Rollbar.NetCore.AspNet
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;
    using Rollbar.Common;
    using Rollbar.DTOs;
    using Rollbar.Serialization.Json;

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
        private readonly HttpResponse _httpResponse;

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
            : base(packageToDecorate, mustApplySynchronously)
        {
            this._httpResponse = httpResponse;
        }

        /// <summary>
        /// Decorates the specified rollbar data.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        protected override void Decorate(Data rollbarData)
        {
            if (this._httpResponse == null)
            {
                return; // nothing to decorate with... 
            }

            if (rollbarData.Response == null)
            {
                rollbarData.Response = new Response();
            }

            rollbarData.Response.StatusCode = this._httpResponse.StatusCode;

            if (this._httpResponse.Headers?.Count > 0)
            {
                rollbarData.Response.Headers = new Dictionary<string, string>(this._httpResponse.Headers.Count);
                foreach (var header in this._httpResponse.Headers)
                {
                    rollbarData.Response.Headers.Add(header.Key, StringUtility.Combine(header.Value, ", "));
                }
            }

            AssignResponseBody(rollbarData);
        }

        /// <summary>
        /// Assigns the response body.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        private void AssignResponseBody(Data rollbarData)
        {
            if (this._httpResponse == null || this._httpResponse.Body == null)
            {
                return; // nothing to do...
            }

            string jsonString = StreamUtil.ConvertToString(this._httpResponse.Body);
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return;
            }
            rollbarData.Response.Body = jsonString;

            object responseBodyObject = JsonUtil.InterpretAsJsonObject(jsonString);
            if (responseBodyObject != null)
            {
                rollbarData.Response.Body = responseBodyObject;
            }
        }

    }
}
