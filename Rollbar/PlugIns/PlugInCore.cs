namespace Rollbar.PlugIns
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines PlugInCoreBase abstraction.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class PlugInCoreBase
        : IDisposable
    {
        /// <summary>
        /// The Rollbar configuration
        /// </summary>
        protected readonly IRollbarConfig _rollbarConfig;
        
        /// <summary>
        /// The Rollbar logger
        /// </summary>
        private readonly ILogger _rollbarLogger;

        /// <summary>
        /// Prevents a default instance of the <see cref="PlugInCoreBase"/> class from being created.
        /// </summary>
        private PlugInCoreBase()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlugInCoreBase"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="rollbarBlockingTimeout">The rollbar blocking timeout.</param>
        protected PlugInCoreBase(
            IRollbarConfig rollbarConfig,
            TimeSpan? rollbarBlockingTimeout
            )
        {
            this._rollbarConfig = rollbarConfig;

            if (this._rollbarConfig == null)
            {
                IRollbarConfig config = 
                    NetStandard.RollbarConfigUtility.LoadRollbarConfig();

                this._rollbarConfig = config;
            }

            this._rollbarLogger = 
                RollbarFactory.CreateProper(this._rollbarConfig, rollbarBlockingTimeout);
        }

        /// <summary>
        /// The default RollbarLogger's blocking timeout.
        /// </summary>
        public static readonly TimeSpan DefaultRollbarBlockingTimeout = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Creates the configuration.
        /// </summary>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <returns>IRollbarConfig.</returns>
        public static IRollbarConfig CreateConfig(string rollbarAccessToken, string rollbarEnvironment)
        {
            return new RollbarConfig(rollbarAccessToken) { Environment = rollbarEnvironment, };
        }

        /// <summary>
        /// Reports data to Rollbar.
        /// </summary>
        /// <param name="rollbarData">The Rollbar data.</param>
        protected void ReportToRollbar(DTOs.Data rollbarData)
        {
            if (this._rollbarLogger != null)
            {
                ReportToRollbar(this._rollbarLogger, rollbarData);
            }
        }

        /// <summary>
        /// Reports data to Rollbar.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="rollbarData">The Rollbar data.</param>
        private static void ReportToRollbar(ILogger logger, DTOs.Data rollbarData)
        {
            logger.Log(rollbarData);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    (this._rollbarLogger as IDisposable)?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PlugInCore() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

    }

    /// <summary>
    /// Class PlugInCore.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <typeparam name="TPlugInErrorLevel">The type of the t plug in error level.</typeparam>
    /// <typeparam name="TPlugInEventData">The type of the t plug in event data.</typeparam>
    public abstract class PlugInCore<TPlugInErrorLevel, TPlugInEventData>
        : PlugInCoreBase
    {
        /// <summary>
        /// The custom prefix for the Rollbar payload custom fields.
        /// </summary>
        private readonly string _customPrefix;

        /// <summary>
        /// The plug in event data type
        /// </summary>
        private readonly Type _plugInEventDataType;

        /// <summary>
        /// The Rollbar ErrorLevel by plug-in's error level.
        /// Essentially, a one-directional map from plug-in's error level value to Rollbar ErrorLevel.
        /// </summary>
        private readonly IDictionary<TPlugInErrorLevel, ErrorLevel> _rollbarErrorLevelByPlugInErrorLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlugInCore{TPlugInErrorLevel, TPlugInEventData}"/> class.
        /// </summary>
        /// <param name="rollbarErrorLevelByPlugInErrorLevel">
        /// The Rollbar ErrorLevel by plug-in's error level. 
        /// Essentially, a one-directional map from plug-in's error level value to Rollbar ErrorLevel.
        /// </param>
        /// <param name="customPrefix">
        /// The custom prefix for the Rollbar payload custom fields.
        /// </param>
        /// <param name="rollbarConfig">
        /// The Rollbar configuration.
        /// </param>
        /// <param name="rollbarBlockingTimeout">
        /// The RollbarLogger's blocking timeout.
        /// </param>
        protected PlugInCore(
            IDictionary<TPlugInErrorLevel, ErrorLevel> rollbarErrorLevelByPlugInErrorLevel,
            string customPrefix,
            IRollbarConfig rollbarConfig,
            TimeSpan? rollbarBlockingTimeout
            )
            : base(rollbarConfig, rollbarBlockingTimeout)
        {
            this._plugInEventDataType = typeof(TPlugInEventData);
            this._rollbarErrorLevelByPlugInErrorLevel = rollbarErrorLevelByPlugInErrorLevel;
            this._customPrefix = customPrefix;
        }

        /// <summary>
        /// Reports to rollbar.
        /// </summary>
        /// <param name="plugInEventData">The plug-in event data.</param>
        /// <param name="plugInErrorLevel">The plug-in error level.</param>
        public virtual void ReportToRollbar(TPlugInEventData plugInEventData, TPlugInErrorLevel plugInErrorLevel)
        {
            if ((this._plugInEventDataType.IsClass || this._plugInEventDataType.IsInterface)
                && object.Equals(plugInEventData, default(TPlugInEventData)))
            {
                return;
            }

            DTOs.Data data = null;

            try
            {
                data = this.Translate(plugInEventData, plugInErrorLevel);
            }
            catch(Exception ex)
            {
                data = new DTOs.Data(this._rollbarConfig, new DTOs.Body(ex));
            }
            finally
            {
                if (data != null)
                {
                    this.ReportToRollbar(data);
                }
            }
        }

        /// <summary>
        /// Translates the specified plug in event data into a Rollbar.DTOs.Data instance.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <param name="plugInErrorLevel">The plug in error level.</param>
        /// <returns>Rollbar.DTOs.Data instance.</returns>
        protected virtual DTOs.Data Translate(TPlugInEventData plugInEventData, TPlugInErrorLevel plugInErrorLevel)
        {
            ErrorLevel errorLevel = this.Translate(plugInErrorLevel);
            string message = this.ExtractMessage(plugInEventData);
            Exception exception = this.ExtractException(plugInEventData);
            object pluginEventProperties = this.ExtractCustomProperties(plugInEventData);

            DTOs.Body rollbarBody = null;
            if (exception != null)
            {
                rollbarBody = new DTOs.Body(exception);
            }
            else
            {
                rollbarBody = new DTOs.Body(new DTOs.Message(message));
            }

            IDictionary<string, object> custom = null;
            if (pluginEventProperties != null)
            {
                const int customCapacity = 1;
                custom = new Dictionary<string, object>(customCapacity);
                custom[this._customPrefix] = pluginEventProperties;
            }

            DTOs.Data rollbarData = new DTOs.Data(this._rollbarConfig, rollbarBody, custom)
            {
                Level = errorLevel
            };

            return rollbarData;
        }

        /// <summary>
        /// Translates the specified plug-in error level to Rollbar ErrorLevel.
        /// </summary>
        /// <param name="plugInErrorLevel">The plug-in error level.</param>
        /// <returns>ErrorLevel.</returns>
        protected virtual ErrorLevel Translate(TPlugInErrorLevel plugInErrorLevel)
        {
            if (plugInErrorLevel == null)
            {
                return ErrorLevel.Debug;
            }

            if (this._rollbarErrorLevelByPlugInErrorLevel.TryGetValue(plugInErrorLevel, out ErrorLevel rollbarErrorLevel))
            {
                return rollbarErrorLevel;
            }

            return ErrorLevel.Debug;
        }

        /// <summary>
        /// Extracts the message for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>System.String.</returns>
        protected abstract string ExtractMessage(TPlugInEventData plugInEventData);

        /// <summary>
        /// Extracts the exception for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>Exception.</returns>
        protected abstract Exception ExtractException(TPlugInEventData plugInEventData);

        /// <summary>
        /// Extracts the custom properties  for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>Usually, either a data structure or a key-value dictionary returned as a System.Object.</returns>
        protected abstract object ExtractCustomProperties(TPlugInEventData plugInEventData);

    }
}
