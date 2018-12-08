namespace Rollbar.PlugIns.NLog
{
    using System;
    using System.Collections.Generic;
    using global::NLog;
    using global::NLog.Config;
    using global::NLog.Targets;
    using Rollbar.NetStandard;

    /// <summary>
    /// Class RollbarTarget for NLog.
    /// </summary>
    [Target("Rollbar.PlugIns.NLog")]
    public class RollbarTarget : TargetWithContext
    {
        private readonly RollbarPlugInCore _rollbarPlugIn;

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
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        public RollbarTarget(IRollbarConfig rollbarConfig)
        {
            OptimizeBufferReuse = true;
            Layout = "${message}";

            this._rollbarPlugIn =
                new RollbarPlugInCore(rollbarConfig, RollbarPlugInCore.DefaultRollbarBlockingTimeout, this);
        }

        /// <summary>
        /// Writes logging event to the log target. Must be overridden in inheriting
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent != null)
            {
                this._rollbarPlugIn.ReportToRollbar(logEvent, logEvent.Level);
            }
        }

        /// <summary>
        /// Gets the formatted event message.
        /// </summary>
        /// <param name="logEventInfo">The log event information.</param>
        /// <returns>System.String.</returns>
        public string GetFormattedEventMessage(LogEventInfo logEventInfo)
        {
            return RenderLogEvent(Layout, logEventInfo);
        }

        /// <summary>
        /// Gets the event properties.
        /// </summary>
        /// <param name="logEventInfo">The log event information.</param>
        /// <returns>IDictionary&lt;System.String, System.Object&gt;.</returns>
        public IDictionary<string, object> GetEventProperties(LogEventInfo logEventInfo)
        {
            return this.GetAllProperties(logEventInfo);
        }

        /// <summary>
        /// Initializes a new <see cref="RollbarConfig"/> from app.config (NetFramework) / appsettings.json (NetCore)
        /// </summary>
        private static IRollbarConfig CreateDefaultRollbarConfig()
        {
            return RollbarConfigUtil.LoadRollbarConfig();
        }
    }
}
