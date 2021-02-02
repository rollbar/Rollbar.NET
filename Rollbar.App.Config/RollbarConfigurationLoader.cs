namespace Rollbar.App.Config
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar;
    using Rollbar.NetStandard;
    using Rollbar.Telemetry;

    public class RollbarConfigurationLoader
        : IRollbarConfigurationLoader
    {
        public bool Load(RollbarConfig config)
        {
            return AppConfigUtility.LoadAppSettings(config);
        }

        public bool Load(TelemetryConfig config)
        {
            return AppConfigUtility.LoadAppSettings(config);
        }

        public IRollbarConfig LoadRollbarConfig()
        {
            RollbarConfig config = new RollbarConfig("seedToken");
            if (this.Load(config))
            {
                return config;
            }
            return null;
        }

        public ITelemetryConfig LoadTelemetryConfig()
        {
            TelemetryConfig config = new TelemetryConfig();
            if (this.Load(config))
            {
                return config;
            }
            return null;
        }
    }
}
