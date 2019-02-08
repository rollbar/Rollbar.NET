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
            if (stringDictionary == null || stringDictionary.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            return stringDictionary.Keys.Where(n => n != null).ToDictionary(k => k, k => stringDictionary[k] as object);
        }

    }
}
