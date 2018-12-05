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
        private readonly RollbarPlugInCore _rollbarPlugIn;

        public RollbarTarget()
            :this(CreateDefaultRollbarConfig())
        {
        }

        public RollbarTarget(IRollbarConfig rollbarConfig)
        {
            OptimizeBufferReuse = true;
            Layout = "${message}";

            this._rollbarPlugIn =
                new RollbarPlugInCore(rollbarConfig, RollbarPlugInCore.DefaultRollbarBlockingTimeout, this);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent != null)
            {
                this._rollbarPlugIn.ReportToRollbar(logEvent, logEvent.Level);
            }
        }

        public string GetFormattedEventMessage(LogEventInfo logEventInfo)
        {
            return RenderLogEvent(Layout, logEventInfo);
        }

        public IDictionary<string, object> GetEventProperties(LogEventInfo logEventInfo)
        {
            return this.GetAllProperties(logEventInfo);
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
    }
}
