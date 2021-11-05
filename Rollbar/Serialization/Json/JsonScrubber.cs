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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "Not applicable here.")]
        public static JProperty? GetChildPropertyByName(JContainer root, string childPropertyName)
        {
            foreach(var child in root.Children())
            {
                if (child is JProperty property && property.Name == childPropertyName)
                {
                    return property;
                }
            }

            return null;
        }

        /// <summary>
        /// Scrubs the json fields by their names.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <returns>System.String.</returns>
        public static string ScrubJsonFieldsByName(string jsonData, IEnumerable<string>? scrubFields, string scrubMask)
        {
            if (scrubFields == null || !scrubFields.Any())
            {
                return jsonData;
            }

            JObject json = JObject.Parse(jsonData);

            ScrubJsonFieldsByName(json, scrubFields, scrubMask);

            return json.ToString();
        }

        /// <summary>
        /// Scrubs the json fields by their names.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        public static void ScrubJsonFieldsByName(JToken? json, IEnumerable<string> scrubFields, string scrubMask)
        {
            if(json == null)
            {
                return;
            }

            if(json is JProperty property)
            {
                ScrubJsonFieldsByName(property, scrubFields, scrubMask);
                return;
            }

            foreach (var child in json.Children())
            {
                ScrubJsonFieldsByName(child, scrubFields, scrubMask);
            }
        }

        /// <summary>
        /// Scrubs the json fields by their name.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        public static void ScrubJsonFieldsByName(JProperty? json, IEnumerable<string> scrubFields, string scrubMask)
        {
            if(json == null)
            {
                return;
            }

            var fields = scrubFields as string[] ?? scrubFields.ToArray();
            if (fields.Contains(json.Name))
            {
                json.Value = scrubMask;
                return;
            }

            if (json.Value is JContainer propertyValue)
            {
                foreach (var child in propertyValue)
                {
                    ScrubJsonFieldsByName(child, fields, scrubMask);
                }
            }
        }

        /// <summary>
        /// Scrubs the json fields by their full names/paths.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="scrubFieldsPaths">The scrub fields paths.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <returns>System.String.</returns>
        public static string ScrubJsonFieldsByPaths(string jsonData, IEnumerable<string>? scrubFieldsPaths, string scrubMask)
        {
            if(scrubFieldsPaths == null)
            {
                return jsonData;
            }

            var fieldsPaths = scrubFieldsPaths as string[] ?? scrubFieldsPaths.ToArray();

            if (fieldsPaths.LongLength == 0)
            {
                return jsonData;
            }

            JObject json = JObject.Parse(jsonData);

            foreach (var path in fieldsPaths)
            {
                JsonScrubber.ScrubJsonPath(json, path, scrubMask);
            }

            return json.ToString();
        }

        /// <summary>
        /// Scrubs the json fields by their full names/paths.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="scrubFieldsPaths">The scrub fields paths.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        public static void ScrubJsonFieldsByPaths(JObject jsonData, IEnumerable<string> scrubFieldsPaths, string scrubMask)
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
                JsonScrubber.ScrubJsonPath(jsonData, path, scrubMask);
            }
        }

        /// <summary>
        /// Scrubs the json fields by their full names/paths.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="scrubFieldsPaths">The scrub fields paths.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        public static void ScrubJsonFieldsByPaths(JProperty jsonData, IEnumerable<string> scrubFieldsPaths, string scrubMask)
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
                JsonScrubber.ScrubJsonPath(jsonData, path, scrubMask);
            }
        }

        /// <summary>
        /// Scrubs the json path.
        /// </summary>
        /// <param name="jsonProperty">The json property.</param>
        /// <param name="scrubPath">The scrub path.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        public static void ScrubJsonPath(JProperty jsonProperty, string scrubPath, string scrubMask)
        {
            if (jsonProperty == null)
            {
                return;
            }

            var jProperty = jsonProperty.SelectToken(scrubPath) as JProperty;
            jProperty?.Replace(new JProperty(jProperty.Name, scrubMask));
        }

        /// <summary>
        /// Scrubs the json path.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="scrubPath">The scrub path.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        public static void ScrubJsonPath(JObject jsonData, string scrubPath, string scrubMask)
        {
            if (jsonData == null)
            {
                return;
            }

            JToken? jToken = JsonScrubber.FindJsonTokenSafelyUsingPath(jsonData, scrubPath);            
            if (jToken != null)
            {
                var jProperty = jToken.Parent as JProperty;
                if (jProperty != null)
                {
                    jProperty.Replace(new JProperty(jProperty.Name, scrubMask));
                    return;
                }
            }

            //to deal with the possible dotted data element name we need to perform some acrobatics here:
            const int startingIndex = 0;
            int indexLimit = scrubPath.LastIndexOf('.');
            int dotIndex = scrubPath.IndexOf('.', startingIndex);
            while (dotIndex > 0 && dotIndex < indexLimit)
            {
                string dottedFieldPath = scrubPath.Substring(0, dotIndex);
                string dottedFieldName = scrubPath.Substring(dotIndex + 1);
                jToken = JsonScrubber.FindJsonTokenSafelyUsingPath(jsonData, dottedFieldPath);
                jToken = jToken?[dottedFieldName];
                if (jToken != null)
                {
                    //we found the dotted data element name, let's mask its value and return:
                    var jProperty = jToken.Parent as JProperty;
                    if (jProperty != null)
                    {
                        jProperty.Replace(new JProperty(jProperty.Name, scrubMask));
                        return;
                    }
                }
                dotIndex = scrubPath.IndexOf('.', dotIndex + 1);
            }
        }

        private static JToken? FindJsonTokenSafelyUsingPath(JObject jsonData, string tokenPath)
        {
            JToken? jToken = null;
            try
            {
                jToken = jsonData.SelectToken(tokenPath);
            }
            catch
            {
                //that is expected in some scenarios, let's just make sure jToken is still null:
                jToken = null;
            }

            return jToken;
        }

    }
}
