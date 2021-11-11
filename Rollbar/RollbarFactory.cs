namespace Rollbar
{
    using System;
    using System.Runtime.InteropServices;

    using Rollbar.Infrastructure;

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
            IRollbarLoggerConfig? config = null,
            TimeSpan? blockingLoggingTimeout = null
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
        /// <param name="config">The rollbar configuration.</param>
        /// <returns>IRollbar.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3973:A conditionally executed single line should be denoted by indentation", Justification = "We have conditional compilation here. Keeps better code structure.")]
        public static IRollbar CreateNew(
            IRollbarLoggerConfig? config = null
            )
        {
            if(config == null && RollbarInfrastructure.Instance.IsInitialized)
            {
                config = RollbarInfrastructure.Instance.Config.RollbarLoggerConfig;
            }

#if !NETFX_47nOlder
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
            {
                // We are running within Blazor WASM runtime environment that is known to be single threaded in its nature
                // at least at the moment, so no background threads are allowed and our infrastructure depends on the threads.
                // Hence, let's use a single-threaded logger that does not rely on any infrastructure services:
                return new RollbarSingleThreadedLogger(config);
            }
            else
#endif
            if(!RollbarInfrastructure.Instance.IsInitialized)
            {
                // Regardless of actual runtime environment, it looks like our infrastructure was never initialized.
                // Hence, let's use a single-threaded logger that does not rely on any infrastructure services:
                return new RollbarSingleThreadedLogger(config);
            }
            else
            {
                // It is safe to assume we can take advantage of our pre-initialized infrastructure:
                return new RollbarLogger(config);
            }
        }

    }
}
