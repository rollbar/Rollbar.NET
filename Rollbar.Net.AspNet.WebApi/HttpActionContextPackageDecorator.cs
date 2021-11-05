namespace Rollbar.Net.AspNet.WebApi
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http.Controllers;
    using System.Collections.Generic;
    using System.Linq;
    using Rollbar.DTOs;
    using System.Text;

    /// <summary>
    /// Class HttpActionContextPackageDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackageDecoratorBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageDecoratorBase" />
    public class HttpActionContextPackageDecorator
        : RollbarPackageDecoratorBase
    {
        /// <summary>
        /// The HTTP action context
        /// </summary>
        private readonly HttpActionContext _httpActionContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpActionContextPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpActionContext">The HTTP action context.</param>
        public HttpActionContextPackageDecorator(IRollbarPackage packageToDecorate, HttpActionContext httpActionContext)
            : this(packageToDecorate, httpActionContext, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpActionContextPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpActionContext">The HTTP action context.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public HttpActionContextPackageDecorator(IRollbarPackage packageToDecorate, HttpActionContext httpActionContext, bool mustApplySynchronously) 
            : base(packageToDecorate, mustApplySynchronously)
        {
            this._httpActionContext = httpActionContext;
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

            if(this._httpActionContext == null)
            {
                return; //nothing to do...
            }

            if (rollbarData.Request == null)
            {
                rollbarData.Request = new Request();
            }

            if (rollbarData.Request.Params == null)
            {
                rollbarData.Request.Params = new Dictionary<string, object>();
            }
            rollbarData.Request.Params["controller"] =
                this._httpActionContext.ControllerContext?.ControllerDescriptor?.ControllerName ?? string.Empty;
            rollbarData.Request.Params["action"] = 
                this._httpActionContext.ActionDescriptor?.ActionName ?? string.Empty;

            if (this._httpActionContext.Request != null)
            {
                rollbarData.Request.Url = this._httpActionContext.Request.RequestUri?.PathAndQuery;
                rollbarData.Request.Method = this._httpActionContext.Request.Method?.ToString().ToUpper();
                rollbarData.Request.Headers = Convert(this._httpActionContext.Request.Headers);
                rollbarData.Request.PostBody = ReadInHttpMessageBody(this._httpActionContext.Request.Content);
                rollbarData.Request["request_properties"] = this._httpActionContext.Request.Properties;
            }

            if (this._httpActionContext.Response != null)
            {
                if (rollbarData.Response == null)
                {
                    rollbarData.Response = new Response();
                }

                rollbarData.Response.StatusCode = (int) this._httpActionContext.Response.StatusCode;

                if (this._httpActionContext.Response.Headers != null)
                {
                    rollbarData.Response.Headers = Convert(this._httpActionContext.Response.Headers);
                }

                rollbarData.Response.Body = ReadInHttpMessageBody(this._httpActionContext.Response.Content);
                rollbarData.Response["reason_phrase"] = this._httpActionContext.Response.ReasonPhrase;
            }
        }

        /// <summary>
        /// Reads the in HTTP message body.
        /// </summary>
        /// <param name="httpContent">Content of the HTTP.</param>
        /// <returns>System.String.</returns>
        private string? ReadInHttpMessageBody(HttpContent httpContent)
        {
            if (httpContent == null)
            {
                return null;
            }
            var resultTask = this._httpActionContext.Response?.Content?.ReadAsStringAsync();
            if (resultTask == null)
            {
                return null;
            }
            resultTask.Wait();
            return resultTask.Result;
        }

        /// <summary>
        /// Converts the specified HTTP headers.
        /// </summary>
        /// <param name="httpHeaders">The HTTP headers.</param>
        /// <returns>IDictionary&lt;System.String, System.String&gt;.</returns>
        private static IDictionary<string, string> Convert(HttpHeaders httpHeaders)
        {
            if (httpHeaders == null)
            {
                return new Dictionary<string, string>(0);
            }

            Dictionary<string, string> headers = new(httpHeaders.Count());
            foreach (var httpHeader in httpHeaders)
            {
                StringBuilder valueBuilder = new();
                foreach (var item in httpHeader.Value)
                {
                    valueBuilder.Append(item + ", ");
                }
                headers[httpHeader.Key] = valueBuilder.ToString().TrimEnd(' ').TrimEnd(',');
            }

            return headers;
        }
    }
}
