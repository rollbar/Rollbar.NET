namespace Rollbar.Infrastructure
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Collections.Generic;

    using Rollbar.Serialization.Json;
    using Rollbar.PayloadScrubbing;
    using Rollbar.DTOs;

    /// <summary>
    /// Class RollbarPayloadScrubber.
    /// </summary>
    internal class RollbarPayloadScrubber
    {
        #region constants

        /// <summary>
        /// The scrub mask
        /// </summary>
        private const string scrubMask = "***";
        /// <summary>
        /// The field path root
        /// </summary>
        private const string fieldPathRoot = @"data.";
        /// <summary>
        /// The HTTP request body path
        /// </summary>
        private const string httpRequestBodyPath = "data.body.request.body.";
        /// <summary>
        /// The HTTP response body path
        /// </summary>
        private const string httpResponseBodyPath = "data.body.response.body.";

        #endregion constants

        #region fields

        /// <summary>
        /// The payload field names
        /// </summary>
        private readonly string[] _payloadFieldNames;
        /// <summary>
        /// The payload field paths
        /// </summary>
        private readonly string[] _payloadFieldPaths;
        /// <summary>
        /// The HTTP request body paths
        /// </summary>
        private readonly string[] _httpRequestBodyPaths;
        /// <summary>
        /// The HTTP response body paths
        /// </summary>
        private readonly string[] _httpResponseBodyPaths;

        #endregion fields

        #region constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarPayloadScrubber"/> class from being created.
        /// </summary>
        private RollbarPayloadScrubber()
        {
            this._payloadFieldNames = new string[0];
            this._payloadFieldPaths = new string[0];
            this._httpRequestBodyPaths = new string[0];
            this._httpResponseBodyPaths = new string[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPayloadScrubber"/> class.
        /// </summary>
        /// <param name="scrubFields">The scrub fields.</param>
        public RollbarPayloadScrubber(IEnumerable<string> scrubFields)
        {
            this._payloadFieldNames = 
                scrubFields.Where(n => !( n.StartsWith(fieldPathRoot) || n.StartsWith(httpRequestBodyPath) || n.StartsWith(httpResponseBodyPath)))
                    .ToArray();
            this._payloadFieldPaths = 
                scrubFields.Where(n => n.StartsWith(fieldPathRoot) && !(n.StartsWith(httpRequestBodyPath) || n.StartsWith(httpResponseBodyPath)))
                    .ToArray();
            this._httpRequestBodyPaths = 
                scrubFields.Where(n => n.StartsWith(httpRequestBodyPath))
                    .ToArray();
            this._httpResponseBodyPaths = 
                scrubFields.Where(n => n.StartsWith(httpResponseBodyPath))
                    .ToArray();

            Debug.Assert(scrubFields.Count() == 
                this._payloadFieldNames.Length + 
                this._payloadFieldPaths.Length + 
                this._httpRequestBodyPaths.Length + 
                this._httpResponseBodyPaths.Length
                );
        }

        #endregion constructors

        #region properties

        /// <summary>
        /// Gets the scrub mask.
        /// </summary>
        /// <value>The scrub mask.</value>
        public string ScrubMask
        {
            get
            {
                return scrubMask;
            }
        }

        /// <summary>
        /// Gets the payload field names.
        /// </summary>
        /// <value>The payload field names.</value>
        public string[] PayloadFieldNames
        {
            get
            {
                return this._payloadFieldNames;
            }
        }

        /// <summary>
        /// Gets the payload field paths.
        /// </summary>
        /// <value>The payload field paths.</value>
        public string[] PayloadFieldPaths
        {
            get
            {
                return this._payloadFieldPaths;
            }
        }

        /// <summary>
        /// Gets the HTTP request body paths.
        /// </summary>
        /// <value>The HTTP request body paths.</value>
        public string[] HttpRequestBodyPaths
        {
            get
            {
                return this._httpRequestBodyPaths;
            }
        }

        /// <summary>
        /// Gets the HTTP response body paths.
        /// </summary>
        /// <value>The HTTP response body paths.</value>
        public string[] HttpResponseBodyPaths
        {
            get
            {
                return this._httpResponseBodyPaths;
            }
        }

        #endregion properties

        #region methods

        /// <summary>
        /// Filters out the critical fields (using case sensitive string comparing).
        /// </summary>
        /// <param name="inputFields">The input fields.</param>
        /// <param name="criticalDataFields">The critical data fields.</param>
        /// <returns>Filtered input fields without the critical ones.</returns>
        public static IEnumerable<string> FilterOutCriticalFields(string[]? inputFields, string[]? criticalDataFields)
        {
            if (criticalDataFields == null)
            {
                return inputFields ?? new string[0];
            }

            List<string>? safeScrubFields = null;

            if (inputFields != null)
            {
                safeScrubFields = new List<string>(inputFields.Length);
                foreach (var field in inputFields)
                {
                    if (!criticalDataFields.Contains(field))
                    {
                        safeScrubFields.Add(field);
                    }
                }
            }

            return safeScrubFields ?? new List<string>(0);
        }

        /// <summary>
        /// Scrubs the payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>System.String.</returns>
        public string ScrubPayload(string payload)
        {
            var jObj = JsonScrubber.CreateJsonObject(payload);
            var dataProperty = JsonScrubber.GetChildPropertyByName(jObj, "data");

            if (this._payloadFieldPaths != null && this._payloadFieldPaths.LongLength > 0)
            {
                JsonScrubber.ScrubJsonFieldsByPaths(jObj, this._payloadFieldPaths, scrubMask);
            }

            if (this._payloadFieldNames != null && this._payloadFieldNames.LongLength > 0)
            {
                JsonScrubber.ScrubJsonFieldsByName(dataProperty, this._payloadFieldNames, scrubMask);
            }

            var scrubbedPayload = jObj.ToString();
            return scrubbedPayload;
        }

        /// <summary>
        /// Scrubs the HTTP messages.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns><c>true</c> if scrubbed successfully, <c>false</c> otherwise.</returns>
        public bool ScrubHttpMessages(PayloadBundle payloadBundle)
        {
            Payload? payload = payloadBundle.GetPayload();
            if (payload == null)
            {
                return true;
            }

            DTOs.Request? request = payload.Data.Request;
            if (request?.PostBody is string requestBody
                && request.Headers != null
                && request.Headers.TryGetValue("Content-Type", out string? requestContentTypeHeader)
                )
            {
                request.PostBody =
                    this.ScrubHttpMessageBodyContentString(
                        requestBody,
                        requestContentTypeHeader,
                        this.ScrubMask,
                        this.PayloadFieldNames,
                        this.HttpRequestBodyPaths);
            }

            DTOs.Response? response = payload.Data.Response;
            if (response?.Body is string responseBody
                && response.Headers != null
                && response.Headers.TryGetValue("Content-Type", out string? responseContentTypeHeader)
                )
            {
                response.Body =
                    this.ScrubHttpMessageBodyContentString(
                        responseBody,
                        responseContentTypeHeader,
                        this.ScrubMask,
                        this.PayloadFieldNames,
                        this.HttpResponseBodyPaths);
            }

            return true;
        }

        /// <summary>
        /// Scrubs the HTTP message body content string.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="contentTypeHeaderValue">The content type header value.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubPaths">The scrub paths.</param>
        /// <returns>System.String.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Globalization",
            "CA1307:Specify StringComparison for clarity",
            Justification = "Mentioned API isn't available in all targeted frameworks."
            )]
        private string? ScrubHttpMessageBodyContentString(
            string body,
            string contentTypeHeaderValue,
            string scrubMask,
            string[] scrubFields,
            string[] scrubPaths
            )
        {
            string contentType = contentTypeHeaderValue.ToLower();
            if (contentType.Contains("json"))
            {
                return new JsonStringScrubber(scrubMask, scrubFields, scrubPaths).Scrub(body);
            }
            else if (contentType.Contains("xml"))
            {
                return new XmlStringScrubber(scrubMask, scrubFields, scrubPaths).Scrub(body);
            }
            else if (contentType.Contains("form-data"))
            {
                return new FormDataStringScrubber(contentTypeHeaderValue, scrubMask, scrubFields, scrubPaths).Scrub(body);
            }
            else
            {
                return new StringScrubber(scrubMask, scrubFields, scrubPaths).Scrub(body);
            }
        }

        #endregion methods
    }
}
