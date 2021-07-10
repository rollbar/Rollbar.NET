namespace Rollbar.PlugIns.NLog
{
    using System;
    using System.Collections.Generic;
    using global::NLog;
    using Rollbar.PlugIns;

#pragma warning disable CS1658 // Warning is overriding an error
#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
    /// <summary>
    /// Class RollbarPlugInCore.
    /// Implements the <see cref="Rollbar.PlugIns.PlugInCore{NLog.LogLevel, NLog.LogEventInfo}" />
    /// </summary>
    /// <seealso cref="Rollbar.PlugIns.PlugInCore{NLog.LogLevel, NLog.LogEventInfo}" />
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
    internal class RollbarPlugInCore
        : PlugInCore<LogLevel, LogEventInfo>
    {
        /// <summary>
        /// The custom prefix
        /// </summary>
        private const string customPrefix = "nlog";

        /// <summary>
        /// The rollbar error level by plug in error level
        /// </summary>
        private static readonly IDictionary<LogLevel, ErrorLevel> rollbarErrorLevelByPlugInErrorLevel =
            new Dictionary<LogLevel, ErrorLevel> {

                { LogLevel.Fatal, ErrorLevel.Critical },
                { LogLevel.Error, ErrorLevel.Error },
                { LogLevel.Warn, ErrorLevel.Warning },
                { LogLevel.Info, ErrorLevel.Info },
                { LogLevel.Debug, ErrorLevel.Debug },
                { LogLevel.Trace, ErrorLevel.Debug },
            };

        /// <summary>
        /// The rollbar target
        /// </summary>
        private readonly RollbarTarget _rollbarTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPlugInCore"/> class.
        /// </summary>
        /// <param name="rollbarAccessToken">The rollbar access token.</param>
        /// <param name="rollbarEnvironment">The rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The rollbar blocking logging timeout.</param>
        /// <param name="rollbarTarget">The rollbar target.</param>
        public RollbarPlugInCore(
            string rollbarAccessToken,
            string rollbarEnvironment,
            TimeSpan? rollbarBlockingLoggingTimeout,
            RollbarTarget rollbarTarget
            )
            : this(
                  CreateConfig(rollbarAccessToken: rollbarAccessToken, rollbarEnvironment: rollbarEnvironment),
                  rollbarBlockingLoggingTimeout,
                  rollbarTarget
                  )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPlugInCore"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="rollbarBlockingTimeout">The rollbar blocking timeout.</param>
        /// <param name="rollbarTarget">The rollbar target.</param>
        public RollbarPlugInCore(
            IRollbarInfrastructureConfig rollbarConfig,
            TimeSpan? rollbarBlockingTimeout,
            RollbarTarget rollbarTarget
            )
            : base(rollbarErrorLevelByPlugInErrorLevel, customPrefix, rollbarConfig, rollbarBlockingTimeout)
        {
            this._rollbarTarget = rollbarTarget;
        }

        /// <summary>
        /// Extracts the custom properties  for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>Usually, either a data structure or a key-value dictionary returned as a System.Object.</returns>
        protected override object ExtractCustomProperties(LogEventInfo plugInEventData)
        {
            return this._rollbarTarget.GetEventProperties(plugInEventData);
        }

        /// <summary>
        /// Extracts the exception for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>Exception.</returns>
        protected override Exception ExtractException(LogEventInfo plugInEventData)
        {
            return plugInEventData.Exception;
        }

        /// <summary>
        /// Extracts the message for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>System.String.</returns>
        protected override string ExtractMessage(LogEventInfo plugInEventData)
        {
            return this._rollbarTarget.GetFormattedEventMessage(plugInEventData);
        }
    }
}
