namespace Rollbar.NetCore.AspNet
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
        public static IServiceCollection AddRollbarLoggerProvider(this IServiceCollection services)
        {
            services.AddOptions();
            services.TryAddSingleton<RollbarLoggerProvider>();

            return services;
        }

        /// <summary>
        /// Adds the Rollbar logger.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddRollbarLogger(this IServiceCollection services)
        {
            services.AddRollbarLogger(loggerOptions =>
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
        public static IServiceCollection AddRollbarLogger(this IServiceCollection services, Action<NetPlatformExtensions.RollbarOptions> rollbarOptionsConfigAction)
        {
            Assumption.AssertNotNull(rollbarOptionsConfigAction, nameof(rollbarOptionsConfigAction));

            services.AddRollbarLoggerProvider();
            services.Configure(rollbarOptionsConfigAction);

            return services;
        }

    }
}
