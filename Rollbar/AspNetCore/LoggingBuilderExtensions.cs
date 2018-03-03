#if NETCOREAPP

namespace Microsoft.Extensions.Logging
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Rollbar.AspNetCore;

    /// <summary>
    /// Implements Rollbar extension to the Microsoft.Extensions.Logging.LoggingBuilderExtensions.
    /// </summary>
    public static class LoggingBuilderExtensions
    {
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
