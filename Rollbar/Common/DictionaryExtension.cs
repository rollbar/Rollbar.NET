namespace Rollbar.Common
{
    using System.Collections.Generic;
    using System.Linq;

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
            if (stringDictionary == null)
            {
                return new Dictionary<string, object>(0);
            }

            int capacity = stringDictionary.Count;
            var objectDictionary = new Dictionary<string, object>(capacity);
            if (capacity == 0)
            {
                return objectDictionary;
            }

            foreach(var key in stringDictionary.Keys.Where(k => k != null))
            {
                objectDictionary[key] = stringDictionary[key];
            }
            return objectDictionary;
        }

    }
}
