#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Rollbar.Diagnostics;
    using Rollbar.NetCore;
    using Rollbar.Telemetry;

    /// <summary>
    /// Utility type aiding in Rollbar configuration options/alternatives.
    /// </summary>
    public static class RollbarConfigurationUtil
    {
        /// <summary>
        /// Deduces the rollbar configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static IRollbarConfig DeduceRollbarConfig(IConfiguration configuration)
        {
            if (RollbarLocator.RollbarInstance.Config.AccessToken != null)
            {
                return RollbarLocator.RollbarInstance.Config;
            }

            // Here we assume that the Rollbar singleton was not explicitly preconfigured 
            // anywhere in the code (Program.cs or Startup.cs), 
            // so we are trying to configure it from IConfiguration:

            Assumption.AssertNotNull(configuration, nameof(configuration));

            const string defaultAccessToken = "none";
            RollbarConfig rollbarConfig = new RollbarConfig(defaultAccessToken);
            AppSettingsUtility.LoadAppSettings(rollbarConfig, configuration);

            if (rollbarConfig.AccessToken == defaultAccessToken)
            {
                const string error = "Rollbar.NET notifier is not configured properly. A valid access token needs to be specified.";
                throw new Exception(error);
            }

            RollbarLocator.RollbarInstance
                .Configure(rollbarConfig);

            return rollbarConfig;
        }

        /// <summary>
        /// Deduces the rollbar telemetry configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static ITelemetryConfig DeduceRollbarTelemetryConfig(IConfiguration configuration)
        {
            TelemetryConfig config = new TelemetryConfig();
            AppSettingsUtility.LoadAppSettings(config, configuration);

            TelemetryCollector.Instance.Config.Reconfigure(config);

            return config;
        }
    }
}

#endif
