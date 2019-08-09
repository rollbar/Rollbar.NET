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
        /// <summary>
        /// The default scrub mask
        /// </summary>
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
        /// <returns>JObject.</returns>
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
        /// <returns>JProperty.</returns>
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
        public static string ScrubJsonFieldsByName(string jsonData, IEnumerable<string> scrubFields = null)
        {
            if (scrubFields == null || !scrubFields.Any())
            {
                return jsonData;
            }

            JObject json = JObject.Parse(jsonData);

            ScrubJsonFieldsByName(json, scrubFields);

            return json.ToString();
        }

        /// <summary>
        /// Scrubs the name of the json fields by.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        public static void ScrubJsonFieldsByName(JToken json, IEnumerable<string> scrubFields)
        {
            JProperty property = json as JProperty;
            if (property != null)
            {
                ScrubJsonFieldsByName(property, scrubFields);
                return;
            }

            foreach (var child in json.Children())
            {
                ScrubJsonFieldsByName(child, scrubFields);
            }
        }

        /// <summary>
        /// Scrubs the Json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        public static void ScrubJsonFieldsByName(JProperty json, IEnumerable<string> scrubFields)
        {
            var fields = scrubFields as string[] ?? scrubFields.ToArray();
            if (fields.Contains(json.Name))
            {
                json.Value = defaultScrubMask;
                return;
            }

            JContainer propertyValue = json.Value as JContainer;
            if (propertyValue != null)
            {
                foreach (var child in propertyValue)
                {
                    ScrubJsonFieldsByName(child, fields);
                }
            }

        }

        /// <summary>
        /// Scrubs the json fields by paths.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="scrubFieldsPaths">The scrub fields paths.</param>
        /// <returns>System.String.</returns>
        public static string ScrubJsonFieldsByPaths(string jsonData, IEnumerable<string> scrubFieldsPaths = null)
        {
            var fieldsPaths = scrubFieldsPaths as string[] ?? scrubFieldsPaths.ToArray();

            if (fieldsPaths.LongLength == 0)
            {
                return jsonData;
            }

            JObject json = JObject.Parse(jsonData);

            foreach (var path in fieldsPaths)
            {
                JsonScrubber.ScrubJsonPath(json, path);
            }

            return json.ToString();
        }

        /// <summary>
        /// Scrubs the json fields by paths.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="scrubFieldsPaths">The scrub fields paths.</param>
        public static void ScrubJsonFieldsByPaths(JObject jsonData, IEnumerable<string> scrubFieldsPaths = null)
        {
            if (jsonData == null)
            {
                return;
            }

            var fieldsPaths = scrubFieldsPaths as string[] ?? scrubFieldsPaths.ToArray();

            if (fieldsPaths.LongLength == 0)
            {
                return;
            }

            foreach (var path in fieldsPaths)
            {
                JsonScrubber.ScrubJsonPath(jsonData, path);
            }
        }

        /// <summary>
        /// Scrubs the json fields by paths.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="scrubFieldsPaths">The scrub fields paths.</param>
        public static void ScrubJsonFieldsByPaths(JProperty jsonData, IEnumerable<string> scrubFieldsPaths = null)
        {
            if (jsonData == null)
            {
                return;
            }

            var fieldsPaths = scrubFieldsPaths as string[] ?? scrubFieldsPaths.ToArray();

            if (fieldsPaths.LongLength == 0)
            {
                return;
            }

            foreach (var path in fieldsPaths)
            {
                JsonScrubber.ScrubJsonPath(jsonData, path);
            }
        }

        /// <summary>
        /// Scrubs the json path.
        /// </summary>
        /// <param name="jsonProperty">The json property.</param>
        /// <param name="scrubPath">The scrub path.</param>
        public static void ScrubJsonPath(JProperty jsonProperty, string scrubPath = null)
        {
            if (jsonProperty == null)
            {
                return;
            }

            var jProperty = jsonProperty.SelectToken(scrubPath) as JProperty;
            jProperty?.Replace(new JProperty(jProperty.Name, defaultScrubMask));
        }

        /// <summary>
        /// Scrubs the json path.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="scrubPath">The scrub path.</param>
        public static void ScrubJsonPath(JObject jsonData, string scrubPath = null)
        {
            if (jsonData == null)
            {
                return;
            }

            object obj = jsonData.SelectToken(scrubPath);
            var jProperty = jsonData.SelectToken(scrubPath)?.Parent as JProperty;
            jProperty?.Replace(new JProperty(jProperty.Name, defaultScrubMask));
        }

    }
}
