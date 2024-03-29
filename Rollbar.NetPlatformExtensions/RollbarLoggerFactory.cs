﻿namespace Rollbar.NetPlatformExtensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using Rollbar.Diagnostics;
    using System;

    /// <summary>
    /// Implements Rollbar version of Microsoft.Extensions.Logging.ILoggerFactory.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILoggerFactory" />
    public class RollbarLoggerFactory
            : ILoggerFactory
    {
        private readonly object _providersSync = new();
        private readonly ICollection<ILoggerProvider> _providers = new HashSet<ILoggerProvider>();
        private RollbarLoggerProvider? _loggerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerFactory"/> class.
        /// </summary>
        /// <param name="loggerProviders">The logger providers.</param>
        public RollbarLoggerFactory(
            ICollection<ILoggerProvider>? loggerProviders = null
            )
        {
            if (loggerProviders != null)
            {
                lock(this._providersSync)
                {
                    this._providers = loggerProviders;
                    foreach(var provider in this._providers)
                    {
                        if (provider is RollbarLoggerProvider rollbarProvider)
                        {
                            this._loggerProvider = rollbarProvider;
                            break;
                        }
                    }
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
            ICollection<ILoggerProvider>? loggerProviders = null
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
            ICollection<ILoggerProvider>? loggerProviders = null
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
                        if(this._loggerProvider == null && provider is RollbarLoggerProvider)
                        {
                            this._loggerProvider = provider as RollbarLoggerProvider;
                        }
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
        public ILogger? CreateLogger(string categoryName)
        {
            if(this._loggerProvider != null)
            {
                return this._loggerProvider.CreateLogger(categoryName);
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
                    // Dispose managed state (managed objects).
                    this._loggerProvider?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
