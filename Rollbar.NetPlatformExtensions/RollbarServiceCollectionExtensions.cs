namespace Rollbar.NetPlatformExtensions
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Implements hooks for Rollbar specific services.
    /// </summary>
    public static class RollbarServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Rollbar logger provider.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection Add<TProvider>(this IServiceCollection services)
            where TProvider : RollbarLoggerProvider
        {
            services.AddOptions();
            services.TryAddSingleton<TProvider>();

            return services;
        }

        /// <summary>
        /// Adds the Rollbar logger.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection Add<TLogger,TProvider>(this IServiceCollection services)
            where TLogger : RollbarLogger
            where TProvider : RollbarLoggerProvider
        {
            services.Add<TLogger,TProvider>(loggerOptions =>
            {
                loggerOptions.Filter = (loggerName, loglevel) => loglevel >= LogLevel.Trace;
            });

            return services;
        }

        /// <summary>
        /// Adds the Rollbar logger.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="rollbarOptionsConfigAction">The rollbar options configuration action.</param>
        /// <returns></returns>
        public static IServiceCollection Add<TLogger,TProvider>(this IServiceCollection services, Action<RollbarOptions> rollbarOptionsConfigAction)
            where TLogger : RollbarLogger
            where TProvider : RollbarLoggerProvider
        {
            Assumption.AssertNotNull(rollbarOptionsConfigAction, nameof(rollbarOptionsConfigAction));

            services.Add<TProvider>();
            services.Configure(rollbarOptionsConfigAction);

            return services;
        }

        #region convenience methods for this plug-in specifically

        /// <summary>
        /// Adds the Rollbar logger provider.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddRollbarLoggerProvider(this IServiceCollection services)
        {
            return Add<RollbarLoggerProvider>(services);
        }

        /// <summary>
        /// Adds the Rollbar logger.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddRollbarLogger(this IServiceCollection services)
        {
            return Add<RollbarLogger,RollbarLoggerProvider>(services);
        }

        /// <summary>
        /// Adds the Rollbar logger.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="rollbarOptionsConfigAction">The rollbar options configuration action.</param>
        /// <returns></returns>
        public static IServiceCollection AddRollbarLogger(this IServiceCollection services, Action<RollbarOptions> rollbarOptionsConfigAction)
        {
            return Add<RollbarLogger,RollbarLoggerProvider>(services, rollbarOptionsConfigAction);
        }

        #endregion convenience methods for this plug-in specifically
    }
}
