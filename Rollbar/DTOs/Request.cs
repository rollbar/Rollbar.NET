namespace Rollbar.DTOs
{
    using Rollbar.Diagnostics;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Linq;
    using Rollbar.Common;

#if (NETSTANDARD || NETCOREAPP)
    using Microsoft.AspNetCore.Http;
#endif

#if (NETCOREAPP)
    using Rollbar.AspNetCore;
    using System.IO;
    using System.Text;
#endif

#if (NETFX)
    using System.ServiceModel.Channels;
    using System.Web;
    using Newtonsoft.Json;
#endif

    /// <summary>
    /// Models Rollbar HTTP Request DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.ExtendableDtoBase" />
    /// <remarks>
    /// Optional: request
    /// Data about the request this event occurred in.
    /// Can contain any arbitrary keys.
    /// </remarks>
    public class Request 
        : ExtendableDtoBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        public Request()
            : base(null)
        {
        }

#if (NETCOREAPP)

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        /// <param name="httpAttributes">The Rollbar HTTP attributes.</param>
        public Request(
            IDictionary<string, object> arbitraryKeyValuePairs
            , RollbarHttpAttributes httpAttributes
            )
            : base(arbitraryKeyValuePairs)
        {
            if (httpAttributes != null)
            {
                this.SnapProperties(httpAttributes);
            }
        }

        private void SnapProperties(RollbarHttpAttributes httpContext)
        {
            Assumption.AssertNotNull(httpContext, nameof(httpContext));

            this.Url = httpContext.Host.Value + httpContext.Path;
            this.QueryString = httpContext.Query.Value;
            this.Params = null;

            this.Headers = new Dictionary<string, string>(httpContext.Headers.Count());
            foreach (var header in httpContext.Headers)
            {
                if (header.Value.Count() == 0)
                    continue;

                this.Headers.Add(header.Key, StringUtility.Combine(header.Value, ", "));
            }

            this.Method = httpContext.Method;
        }

#endif

#if (NETCOREAPP)//(NETSTANDARD || NETCOREAPP)

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public Request(
            IDictionary<string, object> arbitraryKeyValuePairs
            )
            : this(arbitraryKeyValuePairs, null as HttpRequest)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        public Request(
            IDictionary<string, object> arbitraryKeyValuePairs
            , HttpRequest httpRequest
            )
            : base(arbitraryKeyValuePairs)
        {
            if (httpRequest != null)
            {
                this.SnapProperties(httpRequest);
            }
        }

        private void SnapProperties(HttpRequest httpRequest)
        {
            Assumption.AssertNotNull(httpRequest, nameof(httpRequest));

            this.Url = httpRequest.Host.Value + httpRequest.Path;
            this.QueryString = httpRequest.QueryString.Value;
            this.Params = null;

            this.Headers = new Dictionary<string, string>(httpRequest.Headers.Count());
            foreach (var header in httpRequest.Headers)
            {
                this.Headers.Add(header.Key, StringUtility.Combine(header.Value, ", "));
            }

            this.Method = httpRequest.Method;

            switch (this.Method.ToUpper())
            {
                case "POST":
                    httpRequest.Body.Seek(0, SeekOrigin.Begin);
                    this.PostBody = GetBodyAsString(httpRequest);
                    break;
            }
        }

        private static string GetBodyAsString(HttpRequest request, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            using (StreamReader reader = new StreamReader(request.Body, encoding))
            {
                return reader.ReadToEnd();
            }
        }

#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        public Request(
            IDictionary<string, object> arbitraryKeyValuePairs
            , IRollbarConfig rollbarConfig
            )
            : this(arbitraryKeyValuePairs, rollbarConfig, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request" /> class.
        /// </summary>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        public Request(
            IDictionary<string, object> arbitraryKeyValuePairs
            , IRollbarConfig rollbarConfig
            , HttpRequestMessage httpRequest
            ) 
            : base(arbitraryKeyValuePairs)
        {
            if (httpRequest != null)
            {
                Assumption.AssertNotNull(rollbarConfig, nameof(rollbarConfig));
                this.SnapProperties(httpRequest, rollbarConfig);
            }
        }

        private void SnapProperties(HttpRequestMessage httpRequest, IRollbarConfig rollbarConfig)
        {
            Assumption.AssertNotNull(httpRequest, nameof(httpRequest));

            this.Url = httpRequest.RequestUri?.AbsoluteUri;
            this.QueryString = httpRequest.RequestUri?.Query;
            this.Params = null;

            this.Headers = new Dictionary<string, string>(httpRequest.Headers.Count());
            foreach (var header in httpRequest.Headers)
            {
                this.Headers.Add(header.Key, StringUtility.Combine(header.Value, ", "));
            }

            this.Method = httpRequest.Method.Method;
            switch(this.Method.ToUpperInvariant())
            {
                case "POST":
                    var task = httpRequest.Content.ReadAsStringAsync();
                    task.Wait();
                    this.PostBody = task.Result;
                    this.PostParams = null;
                    break;
                case "GET":
                    this.GetParams = null;
                    break;
                default:
                    System.Diagnostics.Trace.WriteLine(
                        $"No-op processing {this.Method.ToUpperInvariant()} HTTP method."
                        );
                    break;
            }

#if (NETFX)
            string userIP = null;
            const string HttpContextProperty = "MS_HttpContext";
            const string RemoteEndpointMessagePropery = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
            if (httpRequest.Properties.ContainsKey(HttpContextProperty))
            {
                HttpContextBase ctx = httpRequest.Properties[HttpContextProperty] as HttpContextBase;
                if (ctx != null)
                {
                    userIP = ctx.Request.UserHostAddress;
                }
            }
            else if (httpRequest.Properties.ContainsKey(RemoteEndpointMessagePropery))
            {
                RemoteEndpointMessageProperty remoteEndpoint = 
                    httpRequest.Properties[RemoteEndpointMessagePropery] as RemoteEndpointMessageProperty;
                if (remoteEndpoint != null)
                {
                    userIP = remoteEndpoint.Address;
                }
            }
            this.UserIp = 
                Request.DecideCollectableUserIPValue(userIP, rollbarConfig.IpAddressCollectionPolicy);
#endif
        }

        private static string DecideCollectableUserIPValue(string initialIPAddress, IpAddressCollectionPolicy ipAddressCollectionPolicy)
        {
            switch(ipAddressCollectionPolicy)
            {
                case IpAddressCollectionPolicy.Collect:
                    return initialIPAddress;
                case IpAddressCollectionPolicy.CollectAnonymized:
                    return IpAddressUtility.Anonymize(initialIPAddress);
                case IpAddressCollectionPolicy.DoNotCollect:
                    return null;
                default:
                    Assumption.FailValidation(
                        "Unexpected enumeration value!"
                        , nameof(ipAddressCollectionPolicy)
                        );
                    break;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static class ReservedProperties
        {
            /// <summary>
            /// The URL
            /// </summary>
            public static readonly string Url = "url";
            /// <summary>
            /// The method
            /// </summary>
            public static readonly string Method = "method";
            /// <summary>
            /// The headers
            /// </summary>
            public static readonly string Headers = "headers";
            /// <summary>
            /// The parameters
            /// </summary>
            public static readonly string Params = "params";
            /// <summary>
            /// The get-parameters
            /// </summary>
            public static readonly string GetParams = "GET";
            /// <summary>
            /// The query string
            /// </summary>
            public static readonly string QueryString = "query_string";
            /// <summary>
            /// The post-parameters
            /// </summary>
            public static readonly string PostParams = "POST";
            /// <summary>
            /// The post body
            /// </summary>
            public static readonly string PostBody = "body";
            /// <summary>
            /// The user IP
            /// </summary>
            public static readonly string UserIp = "user_ip";
        }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        /// <remarks>
        /// url: full URL where this event occurred
        /// </remarks>
        public string Url
        {
            get { return this._keyedValues[ReservedProperties.Url] as string; }
            set { this._keyedValues[ReservedProperties.Url] = value; }
        }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        /// <remarks>
        /// method: the request method
        /// </remarks>
        public string Method
        {
            get { return this._keyedValues[ReservedProperties.Method] as string; }
            set { this._keyedValues[ReservedProperties.Method] = value; }
        }

        /// <summary>
        /// Gets or sets the HTTP request headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
        /// <remarks>
        /// headers: object containing the request headers
        /// </remarks>
        public IDictionary<string, string> Headers
        {
            get { return this._keyedValues[ReservedProperties.Headers] as IDictionary<string, string>; }
            set { this._keyedValues[ReservedProperties.Headers] = value; }
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        /// <remarks>
        /// params: any routing parameters
        /// </remarks>
        public IDictionary<string, object> Params
        {
            get { return this._keyedValues[ReservedProperties.Params] as IDictionary<string, object>; }
            set { this._keyedValues[ReservedProperties.Params] = value; }
        }

        /// <summary>
        /// Gets or sets the GET-parameters.
        /// </summary>
        /// <value>
        /// The GET-parameters.
        /// </value>
        /// <remarks>
        /// GET: query string params
        /// </remarks>
        public IDictionary<string, object> GetParams
        {
            get { return this._keyedValues[ReservedProperties.GetParams] as IDictionary<string, object>; }
            set { this._keyedValues[ReservedProperties.GetParams] = value; }
        }

        /// <summary>
        /// Gets or sets the query string.
        /// </summary>
        /// <value>
        /// The query string.
        /// </value>
        /// <remarks>
        /// query_string: the raw query string
        /// </remarks>
        public string QueryString
        {
            get { return this._keyedValues[ReservedProperties.QueryString] as string; }
            set { this._keyedValues[ReservedProperties.QueryString] = value; }
        }

        /// <summary>
        /// Gets or sets the post-parameters.
        /// </summary>
        /// <value>
        /// The post-parameters.
        /// </value>
        /// <remarks>
        /// POST: POST params
        /// </remarks>
        public IDictionary<string, object> PostParams
        {
            get { return this._keyedValues[ReservedProperties.PostParams] as IDictionary<string, object>; }
            set { this._keyedValues[ReservedProperties.PostParams] = value; }
        }

        /// <summary>
        /// Gets or sets the post-body.
        /// </summary>
        /// <value>
        /// The post-body.
        /// </value>
        /// <remarks>
        /// body: the raw POST body
        /// </remarks>
        public string PostBody
        {
            get { return this._keyedValues[ReservedProperties.PostBody] as string; }
            set { this._keyedValues[ReservedProperties.PostBody] = value; }
        }

        /// <summary>
        /// Gets or sets the user IP.
        /// </summary>
        /// <value>
        /// The user IP.
        /// </value>
        /// <remarks>
        /// user_ip: the user's IP address as a string.
        /// Can also be the special value "$remote_ip", which will be replaced with the source IP of the API request.
        /// Will be indexed, as long as it is a valid IPv4 address.
        /// </remarks>
        public string UserIp
        {
            get { return this._keyedValues[ReservedProperties.UserIp] as string; }
            set { this._keyedValues[ReservedProperties.UserIp] = value; }
        }

    }
}
