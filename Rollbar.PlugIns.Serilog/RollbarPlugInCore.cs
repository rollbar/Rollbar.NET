namespace Rollbar.PlugIns.Serilog
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using global::Serilog.Events;

    internal class RollbarPlugInCore
         : PlugInCore<LogEventLevel, LogEvent>
    {
        private const string customPrefix = "serilog";

        private static readonly IDictionary<LogEventLevel, ErrorLevel> rollbarErrorLevelByPlugInErrorLevel = 
            new Dictionary<LogEventLevel, ErrorLevel> {

                { LogEventLevel.Fatal, ErrorLevel.Critical },
                { LogEventLevel.Error, ErrorLevel.Error },
                { LogEventLevel.Warning, ErrorLevel.Warning },
                { LogEventLevel.Information, ErrorLevel.Info },
                { LogEventLevel.Verbose, ErrorLevel.Debug },
                { LogEventLevel.Debug, ErrorLevel.Debug },
            };

        private readonly IFormatProvider _formatProvider;

        public RollbarPlugInCore(
            string rollbarAccessToken,
            string rollbarEnvironment,
            TimeSpan? rollbarBlockingLoggingTimeout,
            IFormatProvider formatProvider
            )
            : this(
                  CreateConfig(rollbarAccessToken: rollbarAccessToken, rollbarEnvironment: rollbarEnvironment),
                  rollbarBlockingLoggingTimeout,
                  formatProvider
                  )
        {
        }

        public RollbarPlugInCore(
            IRollbarConfig rollbarConfig, 
            TimeSpan? rollbarBlockingTimeout,
            IFormatProvider formatProvider
            ) 
            : base(rollbarErrorLevelByPlugInErrorLevel, customPrefix, rollbarConfig, rollbarBlockingTimeout)
        {
            this._formatProvider = formatProvider;
        }

        protected override object ExtractCustomProperties(LogEvent plugInEventData)
        {
            int customCapacity = 1;
            if (plugInEventData.Properties != null)
            {
                customCapacity += plugInEventData.Properties.Count;
            }
            if (plugInEventData.Exception != null)
            {
                customCapacity++;
            }

            IDictionary<string, object> custom = new Dictionary<string, object>(customCapacity);
            if (plugInEventData.Exception != null)
            {
                custom["LogEventRenderedMessage"] = plugInEventData.RenderMessage(this._formatProvider); ;
            }
            if (plugInEventData.Properties != null)
            {
                foreach (var property in plugInEventData.Properties)
                {
                    custom[property.Key] = property.Value.ToString();
                }
            }
            custom["LogEventTimestamp"] = plugInEventData.Timestamp;

            return custom;
        }

        protected override Exception ExtractException(LogEvent plugInEventData)
        {
            return plugInEventData.Exception;
        }

        protected override string ExtractMessage(LogEvent plugInEventData)
        {
            return plugInEventData.RenderMessage(this._formatProvider);
        }
    }
}
