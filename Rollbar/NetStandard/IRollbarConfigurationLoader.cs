namespace Rollbar.NetStandard
{
    /// <summary>
    /// Interface IRollbarConfigurationLoader
    /// </summary>
    public interface IRollbarConfigurationLoader
    {
        /// <summary>
        /// Loads the provided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        bool Load(RollbarInfrastructureConfig config);

        /// <summary>
        /// Loads the provided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        bool Load(RollbarTelemetryOptions config);

        /// <summary>
        /// Loads the rollbar configuration.
        /// </summary>
        /// <returns>IRollbarConfig or null if no configuration store was found.</returns>
        IRollbarInfrastructureConfig? LoadRollbarConfig();

        /// <summary>
        /// Loads the telemetry configuration.
        /// </summary>
        /// <returns>ITelemetryConfig or null if no configuration store was found.</returns>
        IRollbarTelemetryOptions? LoadTelemetryConfig();

    }
}
