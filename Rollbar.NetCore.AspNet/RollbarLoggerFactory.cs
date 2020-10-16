namespace Rollbar.NetCore.AspNet
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Implements Rollbar version of Microsoft.Extensions.Logging.ILoggerFactory.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILoggerFactory" />
    public class RollbarLoggerFactory
            : NetPlatformExtensions.RollbarLoggerFactory
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerFactory" /> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="rollbarOptions">The rollbar options.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public RollbarLoggerFactory(
                    IConfiguration config
                    , IOptions<NetPlatformExtensions.RollbarOptions> rollbarOptions
                    , IHttpContextAccessor httpContextAccessor
                    )
            : base(new RollbarLoggerProvider(config, rollbarOptions, httpContextAccessor))
        {
        }

    }
}
