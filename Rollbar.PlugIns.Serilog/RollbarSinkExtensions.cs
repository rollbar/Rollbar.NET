namespace Rollbar.PlugIns.Serilog
{
    using global::Serilog;
    using global::Serilog.Configuration;
    using System;

    /// <summary>
    /// Class RollbarSinkExtensions.
    /// </summary>
    public static class RollbarSinkExtensions
    {
        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  string rollbarAccessToken,
                  string rollbarEnvironment
                  )
        {
            return RollbarSink(
                loggerConfiguration,
                rollbarAccessToken,
                rollbarEnvironment,
                null,
                null
                );
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  string rollbarAccessToken,
                  string rollbarEnvironment,
                  IFormatProvider formatProvider
                  )
        {
            return RollbarSink(
                loggerConfiguration,
                rollbarAccessToken,
                rollbarEnvironment,
                null,
                null
                );
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  string rollbarAccessToken,
                  string rollbarEnvironment,
                  TimeSpan? rollbarBlockingLoggingTimeout
                  )
        {
            return RollbarSink(
                loggerConfiguration, 
                rollbarAccessToken, 
                rollbarEnvironment, 
                rollbarBlockingLoggingTimeout, 
                null
                );
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  string rollbarAccessToken,
                  string rollbarEnvironment,
                  TimeSpan? rollbarBlockingLoggingTimeout,
                  IFormatProvider formatProvider)
        {
            IRollbarConfig config = new RollbarConfig(rollbarAccessToken)
            {
                Environment = rollbarEnvironment,
            };

            return loggerConfiguration.RollbarSink(config, rollbarBlockingLoggingTimeout, formatProvider);
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IRollbarConfig rollbarConfig
                  )
        {
            return RollbarSink(loggerConfiguration, rollbarConfig, null, null);
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IRollbarConfig rollbarConfig,
                  TimeSpan? rollbarBlockingLoggingTimeout
                  )
        {
            return RollbarSink(loggerConfiguration, rollbarConfig, rollbarBlockingLoggingTimeout);
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IRollbarConfig rollbarConfig,
                  IFormatProvider formatProvider
                  )
        {
            return RollbarSink(loggerConfiguration, rollbarConfig, null, formatProvider);
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IRollbarConfig rollbarConfig,
                  TimeSpan? rollbarBlockingLoggingTimeout,
                  IFormatProvider formatProvider)
        {
            return loggerConfiguration.Sink(new RollbarSink(rollbarConfig, rollbarBlockingLoggingTimeout, formatProvider));
        }
    }
}
