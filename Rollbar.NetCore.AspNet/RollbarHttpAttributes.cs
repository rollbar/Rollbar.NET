namespace Rollbar.NetCore.AspNet
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;

#if(NETSTANDARD_2_0 || NETCORE_2_0)
    using Microsoft.AspNetCore.Http.Internal;
#endif

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
        public RollbarHttpAttributes(HttpContext? context)
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
                this.RequestHost = context.Request.Host;
                this.RequestPath = context.Request.Path;
                this.RequestHeaders = context.Request.Headers;
                this.RequestMethod = context.Request.Method;
                this.RequestQuery = context.Request.QueryString;
                this.RequestProtocol = context.Request.Protocol;
                this.RequestScheme = context.Request.Scheme;
                this.RequestBody = RollbarHttpAttributes.CaptureRequestBody(context.Request).Result;
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
        /// Captures the request body.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.String.</returns>
        public static async Task<string?> CaptureRequestBody(HttpRequest request)
        {
            var body = string.Empty;
            if(request.ContentLength == null
                || !(request.ContentLength > 0)
                || !request.Body.CanSeek
                )
            {
                return body;
            }

            // Prepare to snal the request body:
            //Stream bodyStream = request.Body; // cache body stream
#if(NETSTANDARD_2_0 || NETCORE_2_0)
            request.EnableRewind();     // so that we can rewind the body stream once we are done
#else
            request.EnableBuffering();  // so that we can rewind the body stream once we are done
#endif

            ////Snap the body stream content:
            //byte[] dataBuffer = new byte[Convert.ToInt32(request.ContentLength)];
            //request.Body.Seek(0, SeekOrigin.Begin);
            //await request.Body.ReadAsync(dataBuffer, 0, dataBuffer.Length);
            //string bodyContent = Encoding.UTF8.GetString(dataBuffer);
            //return bodyContent;

            //string? bodyContent = null;
            //using(StreamReader stream = new StreamReader(request.Body))
            //{
            //    bodyContent = stream.ReadToEnd();
            //}

            //// Rewind/restore the request body back:
            //request.Body = bodyStream;

            ////return bodyContent;
            //return bodyContent;



            request.Body.Position = 0;
            using(var ms = new MemoryStream())
            {
                request.Body.CopyTo(ms);
                var b = ms.ToArray();
                body = Encoding.UTF8.GetString(b); //Assign body to bodyStr
            }

            return body;
        }

        /// <summary>
        /// Captures the response body.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>System.String.</returns>
        public static async Task<string> CaptureResponseBody(HttpResponse response)
        {
            // Make sure we start at the beginning of the body stream:
            response.Body.Seek(0, SeekOrigin.Begin);

            // Snap the body stream content:
            string bodyContent = await new StreamReader(response.Body).ReadToEndAsync();

            // Reset back to the beginning of the body stream:
            response.Body.Seek(0, SeekOrigin.Begin);

            //Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)
            return bodyContent;
        }


    }
}
