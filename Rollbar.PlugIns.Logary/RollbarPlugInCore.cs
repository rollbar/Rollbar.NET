namespace Rollbar.PlugIns.NLog
{
    using System;
    using System.Collections.Generic;
    using Rollbar.PlugIns;

#pragma warning disable CS1658 // Warning is overriding an error
#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
    /// <summary>
    /// Class RollbarPlugInCore.
    /// Implements the <see cref="Rollbar.PlugIns.PlugInCore{NLog.LogLevel, NLog.LogEventInfo}" />
    /// </summary>
    /// <seealso cref="Rollbar.PlugIns.PlugInCore{NLog.LogLevel, NLog.LogEventInfo}" />
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
    internal class RollbarPlugInCore
       // : PlugInCore<LogLevel, LogEventInfo>
    {
        private const string customPrefix = "nlog";

        //private static readonly IDictionary<LogLevel, ErrorLevel> rollbarErrorLevelByPlugInErrorLevel =
        //    new Dictionary<LogLevel, ErrorLevel> {

        //        { LogLevel.Fatal, ErrorLevel.Critical },
        //        { LogLevel.Error, ErrorLevel.Error },
        //        { LogLevel.Warn, ErrorLevel.Warning },
        //        { LogLevel.Info, ErrorLevel.Info },
        //        { LogLevel.Debug, ErrorLevel.Debug },
        //        { LogLevel.Trace, ErrorLevel.Debug },
        //    };

        //private readonly RollbarTarget _rollbarTarget;

        //    string rollbarAccessToken,
        //    string rollbarEnvironment,
        //    TimeSpan? rollbarBlockingLoggingTimeout,
        //    RollbarTarget rollbarTarget
        //    )
        //    : this(
        //          CreateConfig(rollbarAccessToken: rollbarAccessToken, rollbarEnvironment: rollbarEnvironment),
        //          rollbarBlockingLoggingTimeout,
        //          rollbarTarget
        //          )
        //{
        //}

        //public RollbarPlugInCore(
        //    IRollbarConfig rollbarConfig,
        //    TimeSpan? rollbarBlockingTimeout,
        //    RollbarTarget rollbarTarget
        //    )
        //    : base(rollbarErrorLevelByPlugInErrorLevel, customPrefix, rollbarConfig, rollbarBlockingTimeout)
        //{
        //    this._rollbarTarget = rollbarTarget;
        //}

        //protected override object ExtractCustomProperties(LogEventInfo plugInEventData)
        //{
        //    return this._rollbarTarget.GetEventProperties(plugInEventData);
        //}

        //protected override Exception ExtractException(LogEventInfo plugInEventData)
        //{
        //    return plugInEventData.Exception;
        //}

        //protected override string ExtractMessage(LogEventInfo plugInEventData)
        //{
        //    return this._rollbarTarget.GetFormattedEventMessage(plugInEventData);
        //}
    }
}
