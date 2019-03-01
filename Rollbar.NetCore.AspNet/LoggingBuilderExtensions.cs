#if NETCOREAPP

namespace Microsoft.Extensions.Logging
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Rollbar.NetCore.AspNet;

    /// <summary>
    /// Implements Rollbar extension to the Microsoft.Extensions.Logging.LoggingBuilderExtensions.
    /// </summary>
    public static class LoggingBuilderExtensions
    {
        /// <summary>
        /// Adds the rollbar.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static ILoggingBuilder AddRollbar(
                    this ILoggingBuilder builder
                    )
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, RollbarLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerFactory, RollbarLoggerFactory>());

            return builder;
        }
    }
}

#endif
