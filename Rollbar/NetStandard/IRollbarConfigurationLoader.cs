namespace Rollbar.NetStandard
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Telemetry;

    public interface IRollbarConfigurationLoader
    {
        /// <summary>
        /// Loads the povided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        bool Load(RollbarConfig config);

        /// <summary>
        /// Loads the povided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        bool Load(TelemetryConfig config);

        /// <summary>
        /// Loads the rollbar configuration.
        /// </summary>
        /// <returns>IRollbarConfig or null if no configuration store was found.</returns>
        IRollbarConfig LoadRollbarConfig();

        /// <summary>
        /// Loads the telemetry configuration.
        /// </summary>
        /// <returns>ITelemetryConfig or null if no configuration store was found.</returns>
        ITelemetryConfig LoadTelemetryConfig();

        /// <summary>
        /// Loads the rollbar configuration.
        /// </summary>
        /// <param name="configFileName">Name of the configuration file.</param>
        /// <param name="configFilePath">The configuration file path.</param>
        /// <returns>IRollbarConfig or null if no configuration store was found.</returns>
        //IRollbarConfig LoadRollbarConfig(string configFileName, string configFilePath = null);

        /// <summary>
        /// Loads the telemetry configuration.
        /// </summary>
        /// <param name="configFileName">Name of the configuration file.</param>
        /// <param name="configFilePath">The configuration file path.</param>
        /// <returns>ITelemetryConfig or null if no configuration store was found.</returns>
        //ITelemetryConfig LoadTelemetryConfig(string configFileName, string configFilePath = null);


        //bool Load(RollbarConfig config, string configFileName, string configFilePath);
        //bool Load(TelemetryConfig config, string configFileName, string configFilePath);
    }
}
