#if NETCOREAPP

namespace Microsoft.AspNetCore.Builder
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rollbar.AspNetCore;

    /// <summary>
    /// Implements Rollbar middleware extensions to IApplicationBuilder.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables the rollbar middleware.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseRollbarMiddleware(
                    this IApplicationBuilder builder
                    )
        {
            var factory = builder.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var provider = builder.ApplicationServices.GetRequiredService<RollbarLoggerProvider>();
            factory.AddProvider(provider);

            return builder.UseMiddleware<RollbarMiddleware>();
        }
    }
}

#endif