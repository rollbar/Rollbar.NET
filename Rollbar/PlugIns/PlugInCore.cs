namespace Rollbar.PlugIns
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public abstract class PlugInCore<TPlugInErrorLevel, TPlugInEventData>
    {
        private static readonly TimeSpan defaultRollbarBlockingTimeout = TimeSpan.FromSeconds(3);

        private readonly string _customPrefix;
        private readonly IDictionary<TPlugInErrorLevel, ErrorLevel> _rollbarErrorLevelByPlugInErrorLevel;

        /// <summary>
        /// The Rollbar configuration
        /// </summary>
        private readonly IRollbarConfig _rollbarConfig;
        /// <summary>
        /// The Rollbar asynchronous logger
        /// </summary>
        private readonly IAsyncLogger _rollbarAsyncLogger;
        /// <summary>
        /// The Rollbar logger
        /// </summary>
        private readonly ILogger _rollbarLogger;

        private PlugInCore()
        {

        }

        public PlugInCore(
            IDictionary<TPlugInErrorLevel, ErrorLevel> rollbarErrorLevelByPlugInErrorLevel,
            string customPrefix,
            IRollbarConfig rollbarConfig,
            TimeSpan? rollbarBlockingTimeout
            )
        {
            this._rollbarErrorLevelByPlugInErrorLevel = rollbarErrorLevelByPlugInErrorLevel;
            this._customPrefix = customPrefix.EndsWith(".") ? customPrefix : customPrefix + ".";
            this._rollbarConfig = rollbarConfig;

            RollbarFactory.CreateProper(
                this._rollbarConfig,
                rollbarBlockingTimeout,
                out this._rollbarAsyncLogger,
                out this._rollbarLogger
                );
        }

        public virtual void ReportToRollbar(TPlugInEventData plugInEventData, TPlugInErrorLevel plugInErrorLevel)
        {
            if (plugInEventData == null)
            {
                return;
            }

            DTOs.Data data = null;// this.Translate(plugInEventData, plugInErrorLevel);

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

        protected virtual DTOs.Data Translate(TPlugInEventData plugInEventData, TPlugInErrorLevel plugInErrorLevel)
        {
            ErrorLevel errorLevel = this.Translate(plugInErrorLevel);
            string message = this.ExtractMessage(plugInEventData);
            Exception exception = this.ExtractException(plugInEventData);
            IDictionary<string, object> pluginEventProperties = this.ExtractCustomProperties(plugInEventData);

            DTOs.Body rollbarBody = null;
            if (exception != null)
            {
                rollbarBody = new DTOs.Body(exception);
            }
            else
            {
                rollbarBody = new DTOs.Body(new DTOs.Message(message));
            }

            IDictionary<string, object> custom = new Dictionary<string, object>(pluginEventProperties.Count);
            foreach(var property in pluginEventProperties)
            {
                custom[this._customPrefix + property.Key] = property.Value;
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

        protected abstract string ExtractMessage(TPlugInEventData plugInEventData);
        protected abstract Exception ExtractException(TPlugInEventData plugInEventData);
        protected abstract IDictionary<string, object> ExtractCustomProperties(TPlugInEventData plugInEventData);

        /// <summary>
        /// Reports data to Rollbar.
        /// </summary>
        /// <param name="rollbarData">The Rollbar data.</param>
        private void ReportToRollbar(DTOs.Data rollbarData)
        {
            if (this._rollbarAsyncLogger != null)
            {
                ReportToRollbar(this._rollbarAsyncLogger, rollbarData);
            }
            else if (this._rollbarLogger != null)
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

        /// <summary>
        /// Reports data to Rollbar.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="rollbarData">The Rollbar data.</param>
        private static void ReportToRollbar(IAsyncLogger logger, DTOs.Data rollbarData)
        {
            logger.Log(rollbarData);
        }

    }
}
