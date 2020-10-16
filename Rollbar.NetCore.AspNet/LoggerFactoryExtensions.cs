namespace Microsoft.Extensions.Logging
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Rollbar.NetCore.AspNet;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Implements Rollbar extension to the Microsoft.Extensions.Logging.ILoggerFactory.
    /// </summary>
    public static class LoggerFactoryExtensions
    {
        /// <summary>
        /// Adds the Rollbar logging client.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="rollbarOptions">The rollbar options.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <returns></returns>
        public static ILoggerFactory AddRollbar(
                    this ILoggerFactory factory
                    , IConfiguration configuration
                    , IOptions<Rollbar.NetPlatformExtensions.RollbarOptions> rollbarOptions
                    , IHttpContextAccessor httpContextAccessor
                    )
        {
            Assumption.AssertNotNull(configuration, nameof(configuration));

            factory.AddProvider(
                new RollbarLoggerProvider(configuration, rollbarOptions, httpContextAccessor)
                );

            return factory;
        }
    }
}
