#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Rollbar.Diagnostics;
    using System.Collections.Concurrent;

    /// <summary>
    /// Implements Rollbar version of Microsoft.Extensions.Logging.ILoggerProvider.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILoggerProvider" />
    [ProviderAlias("Rollbar")]
    internal class RollbarLoggerProvider
            : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, RollbarLogger> _loggers =
            new ConcurrentDictionary<string, RollbarLogger>();

        private readonly IRollbarConfig _rollbarConfig = null;

        private readonly RollbarOptions _rollbarOptions = null;

        private readonly IHttpContextAccessor _httpContextAccessor = null;

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
                    , IOptions<RollbarOptions> options
                    , IHttpContextAccessor httpContextAccessor
                    )
        {
            Assumption.AssertNotNull(configuration, nameof(configuration));
            Assumption.AssertNotNull(options, nameof(options));

            this._rollbarOptions = options.Value;
            this._rollbarConfig = RollbarConfigurationUtil.DeduceRollbarConfig(configuration);
            this._httpContextAccessor = httpContextAccessor;

            Assumption.AssertNotNull(this._rollbarConfig, nameof(this._rollbarConfig));
            Assumption.AssertNotNullOrWhiteSpace(this._rollbarConfig.AccessToken, nameof(this._rollbarConfig.AccessToken));
        }

        /// <summary>
        /// Creates the logger.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        private RollbarLogger CreateLoggerImplementation(string name)
        {
            return new RollbarLogger(
                name
                , this._rollbarConfig
                , this._rollbarOptions
                , this._httpContextAccessor
                );
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }


        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarLoggerProvider() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
#pragma warning disable CA1063 // Implement IDisposable Correctly
        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}

#endif