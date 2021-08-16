namespace Rollbar.PlugIns.Serilog
{
    using global::Serilog;
    using global::Serilog.Core;
    using global::Serilog.Configuration;
    using global::Serilog.Events;
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
        /// <param name="restrictedToMinimumLevel">
        ///   The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.
        /// </param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  string rollbarAccessToken,
                  string rollbarEnvironment,
                  LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                  LoggingLevelSwitch? levelSwitch = null
                  )
        {
            return RollbarSink(
                loggerConfiguration,
                rollbarAccessToken,
                rollbarEnvironment,
                null,
                null,
                restrictedToMinimumLevel,
                levelSwitch);
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="restrictedToMinimumLevel">
        ///   The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.
        /// </param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  string rollbarAccessToken,
                  string rollbarEnvironment,
                  IFormatProvider formatProvider,
                  LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                  LoggingLevelSwitch? levelSwitch = null
                  )
        {
            return RollbarSink(
                loggerConfiguration,
                rollbarAccessToken,
                rollbarEnvironment,
                null,
                formatProvider,
                restrictedToMinimumLevel,
                levelSwitch);
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <param name="restrictedToMinimumLevel">
        ///   The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.
        /// </param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  string rollbarAccessToken,
                  string rollbarEnvironment,
                  TimeSpan? rollbarBlockingLoggingTimeout,
                  LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                  LoggingLevelSwitch? levelSwitch = null
                  )
        {
            return RollbarSink(
                loggerConfiguration, 
                rollbarAccessToken, 
                rollbarEnvironment, 
                rollbarBlockingLoggingTimeout, 
                null,
                restrictedToMinimumLevel,
                levelSwitch);
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="restrictedToMinimumLevel">
        ///   The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.
        /// </param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  string rollbarAccessToken,
                  string rollbarEnvironment,
                  TimeSpan? rollbarBlockingLoggingTimeout,
                  IFormatProvider? formatProvider,
                  LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                  LoggingLevelSwitch? levelSwitch = null
            )
        {
            RollbarDestinationOptions destinationOptions = new RollbarDestinationOptions(rollbarAccessToken, rollbarEnvironment);
            IRollbarInfrastructureConfig config = new RollbarInfrastructureConfig();
            config.RollbarLoggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);

            return loggerConfiguration.RollbarSink(config, rollbarBlockingLoggingTimeout, formatProvider, restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <param name="restrictedToMinimumLevel">
        ///   The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.
        /// </param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IRollbarInfrastructureConfig rollbarConfig,
                  LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                  LoggingLevelSwitch? levelSwitch = null
                  )
        {
            return RollbarSink(
                loggerConfiguration, 
                rollbarConfig, 
                null, 
                null,
                restrictedToMinimumLevel,
                levelSwitch);
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <param name="restrictedToMinimumLevel">
        ///   The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.
        /// </param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IRollbarInfrastructureConfig rollbarConfig,
                  TimeSpan? rollbarBlockingLoggingTimeout,
                  LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                  LoggingLevelSwitch? levelSwitch = null
                  )
        {
            return RollbarSink(
                loggerConfiguration, 
                rollbarConfig, 
                rollbarBlockingLoggingTimeout, 
                null,
                restrictedToMinimumLevel,
                levelSwitch);
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="restrictedToMinimumLevel">
        ///   The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.
        /// </param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IRollbarInfrastructureConfig rollbarConfig,
                  IFormatProvider formatProvider,
                  LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                  LoggingLevelSwitch? levelSwitch = null
                  )
        {
            return RollbarSink(
                loggerConfiguration, 
                rollbarConfig, 
                null, 
                formatProvider,
                restrictedToMinimumLevel,
                levelSwitch);
        }

        /// <summary>
        /// Provides configuration for RollbarSink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="restrictedToMinimumLevel">
        ///   The minimum level for events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.
        /// </param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IRollbarInfrastructureConfig rollbarConfig,
                  TimeSpan? rollbarBlockingLoggingTimeout,
                  IFormatProvider? formatProvider,
                  LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                  LoggingLevelSwitch? levelSwitch = null)
        {
            return loggerConfiguration.Sink(
                new RollbarSink(rollbarConfig, rollbarBlockingLoggingTimeout, formatProvider),
                restrictedToMinimumLevel,
                levelSwitch);
        }
    }
}
