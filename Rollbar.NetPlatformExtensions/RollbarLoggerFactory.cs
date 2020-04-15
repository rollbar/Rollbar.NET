namespace Rollbar.NetPlatformExtensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Implements Rollbar version of Microsoft.Extensions.Logging.ILoggerFactory.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILoggerFactory" />
    public class RollbarLoggerFactory
            : ILoggerFactory
    {
        private readonly object _providersSync = new object();
        private readonly ICollection<ILoggerProvider> _providers = new HashSet<ILoggerProvider>();
        private readonly RollbarLoggerProvider _loggerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerFactory"/> class.
        /// </summary>
        /// <param name="loggerProviders">The logger providers.</param>
        public RollbarLoggerFactory(
            ICollection<ILoggerProvider> loggerProviders
            )
        {
            if (loggerProviders != null)
            {
                lock(this._providersSync)
                {
                    this._providers = loggerProviders;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerFactory"/> class.
        /// </summary>
        /// <param name="rollbarLoggerProvider">The rollbar logger provider.</param>
        /// <param name="loggerProviders">The logger providers.</param>
        public RollbarLoggerFactory(
            RollbarLoggerProvider rollbarLoggerProvider,
            ICollection<ILoggerProvider> loggerProviders = null
            )
        {
            Assumption.AssertNotNull(rollbarLoggerProvider, nameof(rollbarLoggerProvider));

            this._loggerProvider = rollbarLoggerProvider;

            if (loggerProviders != null)
            {
                lock(this._providersSync)
                {
                    this._providers = loggerProviders;
                }
            }

            this.AddProvider(this._loggerProvider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerFactory"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="rollbarOptions">The rollbar options.</param>
        /// <param name="loggerProviders">The logger providers.</param>
        public RollbarLoggerFactory(
            IConfiguration config,
            IOptions<RollbarOptions> rollbarOptions,
            ICollection<ILoggerProvider> loggerProviders = null
            )
            : this(new RollbarLoggerProvider(config, rollbarOptions), loggerProviders)
        {
        }

        /// <summary>
        /// Adds an <see cref="T:Microsoft.Extensions.Logging.ILoggerProvider" /> to the logging system.
        /// </summary>
        /// <param name="provider">The <see cref="T:Microsoft.Extensions.Logging.ILoggerProvider" />.</param>
        public void AddProvider(ILoggerProvider provider)
        {
            if (provider != null)
            {
                lock(this._providersSync)
                {
                    if (!this._providers.Contains(provider))
                    {
                        this._providers.Add(provider);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Extensions.Logging.ILogger" /> instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>
        /// The <see cref="T:Microsoft.Extensions.Logging.ILogger" />.
        /// </returns>
        public ILogger CreateLogger(string categoryName)
        {
            return this._loggerProvider.CreateLogger(categoryName);
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
                    this._loggerProvider?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }


        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarLoggerFactory() {
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
