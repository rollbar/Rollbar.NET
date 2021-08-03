namespace Rollbar.NetPlatformExtensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Rollbar.AppSettings.Json;

    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Implements Rollbar version of Microsoft.Extensions.Logging.ILoggerProvider.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILoggerProvider" />
    [ProviderAlias("Rollbar")]
    public class RollbarLoggerProvider
            : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers =
            new ConcurrentDictionary<string, ILogger>();

        /// <summary>
        /// The rollbar options
        /// </summary>
        protected readonly RollbarOptions _rollbarOptions;

        /// <summary>
        /// The rollbar configuration
        /// </summary>
        protected readonly IRollbarInfrastructureConfig _rollbarInfrastructureConfig;


        protected readonly IRollbarLoggerConfig _rollbarLoggerConfig;

        public RollbarLoggerProvider(IRollbarLoggerConfig config, IOptions<RollbarOptions> options = null)
        {
            this._rollbarLoggerConfig = config;

            if(options != null)
            {
                this._rollbarOptions = options.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerProvider"/> class.
        /// </summary>
        public RollbarLoggerProvider()
            : this(null as IConfiguration, null)
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
            if(configuration!= null)
            {
                this._rollbarInfrastructureConfig = RollbarConfigurationUtil.DeduceRollbarConfig(configuration);
                if(RollbarInfrastructure.Instance.IsInitialized)
                {
                    RollbarInfrastructure.Instance.Config.Reconfigure(this._rollbarInfrastructureConfig);
                }
                else
                {
                    RollbarInfrastructure.Instance.Init(this._rollbarInfrastructureConfig);
                }
                RollbarConfigurationUtil.DeduceRollbarTelemetryConfig(configuration);
                _ = Rollbar.RollbarInfrastructure.Instance?.TelemetryCollector?.StartAutocollection();
            }

            if(options != null)
            {
                this._rollbarOptions = options.Value;
            }
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

        /// <summary>
        /// Creates the logger implementation.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>ILogger.</returns>
        protected virtual ILogger CreateLoggerImplementation(string name)
        {
            if(this._rollbarInfrastructureConfig != null
                && this._rollbarInfrastructureConfig.RollbarLoggerConfig != null
                )
            {
                return new RollbarLogger(
                    name
                    , this._rollbarInfrastructureConfig.RollbarLoggerConfig
                    , this._rollbarOptions
                    );
            }
            else if(this._rollbarLoggerConfig != null)
            {
                return new RollbarLogger(
                    name
                    , this._rollbarLoggerConfig
                    , this._rollbarOptions
                    );
            }
            else
            {
                return null;
            }
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
                        (item as IDisposable)?.Dispose();
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
