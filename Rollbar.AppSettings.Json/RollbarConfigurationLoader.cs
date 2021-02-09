namespace Rollbar.AppSettings.Json
{
    using Rollbar;
    using Rollbar.NetStandard;
    using Rollbar.Telemetry;

    /// <summary>
    /// Class RollbarConfigurationLoader.
    /// Implements the <see cref="Rollbar.NetStandard.IRollbarConfigurationLoader" />
    /// </summary>
    /// <seealso cref="Rollbar.NetStandard.IRollbarConfigurationLoader" />
    public class RollbarConfigurationLoader
        : IRollbarConfigurationLoader
    {
        /// <summary>
        /// Loads the povided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        public bool Load(RollbarConfig config)
        {
            return AppSettingsUtility.LoadAppSettings(config);
        }

        /// <summary>
        /// Loads the povided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        public bool Load(TelemetryConfig config)
        {
            return AppSettingsUtility.LoadAppSettings(config);
        }

        /// <summary>
        /// Loads the rollbar configuration.
        /// </summary>
        /// <returns>IRollbarConfig or null if no configuration store was found.</returns>
        public IRollbarConfig LoadRollbarConfig()
        {
            RollbarConfig config = new RollbarConfig("seedToken");
            if(this.Load(config))
            {
                return config;
            }
            return null;
        }

        /// <summary>
        /// Loads the telemetry configuration.
        /// </summary>
        /// <returns>ITelemetryConfig or null if no configuration store was found.</returns>
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
