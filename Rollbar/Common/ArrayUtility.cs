namespace Rollbar.Common
{
    using System;

    internal static class ArrayUtility
    {
        public static T[] GetEmptyArray<T>()
        {
#if NET452
                return new T[0];
#else
            return Array.Empty<T>();
#endif
        }
    }
}
