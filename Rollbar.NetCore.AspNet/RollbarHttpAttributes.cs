namespace Rollbar.NetCore.AspNet
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;

    using Rollbar.Common;

    /// <summary>
    /// Implements a capture bag of HTTP context attributes of interest.
    /// </summary>
    public class RollbarHttpAttributes
    {
        private readonly HttpContext? _httpContext;

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarHttpAttributes" /> class from being created.
        /// </summary>
        private RollbarHttpAttributes()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarHttpAttributes" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public RollbarHttpAttributes(HttpContext? context)
        {
            // as we learned from a field issue, 
            // apparently a middleware can be invoked without a valid HttPContext:
            if (context == null)
            {
                return;
            }

            this._httpContext = context;

            this.RequestID = context.Features?
                .Get<IHttpRequestIdentifierFeature>()?
                .TraceIdentifier;

            if (context.Request !=  null)
            {
                this.RequestHost = context.Request.Host;
                this.RequestPath = context.Request.Path;
                this.RequestHeaders = context.Request.Headers;
                this.RequestMethod = context.Request.Method;
                this.RequestQuery = context.Request.QueryString;
                this.RequestProtocol = context.Request.Protocol;
                this.RequestScheme = context.Request.Scheme;
                this.RequestBody = RollbarHttpAttributes.CaptureRequestBody(context.Request);
            }

            if (context.Response != null)
            {
                this.ResponseStatusCode = context.Response.StatusCode;
                this.ResponseHeaders = context.Response.Headers;
            }
        }

        /// <summary>
        /// Gets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public string? RequestID { get; }

        /// <summary>
        /// Gets the host.
        /// </summary>
        /// <value>The host.</value>
        public HostString RequestHost { get; }

        /// <summary>
        /// Gets the request path.
        /// </summary>
        /// <value>The request path.</value>
        public PathString RequestPath { get; }

        /// <summary>
        /// Gets the request method.
        /// </summary>
        /// <value>The request method.</value>
        public string? RequestMethod { get; }

        /// <summary>
        /// Gets the request body.
        /// </summary>
        /// <value>The request body.</value>
        public string? RequestBody { get; }

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        /// <value>The request headers.</value>
        public IHeaderDictionary? RequestHeaders { get; }

        /// <summary>
        /// Gets the request query.
        /// </summary>
        /// <value>The request query.</value>
        public QueryString RequestQuery { get; }

        /// <summary>
        /// Gets the request protocol.
        /// </summary>
        /// <value>The request protocol.</value>
        public string? RequestProtocol { get; }

        /// <summary>
        /// Gets the request scheme.
        /// </summary>
        /// <value>The request scheme.</value>
        public string? RequestScheme { get; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>The status code.</value>
        public int ResponseStatusCode { get; set; }

        /// <summary>
        /// Gets the response headers.
        /// </summary>
        /// <value>The response headers.</value>
        public IHeaderDictionary? ResponseHeaders { get; }

        /// <summary>
        /// Gets the response body.
        /// </summary>
        /// <value>The response body.</value>
        public string? ResponseBody
        {
            get
            {
                return RollbarHttpAttributes.CaptureResponseBody(this._httpContext?.Response);
            }
        }

        /// <summary>
        /// Captures the request body.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.String.</returns>
        public static string? CaptureRequestBody(HttpRequest? request)
        {
            if(request == null
                || request.ContentLength == null
                || !(request.ContentLength > 0)
                || !request.Body.CanSeek
                )
            {
                return null;
            }

            string? bodyContent = 
                StreamUtil.CaptureAsStringAsync(request.BodyReader.AsStream(true)).Result;
            
            return bodyContent;
        }

        /// <summary>
        /// Captures the response body.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>System.String.</returns>
        public static string? CaptureResponseBody(HttpResponse? response)
        {

            if (response == null
                || response.ContentLength == null
                || !(response.ContentLength > 0)
                || !response.Body.CanSeek
                )
            {
                return null;
            }

            string? bodyContent = 
                StreamUtil.CaptureAsStringAsync(response.BodyWriter.AsStream(true)).Result;

            return bodyContent;
        }
    }
}
