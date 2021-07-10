namespace Rollbar.PlugIns.Serilog
{
    using System;
    using System.Collections.Generic;
    using global::Serilog.Events;

#pragma warning disable CS1658 // Warning is overriding an error
#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
    /// <summary>
    /// Class RollbarPlugInCore.
    /// Implements the <see cref="Rollbar.PlugIns.PlugInCore{Serilog.Events.LogEventLevel, Serilog.Events.LogEvent}" />
    /// </summary>
    /// <seealso cref="Rollbar.PlugIns.PlugInCore{Serilog.Events.LogEventLevel, Serilog.Events.LogEvent}" />
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
    internal class RollbarPlugInCore
         : PlugInCore<LogEventLevel, LogEvent>
    {
        /// <summary>
        /// The custom prefix
        /// </summary>
        private const string customPrefix = "serilog";

        /// <summary>
        /// The rollbar error level by plug in error level
        /// </summary>
        private static readonly IDictionary<LogEventLevel, ErrorLevel> rollbarErrorLevelByPlugInErrorLevel = 
            new Dictionary<LogEventLevel, ErrorLevel> {

                { LogEventLevel.Fatal, ErrorLevel.Critical },
                { LogEventLevel.Error, ErrorLevel.Error },
                { LogEventLevel.Warning, ErrorLevel.Warning },
                { LogEventLevel.Information, ErrorLevel.Info },
                { LogEventLevel.Verbose, ErrorLevel.Debug },
                { LogEventLevel.Debug, ErrorLevel.Debug },
            };

        /// <summary>
        /// The format provider
        /// </summary>
        private readonly IFormatProvider _formatProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPlugInCore"/> class.
        /// </summary>
        /// <param name="rollbarAccessToken">The rollbar access token.</param>
        /// <param name="rollbarEnvironment">The rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The rollbar blocking logging timeout.</param>
        /// <param name="formatProvider">The format provider.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPlugInCore"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="rollbarBlockingTimeout">The rollbar blocking timeout.</param>
        /// <param name="formatProvider">The format provider.</param>
        public RollbarPlugInCore(
            IRollbarInfrastructureConfig rollbarConfig, 
            TimeSpan? rollbarBlockingTimeout,
            IFormatProvider formatProvider
            ) 
            : base(rollbarErrorLevelByPlugInErrorLevel, customPrefix, rollbarConfig, rollbarBlockingTimeout)
        {
            this._formatProvider = formatProvider;
        }

        /// <summary>
        /// Extracts the custom properties  for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>Usually, either a data structure or a key-value dictionary returned as a System.Object.</returns>
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
                custom["LogEventRenderedMessage"] = plugInEventData.RenderMessage(this._formatProvider);
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

        /// <summary>
        /// Extracts the exception for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>Exception.</returns>
        protected override Exception ExtractException(LogEvent plugInEventData)
        {
            return plugInEventData.Exception;
        }

        /// <summary>
        /// Extracts the message for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>System.String.</returns>
        protected override string ExtractMessage(LogEvent plugInEventData)
        {
            return plugInEventData.RenderMessage(this._formatProvider);
        }
    }
}
