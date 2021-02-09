namespace Rollbar.NetStandard
{
    using Rollbar.Telemetry;

    /// <summary>
    /// Interface IRollbarConfigurationLoader
    /// </summary>
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

    }
}
