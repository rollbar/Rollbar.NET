namespace Rollbar.DTOs
{
    using System.Collections.Generic;

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
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public Request(
            IDictionary<string, object?>? arbitraryKeyValuePairs
            )
            : base(arbitraryKeyValuePairs)
        {

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
        public string? Url
        {
            get { return this[ReservedProperties.Url] as string; }
            set { this[ReservedProperties.Url] = value; }
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
        public string? Method
        {
            get { return this[ReservedProperties.Method] as string; }
            set { this[ReservedProperties.Method] = value; }
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
        public IDictionary<string, string>? Headers
        {
            get { return this[ReservedProperties.Headers] as IDictionary<string, string>; }
            set { this[ReservedProperties.Headers] = value; }
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
        public IDictionary<string, object>? Params
        {
            get { return this[ReservedProperties.Params] as IDictionary<string, object>; }
            set { this[ReservedProperties.Params] = value; }
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
        public IDictionary<string, object>? GetParams
        {
            get { return this[ReservedProperties.GetParams] as IDictionary<string, object>; }
            set { this[ReservedProperties.GetParams] = value; }
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
        public string? QueryString
        {
            get { return this[ReservedProperties.QueryString] as string; }
            set { this[ReservedProperties.QueryString] = value; }
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
        public IDictionary<string, object>? PostParams
        {
            get { return this[ReservedProperties.PostParams] as IDictionary<string, object>; }
            set { this[ReservedProperties.PostParams] = value; }
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
        public object? PostBody
        {
            get { return this[ReservedProperties.PostBody]; }
            set { this[ReservedProperties.PostBody] = value; }
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
        public string? UserIp
        {
            get { return this[ReservedProperties.UserIp] as string; }
            set { this[ReservedProperties.UserIp] = value; }
        }

    }
}
