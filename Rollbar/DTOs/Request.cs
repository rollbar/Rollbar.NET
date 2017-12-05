namespace Rollbar.DTOs
{
    using System.Collections.Generic;

    /// <summary>
    /// Models Rollbar Request DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.ExtendableDtoBase" />
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
        public Request(IDictionary<string, object> arbitraryKeyValuePairs) 
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
            public const string Url = "url";
            /// <summary>
            /// The method
            /// </summary>
            public const string Method = "method";
            /// <summary>
            /// The headers
            /// </summary>
            public const string Headers = "headers";
            /// <summary>
            /// The parameters
            /// </summary>
            public const string Params = "params";
            /// <summary>
            /// The get-parameters
            /// </summary>
            public const string GetParams = "get_params";
            /// <summary>
            /// The query string
            /// </summary>
            public const string QueryString = "query_string";
            /// <summary>
            /// The post-parameters
            /// </summary>
            public const string PostParams = "post_params";
            /// <summary>
            /// The post body
            /// </summary>
            public const string PostBody = "post_body";
            /// <summary>
            /// The user IP
            /// </summary>
            public const string UserIp = "user_ip";
        }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
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
        public string Method
        {
            get { return this._keyedValues[ReservedProperties.Method] as string; }
            set { this._keyedValues[ReservedProperties.Method] = value; }
        }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
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
        public IDictionary<string, object> Params
        {
            get { return this._keyedValues[ReservedProperties.Params] as IDictionary<string, object>; }
            set { this._keyedValues[ReservedProperties.Params] = value; }
        }

        /// <summary>
        /// Gets or sets the get-parameters.
        /// </summary>
        /// <value>
        /// The get-parameters.
        /// </value>
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
        public string UserIp
        {
            get { return this._keyedValues[ReservedProperties.UserIp] as string; }
            set { this._keyedValues[ReservedProperties.UserIp] = value; }
        }

    }
}
