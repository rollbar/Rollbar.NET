namespace Rollbar.Infrastructure
{
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using Rollbar.PayloadStore;
    using System.Diagnostics;

    /// <summary>
    /// Implements disposable implementation of IRollbar.
    /// All the logging methods implemented in async "fire-and-forget" fashion.
    /// Hence, the payload is not yet delivered to the Rollbar API service when
    /// the methods return.
    /// </summary>
    /// <seealso cref="Rollbar.IRollbar" />
    internal class RollbarLogger
        : IRollbar
    {

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IRollbarLoggerConfig _config;
        /// <summary>
        /// The payload queue
        /// </summary>
        private readonly PayloadQueue _payloadQueue;

        /// <summary>
        /// Occurs when a Rollbar internal event happens.
        /// </summary>
        public event EventHandler<RollbarEventArgs>? InternalEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLogger" /> class.
        /// </summary>
        internal RollbarLogger()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLogger" /> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        internal RollbarLogger(IRollbarLoggerConfig? rollbarConfig)
        {
            Assumption.AssertTrue(RollbarInfrastructure.Instance.IsInitialized, nameof(RollbarInfrastructure.Instance.IsInitialized));

            if (RollbarTelemetryCollector.Instance != null 
                && !RollbarTelemetryCollector.Instance.IsAutocollecting
                )
            {
                RollbarTelemetryCollector.Instance.StartAutocollection();
            }

            if (rollbarConfig != null)
            {
                ValidateConfiguration(rollbarConfig);
                this._config = new RollbarLoggerConfig(this).Reconfigure(rollbarConfig);
            }
            else
            {
                this._config = new RollbarLoggerConfig(this);
            }

            // let's init proper Rollbar client:
            var rollbarClient = new RollbarClient(this);

            // let's init the corresponding queue and register it:
            this._payloadQueue = new PayloadQueue(this, rollbarClient);
            RollbarQueueController.Instance?.Register(this._payloadQueue);
        }

        /// <summary>
        /// Gets the queue.
        /// </summary>
        /// <value>The queue.</value>
        internal PayloadQueue Queue
        {
            get { return this._payloadQueue; }
        }

        #region IRollbar

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger => this;

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IRollbarLoggerConfig Config
        {
            get { return this._config; }
        }

        /// <summary>
        /// Configures the using specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>IRollbar.</returns>
        public IRollbar Configure(IRollbarLoggerConfig settings)
        {
            ValidateConfiguration(settings);

            this._config.Reconfigure(settings);

            return this;
        }

        /// <summary>
        /// Configures using the specified access token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns>IRollbar.</returns>
        public IRollbar Configure(string accessToken)
        {
            return this.Configure(new RollbarLoggerConfig(accessToken));
        }

        #endregion IRollbar

        #region ILogger

        /// <summary>
        /// Returns blocking/synchronous implementation of this ILogger.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>Blocking (fully synchronous) instance of an ILogger.
        /// It either completes logging calls within the specified timeout
        /// or throws a TimeoutException.</returns>
        public ILogger AsBlockingLogger(TimeSpan timeout)
        {
            return new RollbarLoggerBlockingWrapper(this, timeout);
        }

        /// <summary>
        /// Logs the specified rollbar data.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        /// <returns>ILogger.</returns>
        public ILogger Log(DTOs.Data rollbarData)
        {
            return this.Enqueue(rollbarData, rollbarData.Level ?? ErrorLevel.Debug, null);
        }

        /// <summary>
        /// Logs using the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Log(ErrorLevel level, object obj, IDictionary<string, object?>? custom = null)
        {
            return this.Enqueue(obj, level, custom);
        }


        /// <summary>
        /// Logs the specified object as critical.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Critical(object obj, IDictionary<string, object?>? custom = null)
        {
            return this.Enqueue(obj, ErrorLevel.Critical, custom);
        }

        /// <summary>
        /// Logs the specified object as error.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Error(object obj, IDictionary<string, object?>? custom = null)
        {
            return this.Enqueue(obj, ErrorLevel.Error, custom);
        }

        /// <summary>
        /// Logs the specified object as warning.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Warning(object obj, IDictionary<string, object?>? custom = null)
        {
            return this.Enqueue(obj, ErrorLevel.Warning, custom);
        }

        /// <summary>
        /// Logs the specified object as info.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Info(object obj, IDictionary<string, object?>? custom = null)
        {
            return this.Enqueue(obj, ErrorLevel.Info, custom);
        }

        /// <summary>
        /// Logs the specified object as debug.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Debug(object obj, IDictionary<string, object?>? custom = null)
        {
            return this.Enqueue(obj, ErrorLevel.Debug, custom);
        }

        #endregion ILogger

        #region IRollbar explicitly

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        IRollbarLoggerConfig IRollbar.Config { get { return this.Config; } }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        ILogger IRollbar.Logger { get { return this; } }

        /// <summary>
        /// Configures the using specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>IRollbar.</returns>
        IRollbar IRollbar.Configure(IRollbarLoggerConfig settings)
        {
            return this.Configure(settings);
        }

        /// <summary>
        /// Occurs when a Rollbar internal event happens.
        /// </summary>
        event EventHandler<RollbarEventArgs> IRollbar.InternalEvent
        {
            add
            {
                this.InternalEvent += value;
            }

            remove
            {
                this.InternalEvent -= value;
            }
        }

        #endregion IRollbar explicitly

        #region ILogger explicitly

        /// <summary>
        /// Returns blocking/synchronous implementation of this ILogger.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>Blocking (fully synchronous) instance of an ILogger.
        /// It either completes logging calls within the specified timeout
        /// or throws a TimeoutException.</returns>
        ILogger ILogger.AsBlockingLogger(TimeSpan timeout)
        {
            return this.AsBlockingLogger(timeout);
        }

        /// <summary>
        /// Logs the specified rollbar data.
        /// </summary>
        /// <param name="data">The rollbar data.</param>
        /// <returns>ILogger.</returns>
        ILogger ILogger.Log(Data data)
        {
            return this.Log(data);
        }

        /// <summary>
        /// Logs using the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Log(ErrorLevel level, object obj, IDictionary<string, object?>? custom)
        {
            return this.Log(level, obj, custom);
        }


        /// <summary>
        /// Logs the specified object as critical.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Critical(object obj, IDictionary<string, object?>? custom)
        {
            return this.Critical(obj, custom);
        }

        /// <summary>
        /// Logs the specified object as error.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Error(object obj, IDictionary<string, object?>? custom)
        {
            return this.Error(obj, custom);
        }

        /// <summary>
        /// Logs the specified object as warning.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Warning(object obj, IDictionary<string, object?>? custom)
        {
            return this.Warning(obj, custom);
        }

        /// <summary>
        /// Logs the specified object as info.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Info(object obj, IDictionary<string, object?>? custom)
        {
            return this.Info(obj, custom);
        }

        /// <summary>
        /// Logs the specified object as debug.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Debug(object obj, IDictionary<string, object?>? custom)
        {
            return this.Debug(obj, custom);
        }

        #endregion ILogger explicitly 

        /// <summary>
        /// Enqueues the specified data object.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="level">The level.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="signal">The signal.</param>
        /// <returns>ILogger.</returns>
        internal ILogger Enqueue(
            object dataObject,
            ErrorLevel level,
            IDictionary<string, object?>? custom,
            TimeSpan? timeout = null,
            SemaphoreSlim? signal = null
            )
        {
            this.EnqueueData(dataObject, level, custom, timeout, signal);
            return this;
        }

        /// <summary>
        /// Enqueues the data.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="level">The level.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="signal">The signal.</param>
        /// <returns>PayloadBundle.</returns>
        internal PayloadBundle? EnqueueData(
            object dataObject,
            ErrorLevel level,
            IDictionary<string, object?>? custom,
            TimeSpan? timeout = null,
            SemaphoreSlim? signal = null
            )
        {
            // here is the last chance to decide if we need to actually send this payload
            // based on the current config settings and rate-limit conditions:
            if (string.IsNullOrWhiteSpace(this._config.RollbarDestinationOptions.AccessToken)
                || !this._config.RollbarDeveloperOptions.Enabled 
                || (level < this._config.RollbarDeveloperOptions.LogLevel)
                || ((this._payloadQueue.AccessTokenQueuesMetadata != null) && this._payloadQueue.AccessTokenQueuesMetadata.IsTransmissionSuspended)
                )
            {
                // nice shortcut:
                return null;
            }

            if (this._config.RollbarDeveloperOptions.RethrowExceptionsAfterReporting)
            {
                System.Exception? exception = dataObject as System.Exception;
                if (exception == null 
                    && dataObject is Data data 
                    && data.Body != null
                    )
                {
                    exception = data.Body.OriginalException;
                }

                if (exception != null)
                {
                    try
                    {
                        // Here we need to create another logger instance with similar config but configured not to re-throw.
                        // This would prevent infinite recursive calls (in case if we used this instance or any re-throwing instance).
                        // Because we will be re-throwing the exception after reporting, let's report it fully-synchronously.
                        // This logic is on a heavy side. But, fortunately, RethrowExceptionsAfterReporting is intended to be
                        // a development time option:
                        var config = new RollbarLoggerConfig();
                        config.Reconfigure(this._config);
                        config.RollbarDeveloperOptions.RethrowExceptionsAfterReporting = false;
                        using var rollbar = RollbarFactory.CreateNew(config);
                        rollbar.AsBlockingLogger(TimeSpan.FromSeconds(1)).Log(level, dataObject, custom);
                    }
                    catch
                    {
                        // In case there was a TimeoutException (or any un-expected exception),
                        // there is nothing we can do here.
                        // We tried our best...
                    }
                    finally
                    {
                        if (exception is AggregateException aggregateException)
                        {
                            exception = aggregateException.Flatten();
                        }
                        ExceptionDispatchInfo.Capture(exception).Throw();
                    }

                    return null;
                }
            }


            PayloadBundle? payloadBundle = null;
            try
            {
                payloadBundle = CreatePayloadBundle(dataObject, level, custom, timeout, signal);
            }
            catch (System.Exception exception)
            {
                RollbarErrorUtility.Report(
                    this,
                    dataObject,
                    InternalRollbarError.BundlingError,
                    null,
                    exception,
                    payloadBundle
                    );
                return null;
            }

            try
            {
                this._payloadQueue.Enqueue(payloadBundle);
            }
            catch (System.Exception exception)
            {
                RollbarErrorUtility.Report(
                    this,
                    dataObject,
                    InternalRollbarError.EnqueuingError,
                    null,
                    exception,
                    payloadBundle
                    );
            }

            return payloadBundle;
        }

        /// <summary>
        /// Creates the payload bundle.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="level">The level.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="signal">The signal.</param>
        /// <returns>PayloadBundle.</returns>
        private PayloadBundle CreatePayloadBundle(
            object dataObject,
            ErrorLevel level,
            IDictionary<string, object?>? custom,
            TimeSpan? timeout = null,
            SemaphoreSlim? signal = null
            )
        {
            DateTime? timeoutAt = null;
            if (timeout.HasValue)
            {
                timeoutAt = DateTime.Now.Add(timeout.Value);
            }

            switch (dataObject)
            {
                case IRollbarPackage package:
                    if (package.MustApplySynchronously)
                    {
                        package.PackageAsRollbarData();
                    }
                    return new PayloadBundle(this, package, level, custom, timeoutAt, signal);
                default:
                    return new PayloadBundle(this, dataObject, level, custom, timeoutAt, signal);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:RollbarEvent" /> event.
        /// </summary>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        internal virtual void OnRollbarEvent(RollbarEventArgs e)
        {
            InternalEvent?.Invoke(this, e);
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        private static void ValidateConfiguration(IRollbarLoggerConfig rollbarConfig)
        {
            switch (rollbarConfig)
            {
                case IValidatable v:
                    Validator.Validate(v, InternalRollbarError.ConfigurationError, "Failed to configure using invalid configuration prototype!");
                    break;
            }
        }


        #region IDisposable Support

        private bool disposedValue;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // RollbarLogger type supports both paradigms: singleton-like (via RollbarLocator) and
                    // multiple disposable instances (via RollbarFactory).
                    // Here we want to make sure that the singleton instance is never disposed:
                    System.Diagnostics.Trace.Assert(
                        this != RollbarLocator.RollbarInstance.Logger, 
                        $"Do not attempt disposing the singleton {this.GetType().FullName}"
                        );

                    // dispose managed state (managed objects)
                    this._payloadQueue.Release();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
