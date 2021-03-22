namespace Rollbar.NetStandard
{
    using System;

    using Rollbar.Telemetry;

    /// <summary>
    /// Class RollbarConfigUtility.
    /// </summary>
    [Obsolete("Use Rollbar.NetStandard.RollbarConfigurationLoader class instead")]
    public static class RollbarConfigUtility
    {
        private readonly static IRollbarConfigurationLoader configurationLoader = 
            new RollbarConfigurationLoader();

        /// <summary>
        /// Loads the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        public static bool Load(RollbarConfig config)
        {
            return RollbarConfigUtility.configurationLoader.Load(config);
        }

        /// <summary>
        /// Loads the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        public static bool Load(TelemetryConfig config)
        {
            return RollbarConfigUtility.configurationLoader.Load(config);
        }

        /// <summary>
        /// Loads the rollbar configuration.
        /// </summary>
        /// <returns>Either IRollbarConfig or null if no configuration file found.</returns>
        public static IRollbarConfig LoadRollbarConfig()
        {
            RollbarConfig config = new RollbarConfig("seedToken");
            if(RollbarConfigUtility.Load(config))
            {
                return config;
            }
            return null;
        }

        /// <summary>
        /// Loads the telemetry configuration.
        /// </summary>
        /// <returns>ITelemetryConfig.</returns>
        /// <returns>Either IRollbarConfig or null if no configuration file found.</returns>
        public static ITelemetryConfig LoadTelemetryConfig()
        {
            TelemetryConfig config = new TelemetryConfig();
            if (RollbarConfigUtility.Load(config))
            {
                return config;
            }
            return null;
        }

    }
}
