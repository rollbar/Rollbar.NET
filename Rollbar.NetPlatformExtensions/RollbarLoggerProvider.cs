﻿namespace Rollbar.NetPlatformExtensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Rollbar.AppSettings.Json;
    using Rollbar.Infrastructure;

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
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new();

        /// <summary>
        /// The rollbar options
        /// </summary>
        protected readonly RollbarOptions? _rollbarOptions;

        /// <summary>
        /// The rollbar infrastructure configuration
        /// </summary>
        protected readonly IRollbarInfrastructureConfig? _rollbarInfrastructureConfig;

        /// <summary>
        /// The rollbar logger configuration
        /// </summary>
        protected readonly IRollbarLoggerConfig? _rollbarLoggerConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerProvider"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="options">The options.</param>
        public RollbarLoggerProvider(IRollbarLoggerConfig config, IOptions<RollbarOptions>? options = null)
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
                    IConfiguration? configuration,
                    IOptions<RollbarOptions>? options = default
                    )
        {
            if(configuration != null)
            {
                if (!RollbarInfrastructure.Instance.IsInitialized)
                {
                    this._rollbarInfrastructureConfig = RollbarConfigurationUtil.DeduceRollbarConfig(configuration);
                    RollbarInfrastructure.Instance.Init(this._rollbarInfrastructureConfig);
                }
                this._rollbarInfrastructureConfig = RollbarInfrastructure.Instance.Config;
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
                throw new RollbarException(
                    InternalRollbarError.InfrastructureError, 
                    $"{this.GetType().FullName}: Failed to create ILogger!");
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
                    foreach(var item in this._loggers.Values)
                    {
                        (item as IDisposable)?.Dispose();
                    }
                    this._loggers.Clear();
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
