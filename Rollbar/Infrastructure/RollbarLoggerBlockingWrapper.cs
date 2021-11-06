namespace Rollbar.Infrastructure
{
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// 
    /// Implements disposable implementation of IRollbar.
    /// 
    /// All the logging methods implemented in synchronous "blocking" fashion.
    /// Hence, the payload is either delivered or a method timed out when
    /// the methods return.
    /// 
    /// </summary>
    internal class RollbarLoggerBlockingWrapper
        : ILogger
        , IDisposable
    {
        private readonly RollbarLogger _asyncLogger;
        private readonly TimeSpan _timeout;

        private void Report(object dataObject, ErrorLevel level, IDictionary<string, object?>? custom = null)
        {
            using (var signal = CreateSignalObject())
            {
                PayloadBundle? payloadBundle = this._asyncLogger.EnqueueData(dataObject, level, custom, this._timeout, signal);

                WaitAndCompleteReport(payloadBundle, signal);
            }
        }

        private static SemaphoreSlim CreateSignalObject()
        {
            SemaphoreSlim signal = 
                new SemaphoreSlim(initialCount: 0, maxCount: 1);

            return signal;
        }

        private void WaitAndCompleteReport(PayloadBundle? payloadBundle, SemaphoreSlim signal)
        {
            if (!signal.Wait(this._timeout))
            {
                const string exceptionMessage = "Posting a payload to the Rollbar API Service timed-out";
                if (payloadBundle?.Exceptions.Count > 0)
                {
                    throw new TimeoutException(exceptionMessage, new AggregateException(payloadBundle.Exceptions));
                }
                else
                {
                    throw new TimeoutException(exceptionMessage);
                }
            }
        }

        public RollbarLoggerBlockingWrapper(RollbarLogger asyncLogger, TimeSpan timeout)
        {
            Assumption.AssertNotNull(asyncLogger, nameof(asyncLogger));

            this._asyncLogger = asyncLogger;
            this._timeout = timeout;
        }

        #region ILogger

        /// <summary>
        /// Returns blocking/synchronous implementation of this ILogger.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>ILogger.</returns>
        public ILogger AsBlockingLogger(TimeSpan timeout)
        {
            return new RollbarLoggerBlockingWrapper(this._asyncLogger, timeout);
        }

        public ILogger Log(Data data)
        {
            this.Report(data, data.Level.HasValue ? data.Level.Value : ErrorLevel.Debug);
            return this;
        }

        /// <summary>
        /// Logs using the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>ILogger.</returns>
        public ILogger Log(ErrorLevel level, object obj, IDictionary<string, object?>? custom = null)
        {
            if (level < this._asyncLogger.Config.RollbarDeveloperOptions.LogLevel)
            {
                // nice shortcut:
                return this;
            }

            this.Report(obj, level, custom);
            return this;
        }

        /// <summary>
        /// Logs the specified object as critical.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>ILogger.</returns>
        public ILogger Critical(object obj, IDictionary<string, object?>? custom = null)
        {
            return this.Log(ErrorLevel.Critical, obj, custom);
        }

        /// <summary>
        /// Logs the specified object as error.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>ILogger.</returns>
        public ILogger Error(object obj, IDictionary<string, object?>? custom = null)
        {
            return this.Log(ErrorLevel.Error, obj, custom);
        }

        /// <summary>
        /// Logs the specified object as warning.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>ILogger.</returns>
        public ILogger Warning(object obj, IDictionary<string, object?>? custom = null)
        {
            return this.Log(ErrorLevel.Warning, obj, custom);
        }

        /// <summary>
        /// Logs the specified object as info.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>ILogger.</returns>
        public ILogger Info(object obj, IDictionary<string, object?>? custom = null)
        {
            return this.Log(ErrorLevel.Info, obj, custom);
        }

        /// <summary>
        /// Logs the specified object as debug.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom data.</param>
        /// <returns>ILogger.</returns>
        public ILogger Debug(object obj, IDictionary<string, object?>? custom = null)
        {
            return this.Log(ErrorLevel.Debug, obj, custom);
        }

        #endregion ILogger

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    this._asyncLogger.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// This code added to correctly implement the disposable pattern.
        /// </remarks>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

    }
}
