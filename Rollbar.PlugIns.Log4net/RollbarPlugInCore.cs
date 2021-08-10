namespace Rollbar.PlugIns.Log4net
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using log4net.Core;

    /// <summary>
    /// Class RollbarPlugInCore.
    /// Implements the <see cref="Rollbar.PlugIns.PlugInCore{TPlugInErrorLevel, TPlugInEventData}" />
    /// </summary>
    /// <seealso cref="Rollbar.PlugIns.PlugInCore{TPlugInErrorLevel, TPlugInEventData}" />
    internal class RollbarPlugInCore
        : PlugInCore<int, LoggingEvent>
    {
        /// <summary>
        /// The custom prefix
        /// </summary>
        private const string customPrefix = "log4net";

        /// <summary>
        /// The rollbar error level by plug in error level
        /// </summary>
        private static readonly IDictionary<int, ErrorLevel> rollbarErrorLevelByPlugInErrorLevel = new Dictionary<int, ErrorLevel>
        {
            { Level.Critical.Value, ErrorLevel.Critical },
            { Level.Emergency.Value, ErrorLevel.Critical },
            { Level.Fatal.Value, ErrorLevel.Critical },

            { Level.Error.Value, ErrorLevel.Error },

            { Level.Alert.Value, ErrorLevel.Warning },
            { Level.Notice.Value, ErrorLevel.Warning },
            { Level.Warn.Value, ErrorLevel.Warning },

            { Level.Info.Value, ErrorLevel.Info },

            { Level.All.Value, ErrorLevel.Debug },
            { Level.Off.Value, ErrorLevel.Debug },

            // NOTE: the Fine, Finer and Finest log4net levels have their values corresponding to 
            // following three log4net levels: Verbose, Trace, Debug. Hence, let's count them only once:
            //{ Level.Fine.Value, ErrorLevel.Debug },
            //{ Level.Finer.Value, ErrorLevel.Debug },
            //{ Level.Finest.Value, ErrorLevel.Debug },
            { Level.Verbose.Value, ErrorLevel.Debug },
            { Level.Trace.Value, ErrorLevel.Debug },
            { Level.Debug.Value, ErrorLevel.Debug },

            // NOTE: the Log4Net_Debug log4net level  has its value corresponding to the Emergency level
            // that is already accounted for above:
            //{ Level.Log4Net_Debug.Value, ErrorLevel.Debug },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPlugInCore"/> class.
        /// </summary>
        /// <param name="rollbarAccessToken">The rollbar access token.</param>
        /// <param name="rollbarEnvironment">The rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The rollbar blocking logging timeout.</param>
        public RollbarPlugInCore(
            string rollbarAccessToken,
            string rollbarEnvironment,
            TimeSpan? rollbarBlockingLoggingTimeout
            )
            : this(
                  CreateConfig(rollbarAccessToken: rollbarAccessToken, rollbarEnvironment: rollbarEnvironment),
                  rollbarBlockingLoggingTimeout
                  )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPlugInCore"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="rollbarBlockingTimeout">The rollbar blocking timeout.</param>
        public RollbarPlugInCore(
            IRollbarInfrastructureConfig rollbarConfig, 
            TimeSpan? rollbarBlockingTimeout
            ) 
            : base(rollbarErrorLevelByPlugInErrorLevel, customPrefix, rollbarConfig, rollbarBlockingTimeout)
        {
        }

        /// <summary>
        /// Extracts the custom properties  for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>Usually, either a data structure or a key-value dictionary returned as a System.Object.</returns>
        protected override object ExtractCustomProperties(LoggingEvent plugInEventData)
        {
            return plugInEventData.GetLoggingEventData();
        }

        /// <summary>
        /// Extracts the exception for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>Exception.</returns>
        protected override Exception ExtractException(LoggingEvent plugInEventData)
        {
            return plugInEventData.ExceptionObject;
        }

        /// <summary>
        /// Extracts the message for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>System.String.</returns>
        protected override string ExtractMessage(LoggingEvent plugInEventData)
        {
            return plugInEventData.RenderedMessage;
        }
    }
}
