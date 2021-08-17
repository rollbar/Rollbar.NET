namespace Rollbar
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// RollbarFactory utility class.
    /// </summary>
    public static class RollbarFactory
    {
        /// <summary>
        /// Creates the proper.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="blockingLoggingTimeout">The blocking logging timeout.</param>
        /// <returns>ILogger.</returns>
        public static ILogger CreateProper(
            IRollbarLoggerConfig? config,
            TimeSpan? blockingLoggingTimeout
            )
        {
            IRollbar rollbar = RollbarFactory.CreateNew(config);

            if(blockingLoggingTimeout.HasValue)
            {
                return rollbar.AsBlockingLogger(blockingLoggingTimeout.Value);
            }
            else
            {
                return rollbar.Logger;
            }
        }

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
        /// <param name="config">The rollbar configuration.</param>
        /// <returns></returns>
        public static IRollbar CreateNew(IRollbarLoggerConfig? config)
        {
            return RollbarFactory.CreateNew(false, config);
        }

        /// <summary>
        /// Creates the new instance of IRollbar.
        /// </summary>
        /// <param name="isSingleton">if set to <c>true</c> [is singleton].</param>
        /// <returns>IRollbar.</returns>
        internal static IRollbar CreateNew(bool isSingleton)
        {
            IRollbarLoggerConfig? config = null;
            if(RollbarInfrastructure.Instance.IsInitialized)
            {
                config = RollbarInfrastructure.Instance.Config.RollbarLoggerConfig;
            }
            return RollbarFactory.CreateNew(false, config);
        }

        /// <summary>
        /// Creates the new instance of IRollbar.
        /// </summary>
        /// <param name="isSingleton">if set to <c>true</c> [is singleton].</param>
        /// <param name="config">The rollbar configuration.</param>
        /// <returns>IRollbar.</returns>
        internal static IRollbar CreateNew(
            bool isSingleton, 
            IRollbarLoggerConfig? config
            )
        {
#if !NETFX_47nOlder
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
            {
                // We are running within Blazor WASM runtime environment that is known to be single threaded in its nature
                // at least at the moment, so no background threads are allowed and our infrastructure depends on the threads.
                // Hence, let's use a single-threaded logger that does not rely on any infrastructure services:
                return new RollbarSingleThreadedLogger(isSingleton, config);
            }
            else
#endif
            if(!RollbarInfrastructure.Instance.IsInitialized)
            {
                // Regardless of actual runtime environment, it looks like our infrastructure was never initialized.
                // Hence, let's use a single-threaded logger that does not rely on any infrastructure services:
                return new RollbarSingleThreadedLogger(isSingleton, config);
            }
            else
            {
                // It is safe to assume we can take advantage of our pre-initialized infrastructure:
                return new RollbarLogger(isSingleton, config);
            }
        }

    }
}
