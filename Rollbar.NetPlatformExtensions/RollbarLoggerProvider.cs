﻿namespace Rollbar.NetPlatformExtensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Rollbar.Diagnostics;
    using Rollbar.NetCore;
    using Rollbar.Telemetry;
    using System.Collections.Concurrent;
    using System.Linq;

    /// <summary>
    /// Implements Rollbar version of Microsoft.Extensions.Logging.ILoggerProvider.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILoggerProvider" />
    [ProviderAlias("Rollbar")]
    public class RollbarLoggerProvider
            : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, RollbarLogger> _loggers =
            new ConcurrentDictionary<string, RollbarLogger>();

        private readonly IRollbarConfig _rollbarConfig;

        private readonly RollbarOptions _rollbarOptions;

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
        public RollbarLoggerProvider(
                    IConfiguration configuration,
                    IOptions<RollbarOptions> options
                    )
        {
            Assumption.AssertNotNull(configuration, nameof(configuration));
            Assumption.AssertNotNull(options, nameof(options));

            RollbarConfigurationUtil.DeduceRollbarTelemetryConfig(configuration);
            TelemetryCollector.Instance.StartAutocollection();
            this._rollbarOptions = options.Value;
            this._rollbarConfig = RollbarConfigurationUtil.DeduceRollbarConfig(configuration);

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
                    foreach(var item in this._loggers?.Values)
                    {
                        item?.Dispose();
                    }
                    this._loggers?.Clear();
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