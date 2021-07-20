namespace Rollbar.AppSettings.Json
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Rollbar.Diagnostics;
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
        public static IRollbarInfrastructureConfig DeduceRollbarConfig(IConfiguration configuration)
        {
            //if (RollbarLocator.RollbarInstance.Config.RollbarDestinationOptions.AccessToken != null)
            if (RollbarInfrastructure.Instance.Config?.RollbarLoggerConfig?.RollbarDestinationOptions?.AccessToken != null)
            {
                //return RollbarLocator.RollbarInstance.Config;
                return RollbarInfrastructure.Instance.Config;
            }

            // Here we assume that the Rollbar singleton was not explicitly preconfigured 
            // anywhere in the code (Program.cs or Startup.cs), 
            // so we are trying to configure it from IConfiguration:

            Assumption.AssertNotNull(configuration, nameof(configuration));

            const string defaultAccessToken = "none";
            RollbarInfrastructureConfig rollbarConfig = new RollbarInfrastructureConfig(defaultAccessToken);
            AppSettingsUtility.LoadAppSettings(rollbarConfig, configuration);

            if (rollbarConfig.RollbarLoggerConfig.RollbarDestinationOptions.AccessToken == defaultAccessToken)
            {
                const string error = "Rollbar.NET notifier is not configured properly. A valid access token needs to be specified.";
                throw new Exception(error);
            }

            if(RollbarInfrastructure.Instance.IsInitialized)
            {
                RollbarInfrastructure.Instance.Config.Reconfigure(rollbarConfig);
            }
            else
            {
                RollbarInfrastructure.Instance.Init(rollbarConfig);
            }

            RollbarLocator.RollbarInstance
                .Configure(rollbarConfig.RollbarLoggerConfig);

            return rollbarConfig;
        }

        /// <summary>
        /// Deduces the rollbar telemetry configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static IRollbarTelemetryOptions DeduceRollbarTelemetryConfig(IConfiguration configuration)
        {
            RollbarTelemetryOptions config = new RollbarTelemetryOptions();
            AppSettingsUtility.LoadAppSettings(config, configuration);

            TelemetryCollector.Instance?.Config.Reconfigure(config);

            return config;
        }
    }
}
