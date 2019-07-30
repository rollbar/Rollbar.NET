namespace Rollbar.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json;


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
        public static object InterpretAsJsonObject(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return null; //nothing to
            }

            object jsonObject = null;
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
    }
}
