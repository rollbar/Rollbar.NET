namespace Rollbar.PlugIns.NLog
{
    using System;
    using System.Collections.Generic;
    using global::NLog;
    using global::NLog.Config;
    using global::NLog.Targets;

    /// <summary>
    /// Class RollbarTarget for NLog.
    /// </summary>
    [Target("Rollbar.PlugIns.NLog")]
    public class RollbarTarget : TargetWithContext
    {
        [NLogConfigurationIgnoreProperty]
        public RollbarConfig RollbarConfig { get; }

        /// <summary>
        /// Configure to avoid out-of-order enqueuing of LogEvents, because of Task-race-conditions
        /// </summary>
        public TimeSpan? BlockingLoggingTimeout { get; set; }

        /// <summary>
        /// The Rollbar asynchronous logger
        /// </summary>
        private Rollbar.IAsyncLogger _rollbarAsyncLogger;

        /// <summary>
        /// The Rollbar logger
        /// </summary>
        private Rollbar.ILogger _rollbarLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarTarget"/> class.
        /// </summary>
        public RollbarTarget()
            :this(CreateDefaultRollbarConfig())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarTarget"/> class.
        /// </summary>
        public RollbarTarget(RollbarConfig rollbarConfig)
        {
            RollbarConfig = rollbarConfig;
            OptimizeBufferReuse = true;
            Layout = "${message}";
        }

        protected override void InitializeTarget()
        {
            RollbarFactory.CreateProper(RollbarConfig, BlockingLoggingTimeout, out _rollbarAsyncLogger, out _rollbarLogger);
            base.InitializeTarget();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (!LevelMapper.TryGetValue(logEvent.Level, out ErrorLevel rollbarLogLevel))
                rollbarLogLevel = ErrorLevel.Debug;

            DTOs.Body rollbarBody = null;
            if (logEvent.Exception != null)
            {
                rollbarBody = new DTOs.Body(logEvent.Exception);
            }
            else
            {
                var formattedMessage = RenderLogEvent(Layout, logEvent);
                rollbarBody = new DTOs.Body(new DTOs.Message(formattedMessage));
            }

            IDictionary<string, object> custom = GetAllProperties(logEvent);
            DTOs.Data rollbarData = new DTOs.Data(RollbarConfig, rollbarBody, custom)
            {
                Level = rollbarLogLevel
            };

            ReportToRollbar(rollbarData);
        }

        /// <summary>
        /// Reports to Rollbar.
        /// </summary>
        /// <param name="rollbarData">The Rollbar data.</param>
        private void ReportToRollbar(DTOs.Data rollbarData)
        {
            if (_rollbarAsyncLogger != null)
            {
                _rollbarAsyncLogger.Log(rollbarData);
            }
            else if (_rollbarLogger != null)
            {
                _rollbarLogger.Log(rollbarData);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="RollbarConfig"/> from app.config (NetFramework) / appsettings.json (NetCore)
        /// </summary>
        private static RollbarConfig CreateDefaultRollbarConfig()
        {
            var rollbarConfig = new RollbarConfig("just_a_seed_value");
#if NETSTANDARD
            Rollbar.NetCore.AppSettingsUtil.LoadAppSettings(ref rollbarConfig);
#endif
            return rollbarConfig;
        }

        private static readonly Dictionary<LogLevel, ErrorLevel> LevelMapper = new Dictionary<LogLevel, ErrorLevel>
        {
            // Map NLog levels to Rollbar ErrorLevel
            { LogLevel.Fatal, ErrorLevel.Critical },
            { LogLevel.Error, ErrorLevel.Error },
            { LogLevel.Warn, ErrorLevel.Warning },
            { LogLevel.Info, ErrorLevel.Info },
            { LogLevel.Debug, ErrorLevel.Debug },
            { LogLevel.Trace, ErrorLevel.Debug },
        };
    }
}
