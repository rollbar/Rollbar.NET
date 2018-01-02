[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using System;

    /// <summary>
    /// RollbarFactory utility class.
    /// </summary>
    public static class RollbarFactory
    {
        /// <summary>
        /// Creates the new instance of IRollbar.
        /// </summary>
        /// <returns></returns>
        public static IRollbar CreateNew()
        {
            return RollbarFactory.CreateNew(false);
        }

        /// <summary>
        /// Creates the new instance of IRollbar.
        /// </summary>
        /// <param name="isSingleton">if set to <c>true</c> [is singleton].</param>
        /// <returns></returns>
        internal static IRollbar CreateNew(bool isSingleton)
        {
            return new RollbarLogger(isSingleton);
        }
    }
}
