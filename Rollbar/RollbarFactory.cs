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
            return RollbarFactory.CreateNew(null);
        }

        /// <summary>
        /// Creates the new instance of IRollbar.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <returns></returns>
        public static IRollbar CreateNew(IRollbarConfig rollbarConfig)
        {
            return RollbarFactory.CreateNew(false, rollbarConfig);
        }

        /// <summary>
        /// Creates the new instance of IRollbar.
        /// </summary>
        /// <param name="isSingleton">if set to <c>true</c> [is singleton].</param>
        /// <returns>IRollbar.</returns>
        internal static IRollbar CreateNew(bool isSingleton)
        {
            return new RollbarLogger(isSingleton, null);
        }

        /// <summary>
        /// Creates the new instance of IRollbar.
        /// </summary>
        /// <param name="isSingleton">if set to <c>true</c> [is singleton].</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <returns>IRollbar.</returns>
        internal static IRollbar CreateNew(bool isSingleton, IRollbarConfig rollbarConfig)
        {
            return new RollbarLogger(isSingleton, rollbarConfig);
        }

        /// <summary>
        /// Creates the proper.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The rollbar blocking logging timeout.</param>
        public static ILogger CreateProper(
            IRollbarConfig rollbarConfig,
            TimeSpan? rollbarBlockingLoggingTimeout
            )
        {
            IRollbar rollbar = RollbarFactory.CreateNew().Configure(rollbarConfig);

            if (rollbarBlockingLoggingTimeout.HasValue)
            {
                return rollbar.AsBlockingLogger(rollbarBlockingLoggingTimeout.Value);
            }
            else
            {
                return rollbar.Logger;
            }
        }

    }
}
