namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class DictionaryExtension.
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// Converts to object dictionary (where keys are strings and values are objects).
        /// </summary>
        /// <param name="stringDictionary">The string dictionary (where keys are strings and values are strings).</param>
        /// <returns>IDictionary&lt;System.String, System.Object&gt;.</returns>
        public static IDictionary<string, object> ToObjectDictionary(this IDictionary<string, string> stringDictionary)
        {
            int capacity = stringDictionary?.Count ?? 0;
            var objectDictionary = new Dictionary<string, object>(capacity);

            foreach(var key in stringDictionary.Keys.Where(k => k != null))
            {
                objectDictionary[key] = stringDictionary[key];
            }

            return objectDictionary;
        }

    }
}
