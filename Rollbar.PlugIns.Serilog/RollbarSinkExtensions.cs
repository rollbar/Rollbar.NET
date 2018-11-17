namespace Rollbar.PlugIns.Serilog
{
    using global::Serilog;
    using global::Serilog.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class RollbarSinkExtensions
    {
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  string rollbarAccessToken,
                  string rollbarEnvironment,
                  TimeSpan? rollbarBlockingLoggingTimeout,
                  IFormatProvider formatProvider = null)
        {
            IRollbarConfig config = new RollbarConfig(rollbarAccessToken)
            {
                Environment = rollbarEnvironment,
            };

            return loggerConfiguration.RollbarSink(config, rollbarBlockingLoggingTimeout, formatProvider);
        }

        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IRollbarConfig rollbarConfig,
                  TimeSpan? rollbarBlockingLoggingTimeout,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new RollbarSink(rollbarConfig, rollbarBlockingLoggingTimeout, formatProvider));
        }
    }
}
