namespace Rollbar.Serialization.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class JsonUtil.
    /// </summary>
    public static class JsonUtil
    {
        /// <summary>
        /// Interprets as json object.
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        /// <returns>System.Object.</returns>
        public static object? InterpretAsJsonObject(string? jsonString)
        {
            if (jsonString == null || string.IsNullOrWhiteSpace(jsonString))
            {
                return null; //nothing to
            }

            object? jsonObject = null;
            try
            {
                jsonObject = JsonConvert.DeserializeObject(jsonString);
            }
            catch
            {
                // Nothing to do.
                // We tried our best trying to interpret the body string as JSON
                // but without success.
                // Let's treat it as a string...
            }
            return jsonObject;
        }

        /// <summary>
        /// Determines whether [is valid json] [the specified string value].
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        /// <returns><c>true</c> if the string value is valid JSON; otherwise, <c>false</c>.</returns>
        public static bool IsValidJson(string stringValue)
        {
            return JsonUtil.TryAsValidJson(stringValue, out JToken? token);
        }

        /// <summary>
        /// Tries as valid json.
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        /// <param name="jsonToken">The json token.</param>
        /// <returns><c>true</c> if the string value is a valid JSON string, <c>false</c> otherwise.</returns>
        public static bool TryAsValidJson(string stringValue, out JToken? jsonToken)
        {
            jsonToken = null;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return false;
            }

            var value = stringValue.Trim();

            if ((value.StartsWith("{") && value.EndsWith("}")) || //For object
                (value.StartsWith("[") && value.EndsWith("]"))) //For array
            {
                try
                {
                    jsonToken = JToken.Parse(value);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Serializes as a JSON string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>System.String.</returns>
        public static string SerializeAsJsonString(object? obj)
        {
            string jsonString = JsonConvert.SerializeObject(obj);
            return jsonString;
        }

    }
}
