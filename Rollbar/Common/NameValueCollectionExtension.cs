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
        /// Converts to string dictionary (where keys are strings and values are strings).
        /// </summary>
        /// <param name="nvc">The NVC.</param>
        /// <returns>IDictionary&lt;System.String, System.String&gt;.</returns>
        public static IDictionary<string, string?> ToStringDictionary(this NameValueCollection nvc)
        {
            if (nvc == null || nvc.Count == 0)
            {
                return new Dictionary<string, string?>(0);
            }

            return nvc.AllKeys.Where(n => n != null).Cast<string>().ToDictionary(k => k, k => nvc[k]);
        }

        /// <summary>
        /// Converts to object dictionary (where keys are strings and values are objects).
        /// </summary>
        /// <param name="nvc">The NVC.</param>
        /// <returns>IDictionary&lt;System.String, System.Object&gt;.</returns>
        public static IDictionary<string, object?> ToObjectDictionary(this NameValueCollection nvc)
        {
            if (nvc == null || nvc.Count == 0)
            {
                return new Dictionary<string, object?>(0);
            }

            return nvc.AllKeys.Where(n => n != null).Cast<string>().ToDictionary(k => k, k => nvc[k] as object);
        }
    }
}
