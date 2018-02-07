namespace Rollbar.Common
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A utility type aiding in string manipulations.
    /// </summary>
    public static class StringUtility
    {
        /// <summary>
        /// Combines the specified substrings using the specified separator.
        /// </summary>
        /// <param name="substrings">The substrings.</param>
        /// <param name="substringSeparator">The substring separator.</param>
        /// <returns></returns>
        public static string Combine(IEnumerable<string> substrings, string substringSeparator)
        {
            if (substrings == null)
            {
                return string.Empty;
            }

            return string.Join(substringSeparator, substrings);
        }
    }
}
