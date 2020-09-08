[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using Rollbar.Telemetry;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using Rollbar.PayloadStore;
    using System.IO;


    /// <summary>
    /// Implements disposable implementation of IRollbar.
    /// All the logging methods implemented in async "fire-and-forget" fashion.
    /// Hence, the payload is not yet delivered to the Rollbar API service when
    /// the methods return.
    /// </summary>
    /// <seealso cref="Rollbar.IRollbar" />
    /// <seealso cref="System.IDisposable" />
    internal class RollbarLogger
        : IRollbar
        , IDisposable
    {

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IRollbarConfig _config;
        /// <summary>
        /// The payload queue
        /// </summary>
        private readonly PayloadQueue _payloadQueue;

        /// <summary>
        /// Occurs when a Rollbar internal event happens.
        /// </summary>
        public event EventHandler<RollbarEventArgs> InternalEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLogger" /> class.
        /// </summary>
        /// <param name="isSingleton">if set to <c>true</c> [is singleton].</param>
        internal RollbarLogger(bool isSingleton)
            : this(isSingleton, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLogger" /> class.
        /// </summary>
        /// <param name="isSingleton">if set to <c>true</c> [is singleton].</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        internal RollbarLogger(bool isSingleton, IRollbarConfig rollbarConfig)
        {
            if (!TelemetryCollector.Instance.IsAutocollecting)
            {
                TelemetryCollector.Instance.StartAutocollection();
            }

            this.IsSingleton = isSingleton;

            if (rollbarConfig != null)
            {
                ValidateConfiguration(rollbarConfig);
                this._config = new RollbarConfig(this).Reconfigure(rollbarConfig);
            }
            else
            {
                this._config = new RollbarConfig(this);
            }

            // let's figure out where to keep the local payloads store:
            PayloadStoreConstants.DefaultRollbarStoreDbFile = ((RollbarConfig)this._config).GetLocalPayloadStoreFullPathName();

            // let's init proper Rollbar client:
            var rollbarClient = new RollbarClient(this);

            // let's init the corresponding queue and register it:
            this._payloadQueue = new PayloadQueue(this, rollbarClient);
            RollbarQueueController.Instance.Register(this._payloadQueue);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is singleton.
        /// </summary>
        /// <value><c>true</c> if this instance is singleton; otherwise, <c>false</c>.</value>
        internal bool IsSingleton { get; private set; }

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
        public IRollbarConfig Config
        {
            get { return this._config; }
        }

        /// <summary>
        /// Configures the using specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>IRollbar.</returns>
        public IRollbar Configure(IRollbarConfig settings)
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
            return this.Configure(new RollbarConfig(accessToken));
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
        /// Logs using the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Log(ErrorLevel level, object obj)
        {
            return this.Log(level, obj, null);
        }

        /// <summary>
        /// Logs the specified object as critical.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Critical(object obj)
        {
            return this.Critical(obj, null);
        }

        /// <summary>
        /// Logs the specified object as error.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Error(object obj)
        {
            return this.Error(obj, null);
        }

        /// <summary>
        /// Logs the specified object as warning.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Warning(object obj)
        {
            return this.Warning(obj, null);
        }

        /// <summary>
        /// Logs the specified object as info.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Info(object obj)
        {
            return this.Info(obj, null);
        }

        /// <summary>
        /// Logs the specified object as debug.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Debug(object obj)
        {
            return this.Debug(obj, null);
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
        public ILogger Log(ErrorLevel level, object obj, IDictionary<string, object> custom)
        {
            return this.Enqueue(obj, level, custom);
        }


        /// <summary>
        /// Logs the specified object as critical.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Critical(object obj, IDictionary<string, object> custom)
        {
            return this.Enqueue(obj, ErrorLevel.Critical, custom);
        }

        /// <summary>
        /// Logs the specified object as error.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Error(object obj, IDictionary<string, object> custom)
        {
            return this.Enqueue(obj, ErrorLevel.Error, custom);
        }

        /// <summary>
        /// Logs the specified object as warning.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Warning(object obj, IDictionary<string, object> custom)
        {
            return this.Enqueue(obj, ErrorLevel.Warning, custom);
        }

        /// <summary>
        /// Logs the specified object as info.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Info(object obj, IDictionary<string, object> custom)
        {
            return this.Enqueue(obj, ErrorLevel.Info, custom);
        }

        /// <summary>
        /// Logs the specified object as debug.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        public ILogger Debug(object obj, IDictionary<string, object> custom)
        {
            return this.Enqueue(obj, ErrorLevel.Debug, custom);
        }

        #endregion ILogger

        #region IRollbar explicitly

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        IRollbarConfig IRollbar.Config { get { return this.Config; } }

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
        IRollbar IRollbar.Configure(IRollbarConfig settings)
        {
            return this.Configure(settings);
        }

        /// <summary>
        /// Configures using the specified access token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns>IRollbar.</returns>
        IRollbar IRollbar.Configure(string accessToken)
        {
            return this.Configure(accessToken);
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
        /// <param name="rollbarData">The rollbar data.</param>
        /// <returns>ILogger.</returns>
        ILogger ILogger.Log(Data rollbarData)
        {
            return this.Log(rollbarData);
        }

        /// <summary>
        /// Logs using the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Log(ErrorLevel level, object obj)
        {
            return this.Log(level, obj);
        }

        /// <summary>
        /// Logs the specified object as critical.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Critical(object obj)
        {
            return this.Critical(obj);
        }

        /// <summary>
        /// Logs the specified object as error.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Error(object obj)
        {
            return this.Error(obj);
        }

        /// <summary>
        /// Logs the specified object as warning.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Warning(object obj)
        {
            return this.Warning(obj);
        }

        /// <summary>
        /// Logs the specified object as info.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Info(object obj)
        {
            return this.Info(obj);
        }

        /// <summary>
        /// Logs the specified object as debug.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Debug(object obj)
        {
            return this.Debug(obj);
        }

        /// <summary>
        /// Logs using the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Log(ErrorLevel level, object obj, IDictionary<string, object> custom)
        {
            return this.Log(level, obj, custom);
        }


        /// <summary>
        /// Logs the specified object as critical.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Critical(object obj, IDictionary<string, object> custom)
        {
            return this.Critical(obj, custom);
        }

        /// <summary>
        /// Logs the specified object as error.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Error(object obj, IDictionary<string, object> custom)
        {
            return this.Error(obj, custom);
        }

        /// <summary>
        /// Logs the specified object as warning.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Warning(object obj, IDictionary<string, object> custom)
        {
            return this.Warning(obj, custom);
        }

        /// <summary>
        /// Logs the specified object as info.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Info(object obj, IDictionary<string, object> custom)
        {
            return this.Info(obj, custom);
        }

        /// <summary>
        /// Logs the specified object as debug.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>Instance of the same ILogger that was used for this call.</returns>
        ILogger ILogger.Debug(object obj, IDictionary<string, object> custom)
        {
            return this.Debug(obj, custom);
        }

        #endregion ILogger explicitly 

        #region IDisposable explicitly

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        #endregion IDisposable explicitly

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
            IDictionary<string, object> custom,
            TimeSpan? timeout = null,
            SemaphoreSlim signal = null
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
        internal PayloadBundle EnqueueData(
            object dataObject,
            ErrorLevel level,
            IDictionary<string, object> custom,
            TimeSpan? timeout = null,
            SemaphoreSlim signal = null
            )
        {
            // here is the last chance to decide if we need to actually send this payload
            // based on the current config settings and rate-limit conditions:
            if (string.IsNullOrWhiteSpace(this._config.AccessToken)
                || this._config.Enabled == false
                || (this._config.LogLevel.HasValue && level < this._config.LogLevel.Value)
                || ((this._payloadQueue.AccessTokenQueuesMetadata != null) && this._payloadQueue.AccessTokenQueuesMetadata.IsTransmissionSuspended)
                )
            {
                // nice shortcut:
                return null;
            }

            if (this._config.RethrowExceptionsAfterReporting)
            {
                System.Exception exception = dataObject as System.Exception;
                if (exception == null)
                {
                    if (dataObject is Data data && data.Body != null)
                    {
                        exception = data.Body.OriginalException;
                    }
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
                        var config = new RollbarConfig();
                        config.Reconfigure(this._config);
                        config.RethrowExceptionsAfterReporting = false;
                        using var rollbar = RollbarFactory.CreateNew(config);
                        rollbar.AsBlockingLogger(TimeSpan.FromSeconds(1)).Log(level, dataObject, custom);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
                    {
                        // In case there was a TimeoutException (or any un-expected exception),
                        // there is nothing we can do here.
                        // We tried our best...
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                    finally
                    {
                        if (exception is AggregateException aggregateException)
                        {
                            exception = aggregateException.Flatten();
                            ExceptionDispatchInfo.Capture(
                                exception.InnerException).Throw();
                        }
                        else
                        {
                            ExceptionDispatchInfo.Capture(exception).Throw();
                        }
                    }

                    return null;
                }
            }


            PayloadBundle payloadBundle = null;
            try
            {
                payloadBundle = CreatePayloadBundle(dataObject, level, custom, timeout, signal);
            }
#pragma warning disable CA1031 // Do not catch general exception types
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
#pragma warning restore CA1031 // Do not catch general exception types

            try
            {
                this._payloadQueue.Enqueue(payloadBundle);
            }
#pragma warning disable CA1031 // Do not catch general exception types
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
#pragma warning restore CA1031 // Do not catch general exception types

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
            IDictionary<string, object> custom,
            TimeSpan? timeout = null,
            SemaphoreSlim signal = null
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
        private void ValidateConfiguration(IRollbarConfig rollbarConfig)
        {
            switch (rollbarConfig)
            {
                case IValidatable v:
                    var failedValidationRules = v.Validate();
                    if (failedValidationRules.Count > 0)
                    {
                        var exception =
                            new RollbarException(
                                InternalRollbarError.ConfigurationError,
                                "Failed to configure using invalid configuration prototype!"
                                );
                        exception.Data[nameof(failedValidationRules)] = failedValidationRules.ToArray();

                        throw exception;
                    }
                    break;
            }
        }

        #region IDisposable Support

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool _disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this._payloadQueue.Release();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarLogger() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>This code added to correctly implement the disposable pattern.</remarks>
        public void Dispose()
        {
            // RollbarLogger type supports both paradigms: singleton-like (via RollbarLocator) and
            // multiple disposable instances (via RollbarFactory).
            // Here we want to make sure that the singleton instance is never disposed:
            Assumption.AssertTrue(!this.IsSingleton, nameof(this.IsSingleton));

            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

    }
}
