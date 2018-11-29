namespace Rollbar.PlugIns.Log4net
{
    using System;
    using System.Collections.Generic;
    using log4net.Appender;
    using log4net.Core;
    using Rollbar.NetCore;

    /// <summary>
    /// Class RollbarAppender.
    /// Implements the <see cref="log4net.Appender.IAppender" />
    /// </summary>
    /// <seealso cref="log4net.Appender.IAppender" />
    public class RollbarAppender
        : AppenderSkeleton
        , IAppender
    {
        private const int rollbarTimeoutSeconds = 3;

        private static readonly IDictionary<int, Rollbar.ErrorLevel> rollbarErrorLevelByLog4netLevelValue;

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

        private static ErrorLevel Translate(Level log4netLevel)
        {
            if (rollbarErrorLevelByLog4netLevelValue.TryGetValue(log4netLevel.Value, out ErrorLevel rollbarErrorLevel))
            {
                return rollbarErrorLevel;
            }

            return ErrorLevel.Debug;
        }

        private readonly Rollbar.IRollbarConfig _rollbarConfig;
        private readonly Rollbar.IAsyncLogger _rollbarAsyncLogger;
        private readonly Rollbar.ILogger _rollbarLogger;

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

        public RollbarAppender()
        {
            RollbarConfig rollbarConfig = new RollbarConfig("just_a_seed_value");
            AppSettingsUtil.LoadAppSettings(ref rollbarConfig);
            this._rollbarConfig = rollbarConfig;

            RollbarFactory.CreateProper(
                this._rollbarConfig,
                TimeSpan.FromSeconds(rollbarTimeoutSeconds),
                out this._rollbarAsyncLogger,
                out this._rollbarLogger
                );
        }

        protected override void OnClose()
        {
            (this._rollbarAsyncLogger as IDisposable)?.Dispose();
            (this._rollbarLogger as IDisposable)?.Dispose();

            base.OnClose();
        }

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


            //TODO:
            int customCapacity = 50;
            //if (logEvent.Properties != null)
            //{
            //    customCapacity += logEvent.Properties.Count;
            //}
            //if (logEvent.Exception != null)
            //{
            //    customCapacity++;
            //}
            IDictionary<string, object> custom = new Dictionary<string, object>(customCapacity);
            //custom = RollbarAssistant.CaptureState(loggingEvent);
            //var properties = loggingEvent.GetProperties();
            //foreach (var key in properties.GetKeys())
            //{
            //    //custom[prefix + "." + key] = properties[key];
            //    custom[key] = properties[key];
            //}

            const string prefix = "log4net";

            //custom[prefix + "RenderedMessage"] = message;

            //custom[prefix + "Domain"] = loggingEvent.Domain;
            //custom[prefix + "ExceptionObject"] = loggingEvent.ExceptionObject;
            //custom[prefix + "Fix"] = loggingEvent.Fix;
            //custom[prefix + "Identity"] = loggingEvent.Identity;
            //custom[prefix + "Level"] = loggingEvent.Level;
            //custom[prefix + "LocationInformation"] = loggingEvent.LocationInformation;
            //custom[prefix + "LoggerName"] = loggingEvent.LoggerName;
            //custom[prefix + "MessageObject"] = loggingEvent.MessageObject;
            //custom[prefix + "RenderedMessage"] = loggingEvent.RenderedMessage;
            //custom[prefix + "ThreadName"] = loggingEvent.ThreadName;
            //custom[prefix + "TimeStamp"] = loggingEvent.TimeStamp;
            //custom[prefix + "TimeStampUtc"] = loggingEvent.TimeStampUtc;
            //custom[prefix + "UserName"] = loggingEvent.UserName;
            //custom[prefix + "GetExceptionStrRep"] = loggingEvent.GetExceptionStrRep();
            //custom[prefix + "GetLoggingEventData"] = loggingEvent.GetLoggingEventData();

            custom[prefix] = loggingEvent.GetLoggingEventData();


            //if (logEvent.Exception != null)
            //{
            //    custom["Serilog.LogEvent.RenderedMessage"] = message;
            //}
            //if (logEvent.Properties != null)
            //{
            //    foreach (var property in logEvent.Properties)
            //    {
            //        custom[property.Key] = property.Value.ToString();
            //    }
            //}
            //custom["Serilog.LogEvent.Timestamp"] = logEvent.Timestamp;

            DTOs.Data rollbarData = new DTOs.Data(this._rollbarConfig, rollbarBody, custom)
            {
                Level = rollbarLogLevel
            };

            this.ReportToRollbar(rollbarData);


            //loggingEvent.GetLoggingEventData().;
            //loggingEvent.
        }

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

        private static void ReportToRollbar(Rollbar.ILogger logger, DTOs.Data rollbarData)
        {
            logger.Log(rollbarData);
        }

        private static void ReportToRollbar(IAsyncLogger logger, DTOs.Data rollbarData)
        {
            logger.Log(rollbarData);
        }

    }
}
