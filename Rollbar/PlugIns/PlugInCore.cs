namespace Rollbar.PlugIns
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Rollbar.Infrastructure;
    using Rollbar.NetStandard;

    /// <summary>
    /// Defines PlugInCoreBase abstraction.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class PlugInCoreBase
        : IDisposable
    {

        /// <summary>
        /// The rollbar
        /// </summary>
        protected readonly IRollbar? rollbar;

        /// <summary>
        /// The Rollbar logger
        /// </summary>
        private readonly ILogger? rollbarLogger;

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
            IRollbarInfrastructureConfig? rollbarConfig,
            TimeSpan? rollbarBlockingTimeout
            )
        {
            if (rollbarConfig == null)
            {
                var configLoader = new RollbarConfigurationLoader();
                rollbarConfig = configLoader.LoadRollbarConfig();
            }

            if(rollbarConfig == null)
            {
                throw new RollbarException(InternalRollbarError.ConfigurationError, $"{this.GetType().FullName}: Failed to load Rollbar configuration!");
            }

            // first, initialize the infrastructure:
            if(!RollbarInfrastructure.Instance.IsInitialized)
            {
                RollbarInfrastructure.Instance.Init(rollbarConfig);
            }

            // create proper IRollbar instance:
            this.rollbar = RollbarFactory.CreateNew(rollbarConfig.RollbarLoggerConfig);

            // create proper RollbarLogger instance:
            if (rollbarBlockingTimeout.HasValue)
            {
                this.rollbarLogger = this.rollbar.AsBlockingLogger(rollbarBlockingTimeout.Value);
            }
            else
            {
                this.rollbarLogger = this.rollbar.Logger;
            }
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
        public static IRollbarInfrastructureConfig CreateConfig(string rollbarAccessToken, string rollbarEnvironment)
        {
            IRollbarInfrastructureConfig config = new RollbarInfrastructureConfig(rollbarAccessToken, rollbarEnvironment);

            return config;
        }

        /// <summary>
        /// Gets the rollbar configuration.
        /// </summary>
        /// <value>The rollbar configuration.</value>
        public IRollbarLoggerConfig? RollbarConfig
        {
            get { return this.rollbar?.Config; }
        }

        /// <summary>
        /// Reports data to Rollbar.
        /// </summary>
        /// <param name="rollbarData">The Rollbar data.</param>
        protected void ReportToRollbar(DTOs.Data rollbarData)
        {
            if (this.rollbarLogger != null)
            {
                ReportToRollbar(this.rollbarLogger, rollbarData);
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
                    // ispose managed state (managed objects).
                    (this.rollbarLogger as IDisposable)?.Dispose();
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

        #endregion IDisposable Support

    }

    /// <summary>
    /// Class PlugInCore.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <typeparam name="TPlugInErrorLevel">The type of the t plug in error level.</typeparam>
    /// <typeparam name="TPlugInEventData">The type of the t plug in event data.</typeparam>
    public abstract class PlugInCore<TPlugInErrorLevel, TPlugInEventData>
        : PlugInCoreBase
        where TPlugInErrorLevel : notnull
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
            IRollbarInfrastructureConfig? rollbarConfig,
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

            DTOs.Data? data = null;

            try
            {
                data = this.Translate(plugInEventData, plugInErrorLevel);
            }
            catch(Exception ex)
            {
                data = new DTOs.Data(this.rollbar?.Config, new DTOs.Body(ex));
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
            object? pluginEventProperties = this.ExtractCustomProperties(plugInEventData);

            DTOs.Body? rollbarBody = null;
            if (exception != null)
            {
                rollbarBody = new DTOs.Body(exception);
            }
            else
            {
                rollbarBody = new DTOs.Body(new DTOs.Message(message));
            }

            IDictionary<string, object?>? custom = null;
            if (pluginEventProperties != null)
            {
                const int customCapacity = 1;
                custom = new Dictionary<string, object?>(customCapacity);
                custom[this._customPrefix] = pluginEventProperties;
            }

            DTOs.Data rollbarData = new DTOs.Data(this.rollbar?.Config, rollbarBody, custom)
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
        protected abstract object? ExtractCustomProperties(TPlugInEventData plugInEventData);

    }
}
