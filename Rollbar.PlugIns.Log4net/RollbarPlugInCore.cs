namespace Rollbar.PlugIns.Log4net
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using log4net.Core;

    internal class RollbarPlugInCore
        : PlugInCore<int, LoggingEvent>
    {
        private const string customPrefix = "log4net";

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

        public RollbarPlugInCore(
            IRollbarConfig rollbarConfig, 
            TimeSpan? rollbarBlockingTimeout
            ) 
            : base(rollbarErrorLevelByPlugInErrorLevel, customPrefix, rollbarConfig, rollbarBlockingTimeout)
        {
        }

        protected override object ExtractCustomProperties(LoggingEvent plugInEventData)
        {
            return plugInEventData.GetLoggingEventData();
        }

        protected override Exception ExtractException(LoggingEvent plugInEventData)
        {
            return plugInEventData.ExceptionObject;
        }

        protected override string ExtractMessage(LoggingEvent plugInEventData)
        {
            return plugInEventData.RenderedMessage;
        }
    }
}
