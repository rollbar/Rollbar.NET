namespace Rollbar.Common
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    /// <summary>
    /// Class NameValueCollectionExtension.
    /// </summary>
    public static class NameValueCollectionExtension
    {
        /// <summary>
        /// Converts to a string dictionary (where keys are strings and values are strings).
        /// </summary>
        /// <param name="nvc">The NVC.</param>
        /// <returns>IDictionary&lt;System.String, System.String&gt;.</returns>
        public static IDictionary<string, string?> ToStringDictionary(this NameValueCollection nvc)
        {
            if (nvc == null || nvc.Count == 0)
            {
                return new Dictionary<string, string?>(0);
            }

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            string[] keys = nvc.AllKeys.Where(k => k != null).ToArray();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            IDictionary<string, string?> result = new Dictionary<string, string?>(keys.Length);
            foreach (string key in keys)
            {
                result[key] = nvc[key];
            }

            return result;
        }

        /// <summary>
        /// Converts to a compact (excluding null-values) string dictionary (where keys are strings and values are strings).
        /// </summary>
        /// <param name="nvc">The NVC.</param>
        /// <returns>IDictionary&lt;System.String, System.String&gt;.</returns>
        public static IDictionary<string, string> ToCompactStringDictionary(this NameValueCollection nvc)
        {
            var headers = NameValueCollectionExtension.ToStringDictionary(nvc);
            
            var compactedHeaders = new Dictionary<string, string>(headers.Count);
            foreach (var kvp in headers)
            {
                if (kvp.Key != null && kvp.Value != null)
                {
                    compactedHeaders[kvp.Key] = kvp.Value;
                }
            }

            return compactedHeaders;
        }

        /// <summary>
        /// Converts to an object dictionary (where keys are strings and values are objects).
        /// </summary>
        /// <param name="nvc">The NVC.</param>
        /// <returns>IDictionary&lt;System.String, System.Object&gt;.</returns>
        public static IDictionary<string, object?> ToObjectDictionary(this NameValueCollection nvc)
        {
            if (nvc == null || nvc.Count == 0)
            {
                return new Dictionary<string, object?>(0);
            }

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            string[] keys = nvc.AllKeys.Where(k => k != null).ToArray();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            IDictionary<string, object?> result = new Dictionary<string, object?>(keys.Length);
            foreach (string key in keys)
            {
                result[key] = nvc[key];
            }

            return result;
        }

        /// <summary>
        /// Converts to a compact (excluding null-values) object dictionary (where keys are strings and values are objects).
        /// </summary>
        /// <param name="nvc">The NVC.</param>
        /// <returns>IDictionary&lt;System.String, System.Object&gt;.</returns>
        public static IDictionary<string, object> ToCompactObjectDictionary(this NameValueCollection nvc)
        {
            var headers = NameValueCollectionExtension.ToObjectDictionary(nvc);

            var compactedHeaders = new Dictionary<string, object>(headers.Count);
            foreach (var kvp in headers)
            {
                if (kvp.Value != null)
                {
                    compactedHeaders[kvp.Key] = kvp.Value;
                }
            }

            return compactedHeaders;
        }
    }
}
