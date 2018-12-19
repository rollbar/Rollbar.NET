[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar.Serialization.Json
{
    using Newtonsoft.Json.Linq;
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A utility class aiding in scrubbing Json data fields.
    /// </summary>
    internal static class JsonScrubber
    {
        private const string defaultScrubMask = "***";

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
        /// Creates the Json object.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <returns></returns>
        public static JObject CreateJsonObject(string jsonData)
        {
            Assumption.AssertNotNullOrWhiteSpace(jsonData, nameof(jsonData));

            JObject json = JObject.Parse(jsonData);
            return json;
        }

        /// <summary>
        /// Gets the child Json property by name.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="childPropertyName">Name of the child property.</param>
        /// <returns></returns>
        public static JProperty GetChildPropertyByName(JContainer root, string childPropertyName)
        {
            foreach(var child in root.Children())
            {
                JProperty property = child as JProperty;
                if (property != null && property.Name == childPropertyName)
                {
                    return property;
                }
            }

            return null;
        }

        /// <summary>
        /// Scrubs the Json data string.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <returns>scrubbed Json string.</returns>
        public static string ScrubJson(string jsonData, IEnumerable<string> scrubFields = null)
        {
            if (scrubFields == null || !scrubFields.Any())
            {
                return jsonData;
            }

            JObject json = JObject.Parse(jsonData);

            ScrubJson(json, scrubFields);

            return json.ToString();
        }

        private static void ScrubJson(JToken json, IEnumerable<string> scrubFields)
        {
            JProperty property = json as JProperty;
            if (property != null)
            {
                ScrubJson(property, scrubFields);
                return;
            }

            foreach (var child in json.Children())
            {
                ScrubJson(child, scrubFields);
            }

            return;
        }

        /// <summary>
        /// Scrubs the Json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        public static void ScrubJson(JProperty json, IEnumerable<string> scrubFields)
        {
            if (scrubFields.Contains(json.Name))
            {
                json.Value = defaultScrubMask;
                return;
            }

            JContainer propertyValue = json.Value as JContainer;
            if (propertyValue != null)
            {
                foreach(var child in propertyValue)
                {
                    ScrubJson(child, scrubFields);
                }
            }
        }

    }
}
