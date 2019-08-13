using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar.PayloadScrubbing
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using Rollbar.Serialization.Json;

    internal class RollbarPayloadScrubber
    {
        private const string fieldPathRoot = @"data.";
        private const string httpRequestBodyPath = "data.body.request.body.";
        private const string httpResponseBodyPath = "data.body.response.body.";

        private readonly string[] _payloadFieldNames;
        private readonly string[] _payloadFieldPaths;
        private readonly string[] _httpRequestBodyPaths;
        private readonly string[] _httpResponseBodyPaths;

        private RollbarPayloadScrubber()
        {

        }

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

        public string ScrubPayload(string payload)
        {
            var jObj = JsonScrubber.CreateJsonObject(payload);
            var dataProperty = JsonScrubber.GetChildPropertyByName(jObj, "data");

            if (this._httpRequestBodyPaths != null && this._httpRequestBodyPaths.LongLength > 0)
            {
                this.ScrubHttpMessageBody(jObj, httpRequestBodyPath, this._httpRequestBodyPaths);
            }

            if (this._httpResponseBodyPaths != null && this._httpResponseBodyPaths.LongLength > 0)
            {
                this.ScrubHttpMessageBody(jObj, httpResponseBodyPath, this._httpResponseBodyPaths);
            }

            if (this._payloadFieldPaths != null && this._payloadFieldPaths.LongLength > 0)
            {
                JsonScrubber.ScrubJsonFieldsByPaths(jObj, this._payloadFieldPaths);
            }

            if (this._payloadFieldNames != null && this._payloadFieldNames.LongLength > 0)
            {
                JsonScrubber.ScrubJsonFieldsByName(dataProperty, this._payloadFieldNames);
            }

            var scrubbedPayload = jObj.ToString();
            return scrubbedPayload;
        }

        private void ScrubHttpMessageBody(JObject payloadJson, string pathToBody, IEnumerable<string> bodyFieldPaths)
        {
            // Let's try treating http message bodies as "native JSON" sub-structure, first:
            //==============================================================================

            bool bodyIsNativeJson = false;

            foreach (var bodyFieldPath in bodyFieldPaths)
            {
                JToken jToken = payloadJson.SelectToken(bodyFieldPath);
                JProperty jProperty = jToken?.Parent as JProperty;
                if (jProperty != null)
                {
                    jProperty.Replace(new JProperty(jProperty.Name, "***"));
                    bodyIsNativeJson = true;
                }
            }

            if (bodyIsNativeJson)
            {
                return;
            }


            // Let's try treating http message bodies as a string:
            //====================================================

            string[] bodyPathsToScrub = 
                bodyFieldPaths.Select(i => i.Replace(pathToBody, string.Empty)).ToArray();

            this.ScrubHttpMessageBody(payloadJson.SelectToken(pathToBody.TrimEnd('.')), bodyPathsToScrub);
        }

        private void ScrubHttpMessageBody(JToken httpBodyToken, IEnumerable<string> bodyFieldPaths)
        {
            if (httpBodyToken == null || bodyFieldPaths == null)
            {
                return;
            }

            string bodyString = httpBodyToken.Value<string>();
            if (string.IsNullOrWhiteSpace(bodyString))
            {
                return;
            }

            // Let's try scrubbing as a JSON string:
            if (JsonUtil.TryAsValidJson(bodyString, out JToken jsonToken))
            {
                JObject jsonObj = jsonToken as JObject;
                if (jsonObj != null)
                {
                    JsonScrubber.ScrubJsonFieldsByPaths(jsonObj, bodyFieldPaths);
                    string scrubbedJsonString = JsonConvert.SerializeObject(jsonObj);
                    JProperty jProperty = httpBodyToken.Parent as JProperty;
                    if (jProperty != null)
                    {
                        jProperty.Replace(new JProperty(jProperty.Name, scrubbedJsonString));
                    }
                    return;
                }
            }

            // Let's try scrubbing as an XML string:
            //TODO: implement...

            // Let's try scrubbing as a "key=value" pairs string:
            //TODO: implement...

        }
    }
}
