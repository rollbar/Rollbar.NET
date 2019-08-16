﻿[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPayloadScrubber"/> class.
        /// </summary>
        /// <param name="scrubFields">The scrub fields.</param>
        public RollbarPayloadScrubber(IEnumerable<string> scrubFields)
        {
            this._payloadFieldNames = 
                scrubFields.Where(n => !n.Contains('.'))
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
        public static IEnumerable<string> FilterOutCriticalFields(string[] inputFields, string[] criticalDataFields)
        {
            if (criticalDataFields == null)
            {
                return inputFields;
            }

            List<string> safeScrubFields = null;

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

            return safeScrubFields;
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

            //if (this._httpRequestBodyPaths != null && this._httpRequestBodyPaths.LongLength > 0)
            //{
            //    this.ScrubHttpMessageBody(jObj, httpRequestBodyPath, this._httpRequestBodyPaths);
            //}

            //if (this._httpResponseBodyPaths != null && this._httpResponseBodyPaths.LongLength > 0)
            //{
            //    this.ScrubHttpMessageBody(jObj, httpResponseBodyPath, this._httpResponseBodyPaths);
            //}

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

        //private void ScrubHttpMessageBody(JObject payloadJson, string pathToBody, IEnumerable<string> bodyFieldPaths)
        //{
        //    // Let's try treating http message bodies as "native JSON" sub-structure, first:
        //    //==============================================================================

        //    bool bodyIsNativeJson = false;

        //    foreach (var bodyFieldPath in bodyFieldPaths)
        //    {
        //        JToken jToken = payloadJson.SelectToken(bodyFieldPath);
        //        if (jToken?.Parent is JProperty jProperty)
        //        {
        //            jProperty.Replace(new JProperty(jProperty.Name, scrubMask));
        //            bodyIsNativeJson = true;
        //        }
        //    }

        //    if (bodyIsNativeJson)
        //    {
        //        return;
        //    }


        //    // Let's try treating http message bodies as a string:
        //    //====================================================

        //    string[] bodyPathsToScrub = 
        //        bodyFieldPaths.Select(i => i.Replace(pathToBody, string.Empty)).ToArray();

        //    this.ScrubHttpMessageBody(payloadJson.SelectToken(pathToBody.TrimEnd('.')), bodyPathsToScrub);
        //}

        //private void ScrubHttpMessageBody(JToken httpBodyToken, IEnumerable<string> bodyFieldPaths)
        //{
        //    if (httpBodyToken == null || bodyFieldPaths == null)
        //    {
        //        return;
        //    }

        //    string bodyString = httpBodyToken.Value<string>();
        //    if (string.IsNullOrWhiteSpace(bodyString))
        //    {
        //        return;
        //    }

        //    // Let's try scrubbing as a JSON string:
        //    if (JsonUtil.TryAsValidJson(bodyString, out JToken jsonToken))
        //    {
        //        if (jsonToken is JObject jsonObj)
        //        {
        //            JsonScrubber.ScrubJsonFieldsByPaths(jsonObj, bodyFieldPaths, scrubMask);
        //            string scrubbedJsonString = JsonConvert.SerializeObject(jsonObj);
        //            if (httpBodyToken.Parent is JProperty jProperty)
        //            {
        //                jProperty.Replace(new JProperty(jProperty.Name, scrubbedJsonString));
        //            }
        //            return;
        //        }
        //    }

        //    // Let's try scrubbing as an XML string:
        //    //TODO: implement...

        //    // Let's try scrubbing as a "key=value" pairs string:
        //    //TODO: implement...

        //}

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
