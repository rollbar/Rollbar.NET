namespace Rollbar.PayloadScrubbing
{
    using System;
    using System.Diagnostics;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using Rollbar.Serialization.Json;

    /// <summary>
    /// Class RollbarPayloadScrubber.
    /// </summary>
    internal class RollbarPayloadScrubber
    {
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
        /// Gets the scrub mask.
        /// </summary>
        /// <value>The scrub mask.</value>
        public string ScrubMask
        {
            get { return scrubMask; }
        }

        /// <summary>
        /// Gets the payload field names.
        /// </summary>
        /// <value>The payload field names.</value>
        public string[] PayloadFieldNames
        {
            get { return this._payloadFieldNames; }
        }

        /// <summary>
        /// Gets the payload field paths.
        /// </summary>
        /// <value>The payload field paths.</value>
        public string[] PayloadFieldPaths
        {
            get { return this._payloadFieldPaths; }
        }

        /// <summary>
        /// Gets the HTTP request body paths.
        /// </summary>
        /// <value>The HTTP request body paths.</value>
        public string[] HttpRequestBodyPaths
        {
            get { return this._httpRequestBodyPaths; }
        }

        /// <summary>
        /// Gets the HTTP response body paths.
        /// </summary>
        /// <value>The HTTP response body paths.</value>
        public string[] HttpResponseBodyPaths
        {
            get { return this._httpResponseBodyPaths; }
        }

    }
}
