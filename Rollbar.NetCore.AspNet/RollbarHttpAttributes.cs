namespace Rollbar.NetCore.AspNet
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;

    /// <summary>
    /// Implements a capture bag of HTTP context attributes of interest.
    /// </summary>
    public class RollbarHttpAttributes
    {
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
        public RollbarHttpAttributes(HttpContext context)
        {
            // as we learned from a field issue, 
            // apparently a middleware can be invoked without a valid HttPContext:
            if (context == null)
            {
                return;
            }

            this.RequestID = context.Features?
                .Get<IHttpRequestIdentifierFeature>()?
                .TraceIdentifier;
            if (context.Request !=  null)
            {
                this.Host = context.Request.Host;
                this.Path = context.Request.Path;
                this.Headers = context.Request.Headers;
                this.Method = context.Request.Method;
                this.Query = context.Request.QueryString;
                this.Protocol = context.Request.Protocol;
                this.Scheme = context.Request.Scheme;
            }
            if (context.Response != null)
            {
                this.StatusCode = context.Response.StatusCode;
            }
        }

        /// <summary>
        /// Gets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public string RequestID { get; }

        /// <summary>
        /// Gets the host.
        /// </summary>
        /// <value>
        /// The host.
        /// </value>
        public HostString Host { get; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public PathString Path { get; }

        /// <summary>
        /// Gets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public string Method { get; }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
        public IHeaderDictionary Headers { get; }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <value>
        /// The query.
        /// </value>
        public QueryString Query { get; }

        /// <summary>
        /// Gets the protocol.
        /// </summary>
        /// <value>
        /// The protocol.
        /// </value>
        public string Protocol { get; }

        /// <summary>
        /// Gets the scheme.
        /// </summary>
        /// <value>
        /// The scheme.
        /// </value>
        public string Scheme { get; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public int StatusCode { get; set; }

    }
}
