namespace Rollbar.App.Config
{
    using Rollbar;
    using Rollbar.NetStandard;

    /// <summary>
    /// Class RollbarConfigurationLoader.
    /// Implements the <see cref="Rollbar.NetStandard.IRollbarConfigurationLoader" />
    /// </summary>
    /// <seealso cref="Rollbar.NetStandard.IRollbarConfigurationLoader" />
    public class RollbarConfigurationLoader
        : IRollbarConfigurationLoader
    {
        /// <summary>
        /// Loads the provided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        public bool Load(RollbarInfrastructureConfig config)
        {
            return AppConfigUtility.LoadAppSettings(config);
        }

        /// <summary>
        /// Loads the provided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        public bool Load(RollbarTelemetryOptions config)
        {
            return AppConfigUtility.LoadAppSettings(config);
        }

        /// <summary>
        /// Loads the rollbar configuration.
        /// </summary>
        /// <returns>IRollbarConfig or null if no configuration store was found.</returns>
        public IRollbarInfrastructureConfig? LoadRollbarConfig()
        {
            RollbarInfrastructureConfig config = new RollbarInfrastructureConfig();
            if (this.Load(config))
            {
                return config;
            }
            return null;
        }

        /// <summary>
        /// Loads the telemetry configuration.
        /// </summary>
        /// <returns>ITelemetryConfig or null if no configuration store was found.</returns>
        public IRollbarTelemetryOptions? LoadTelemetryConfig()
        {
            RollbarTelemetryOptions config = new RollbarTelemetryOptions();
            if (this.Load(config))
            {
                return config;
            }
            return null;
        }
    }
}
