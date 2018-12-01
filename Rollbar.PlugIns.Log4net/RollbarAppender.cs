namespace Rollbar.PlugIns.Log4net
{
    using System;
    using System.Collections.Generic;
    using log4net.Appender;
    using log4net.Core;

    /// <summary>
    /// Class RollbarAppender.
    /// Implements the <see cref="log4net.Appender.IAppender" />
    /// Implements the <see cref="log4net.Appender.AppenderSkeleton" />
    /// </summary>
    /// <seealso cref="log4net.Appender.AppenderSkeleton" />
    /// <seealso cref="log4net.Appender.IAppender" />
    public class RollbarAppender
        : AppenderSkeleton
        , IAppender
    {
        /// <summary>
        /// The rollbar timeout in seconds
        /// </summary>
        private const int rollbarTimeoutSeconds = 3;

        /// <summary>
        /// The custom prefix
        /// </summary>
        private const string prefix = "log4net";

        /// <summary>
        /// The Rollbar error level by log4net level value
        /// </summary>
        private static readonly IDictionary<int, Rollbar.ErrorLevel> rollbarErrorLevelByLog4netLevelValue;

        /// <summary>
        /// Initializes static members of the <see cref="RollbarAppender"/> class.
        /// </summary>
        static RollbarAppender()
        {
            rollbarErrorLevelByLog4netLevelValue = new Dictionary<int, Rollbar.ErrorLevel>();

            rollbarErrorLevelByLog4netLevelValue.Add(Level.Critical.Value, ErrorLevel.Critical);
            rollbarErrorLevelByLog4netLevelValue.Add(Level.Emergency.Value, ErrorLevel.Critical);
            rollbarErrorLevelByLog4netLevelValue.Add(Level.Fatal.Value, ErrorLevel.Critical);

            rollbarErrorLevelByLog4netLevelValue.Add(Level.Error.Value, ErrorLevel.Error);

            rollbarErrorLevelByLog4netLevelValue.Add(Level.Alert.Value, ErrorLevel.Warning);
            rollbarErrorLevelByLog4netLevelValue.Add(Level.Notice.Value, ErrorLevel.Warning);
            rollbarErrorLevelByLog4netLevelValue.Add(Level.Warn.Value, ErrorLevel.Warning);

            rollbarErrorLevelByLog4netLevelValue.Add(Level.Info.Value, ErrorLevel.Info);

            rollbarErrorLevelByLog4netLevelValue.Add(Level.All.Value, ErrorLevel.Debug);
            rollbarErrorLevelByLog4netLevelValue.Add(Level.Off.Value, ErrorLevel.Debug);

            // NOTE: the Fine, Finer and Finest log4net levels have their values corresponding to 
            // following three log4net levels: Verbose, Trace, Debug. Hence, let's count them only once:
            //rollbarErrorLevelByLog4netLevelValue.Add(Level.Fine.Value, ErrorLevel.Debug);
            //rollbarErrorLevelByLog4netLevelValue.Add(Level.Finer.Value, ErrorLevel.Debug);
            //rollbarErrorLevelByLog4netLevelValue.Add(Level.Finest.Value, ErrorLevel.Debug);
            rollbarErrorLevelByLog4netLevelValue.Add(Level.Verbose.Value, ErrorLevel.Debug);
            rollbarErrorLevelByLog4netLevelValue.Add(Level.Trace.Value, ErrorLevel.Debug);
            rollbarErrorLevelByLog4netLevelValue.Add(Level.Debug.Value, ErrorLevel.Debug);

            // NOTE: the Log4Net_Debug log4net level  has its value corresponding to the Emergency level
            // that is already accounted for above:
            //rollbarErrorLevelByLog4netLevelValue.Add(Level.Log4Net_Debug.Value, ErrorLevel.Debug);
        }

        /// <summary>
        /// Translates the specified log4net level.
        /// </summary>
        /// <param name="log4netLevel">The log4net level.</param>
        /// <returns>ErrorLevel.</returns>
        private static ErrorLevel Translate(Level log4netLevel)
        {
            if (rollbarErrorLevelByLog4netLevelValue.TryGetValue(log4netLevel.Value, out ErrorLevel rollbarErrorLevel))
            {
                return rollbarErrorLevel;
            }

            return ErrorLevel.Debug;
        }

        /// <summary>
        /// The Rollbar configuration
        /// </summary>
        private readonly Rollbar.IRollbarConfig _rollbarConfig;
        /// <summary>
        /// The Rollbar asynchronous logger
        /// </summary>
        private readonly Rollbar.IAsyncLogger _rollbarAsyncLogger;
        /// <summary>
        /// The Rollbar logger
        /// </summary>
        private readonly Rollbar.ILogger _rollbarLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarAppender"/> class.
        /// </summary>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        public RollbarAppender(
            string rollbarAccessToken,
            string rollbarEnvironment,
            TimeSpan? rollbarBlockingLoggingTimeout
            )
            : this(
                  new RollbarConfig(rollbarAccessToken) { Environment = rollbarEnvironment, }
                  , rollbarBlockingLoggingTimeout
                  )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarAppender"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        public RollbarAppender(
            IRollbarConfig rollbarConfig,
            TimeSpan? rollbarBlockingLoggingTimeout
            )
        {
            this._rollbarConfig = rollbarConfig;

            RollbarFactory.CreateProper(
                this._rollbarConfig,
                rollbarBlockingLoggingTimeout,
                out this._rollbarAsyncLogger,
                out this._rollbarLogger
                );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarAppender"/> class.
        /// </summary>
        public RollbarAppender()
        {
            RollbarConfig rollbarConfig = new RollbarConfig("just_a_seed_value");
#if NETSTANDARD
            Rollbar.NetCore.AppSettingsUtil.LoadAppSettings(ref rollbarConfig);
#endif
            this._rollbarConfig = rollbarConfig;
            RollbarFactory.CreateProper(
                this._rollbarConfig,
                TimeSpan.FromSeconds(rollbarTimeoutSeconds),
                out this._rollbarAsyncLogger,
                out this._rollbarLogger
                );
        }

        /// <summary>
        /// Raises the Close event.
        /// </summary>
        protected override void OnClose()
        {
            (this._rollbarAsyncLogger as IDisposable)?.Dispose();
            (this._rollbarLogger as IDisposable)?.Dispose();

            base.OnClose();
        }

        /// <summary>
        /// Appends the specified logging event.
        /// </summary>
        /// <param name="loggingEvent">The logging event.</param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                return;
            }

            ErrorLevel rollbarLogLevel = Translate(loggingEvent.Level);

            string message = loggingEvent.RenderedMessage;

            DTOs.Body rollbarBody = null;
            if (loggingEvent.ExceptionObject != null)
            {
                rollbarBody = new DTOs.Body(loggingEvent.ExceptionObject);
            }
            else
            {
                rollbarBody = new DTOs.Body(new DTOs.Message(message));
            }

            int customCapacity = 50;
            IDictionary<string, object> custom = new Dictionary<string, object>(customCapacity);
            custom[prefix] = loggingEvent.GetLoggingEventData();

            DTOs.Data rollbarData = new DTOs.Data(this._rollbarConfig, rollbarBody, custom)
            {
                Level = rollbarLogLevel
            };

            this.ReportToRollbar(rollbarData);
        }

        /// <summary>
        /// Reports to rollbar.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        private void ReportToRollbar(DTOs.Data rollbarData)
        {
            if (this._rollbarAsyncLogger != null)
            {
                RollbarAppender.ReportToRollbar(this._rollbarAsyncLogger, rollbarData);
            }
            else if (this._rollbarLogger != null)
            {
                RollbarAppender.ReportToRollbar(this._rollbarLogger, rollbarData);
            }
        }

        /// <summary>
        /// Reports to rollbar.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="rollbarData">The rollbar data.</param>
        private static void ReportToRollbar(Rollbar.ILogger logger, DTOs.Data rollbarData)
        {
            logger.Log(rollbarData);
        }

        /// <summary>
        /// Reports to rollbar.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="rollbarData">The rollbar data.</param>
        private static void ReportToRollbar(IAsyncLogger logger, DTOs.Data rollbarData)
        {
            logger.Log(rollbarData);
        }

    }
}
