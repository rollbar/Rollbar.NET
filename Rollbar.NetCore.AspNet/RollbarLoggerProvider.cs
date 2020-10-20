namespace Rollbar.NetCore.AspNet
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Implements Rollbar version of Microsoft.Extensions.Logging.ILoggerProvider.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILoggerProvider" />
    [ProviderAlias("Rollbar")]
    public class RollbarLoggerProvider
        : NetPlatformExtensions.RollbarLoggerProvider
        , ILoggerProvider
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarLoggerProvider" /> class from being created.
        /// </summary>
        private RollbarLoggerProvider()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerProvider" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="options">The options.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public RollbarLoggerProvider(
                    IConfiguration configuration
                    , IOptions<NetPlatformExtensions.RollbarOptions> options
                    , IHttpContextAccessor httpContextAccessor
                    )
            : base(configuration, options)
        {
            Assumption.AssertNotNull(configuration, nameof(configuration));
            Assumption.AssertNotNull(options, nameof(options));
            Assumption.AssertNotNull(this._rollbarConfig, nameof(this._rollbarConfig));
            Assumption.AssertNotNullOrWhiteSpace(this._rollbarConfig.AccessToken, nameof(this._rollbarConfig.AccessToken));

            this._httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Creates the logger implementation.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>ILogger.</returns>
        protected override ILogger CreateLoggerImplementation(string name)
        {
            return new RollbarLogger(
                name
                ,this._rollbarConfig
                ,this._rollbarOptions
                ,this._httpContextAccessor
                );
        }
    }
}
