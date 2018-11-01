namespace Rollbar.Common
{
    using System;
    using System.Linq;

    /// <summary>
    /// Class EnumUtil.
    /// </summary>
    public static class EnumUtil
    {
        /// <summary>
        /// Gets all values.
        /// </summary>
        /// <typeparam name="TEnum">The type of the Enum.</typeparam>
        /// <returns>TEnum[].</returns>
        public static TEnum[] GetAllValues<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
        }
    }
}
