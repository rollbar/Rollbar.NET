namespace Rollbar.PlugIns.Serilog
{
    using System;
    using System.Collections.Generic;
    using global::Serilog.Core;
    using global::Serilog.Events;
    using Serilog;

    public class RollbarSink
        : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly IRollbarConfig _rollbarConfig;
        private readonly IAsyncLogger _rollbarAsyncLogger;
        private readonly ILogger _rollbarLogger;

        public RollbarSink(
            string rollbarAccessToken, 
            string rollbarEnvironment,
            TimeSpan? rollbarBlockingLoggingTimeout,
            IFormatProvider formatProvider
            )
            : this(
                  new RollbarConfig(rollbarAccessToken) { Environment = rollbarEnvironment}, 
                  rollbarBlockingLoggingTimeout,
                  formatProvider
                  )
        {
        }

        public RollbarSink(
            IRollbarConfig rollbarConfig,
            TimeSpan? rollbarBlockingLoggingTimeout,
            IFormatProvider formatProvider
            )
        {
            this._rollbarConfig = rollbarConfig;

            IRollbar rollbar = RollbarFactory.CreateNew().Configure(this._rollbarConfig);

            RollbarFactory.CreateProper(
                this._rollbarConfig, 
                rollbarBlockingLoggingTimeout, 
                out this._rollbarAsyncLogger, 
                out this._rollbarLogger
                );

            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
            {
                return;
            }

            ErrorLevel rollbarLogLevel;
            switch (logEvent.Level)
            {
                case LogEventLevel.Fatal:
                    rollbarLogLevel = ErrorLevel.Critical;
                    break;
                case LogEventLevel.Error:
                    rollbarLogLevel = ErrorLevel.Error;
                    break;
                case LogEventLevel.Warning:
                    rollbarLogLevel = ErrorLevel.Warning;
                    break;
                case LogEventLevel.Information:
                    rollbarLogLevel = ErrorLevel.Info;
                    break;
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                default:
                    rollbarLogLevel = ErrorLevel.Debug;
                    break;
            }

            string message = logEvent.RenderMessage(this._formatProvider);

            DTOs.Body rollbarBody = null;
            if (logEvent.Exception != null)
            {
                rollbarBody = new DTOs.Body(logEvent.Exception);
            }
            else
            {
                rollbarBody = new DTOs.Body(new DTOs.Message(message));
            }

            int customCapacity = 1;
            if (logEvent.Properties != null)
            {
                customCapacity += logEvent.Properties.Count;
            }
            if (logEvent.Exception != null)
            {
                customCapacity++;
            }
            IDictionary<string, object> custom = new Dictionary<string, object>(customCapacity);
            if (logEvent.Exception != null)
            {
                custom["Serilog.LogEvent.RenderedMessage"] = message;
            }
            if (logEvent.Properties != null)
            {
                foreach (var property in logEvent.Properties)
                {
                    custom[property.Key] = property.Value.ToString();
                }
            }
            custom["Serilog.LogEvent.Timestamp"] = logEvent.Timestamp;

            DTOs.Data rollbarData = new DTOs.Data(this._rollbarConfig, rollbarBody, custom)
            {
                Level = rollbarLogLevel
            };

            this.ReportToRollbar(rollbarData);
        }

        private void ReportToRollbar(DTOs.Data rollbarData)
        {
            if (this._rollbarAsyncLogger != null)
            {
                RollbarSink.ReportToRollbar(this._rollbarAsyncLogger, rollbarData);
            }
            else if (this._rollbarLogger != null)
            {
                RollbarSink.ReportToRollbar(this._rollbarLogger, rollbarData);
            }
        }

        private static void ReportToRollbar(ILogger logger, DTOs.Data rollbarData)
        {
            logger.Log(rollbarData);
        }

        private static void ReportToRollbar(IAsyncLogger logger, DTOs.Data rollbarData)
        {
            logger.Log(rollbarData);
        }
    }
}
