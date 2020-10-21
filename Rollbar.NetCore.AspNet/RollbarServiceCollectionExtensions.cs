namespace Rollbar.NetCore.AspNet
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Rollbar.NetPlatformExtensions;

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
            return services.Add<RollbarLoggerProvider>();
        }

        /// <summary>
        /// Adds the Rollbar logger.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddRollbarLogger(this IServiceCollection services)
        {
            return services.Add<RollbarLoggerProvider>();
        }

        /// <summary>
        /// Adds the Rollbar logger.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="rollbarOptionsConfigAction">The rollbar options configuration action.</param>
        /// <returns></returns>
        public static IServiceCollection AddRollbarLogger(this IServiceCollection services, Action<RollbarOptions> rollbarOptionsConfigAction)
        {
            services.Add<RollbarLoggerProvider>();
            services.Configure(rollbarOptionsConfigAction);

            return services;
        }

    }
}
