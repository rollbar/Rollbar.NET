namespace Rollbar
{
    using System;

    using Rollbar.Common;

    /// <summary>
    /// Interface IRollbarInfrastructureConfig
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    /// Implements the <see cref="System.IEquatable{T}" />
    /// Implements the <see cref="Rollbar.ITraceable" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    /// <seealso cref="System.IEquatable{T}" />
    /// <seealso cref="Rollbar.ITraceable" />
    public interface IRollbarInfrastructureConfig
        : IReconfigurable<IRollbarInfrastructureConfig, IRollbarInfrastructureConfig>
        , IEquatable<IRollbarInfrastructureConfig>
        , ITraceable
    {
        /// <summary>
        /// Gets the rollbar infrastructure options.
        /// </summary>
        /// <value>The rollbar infrastructure options.</value>
        IRollbarInfrastructureOptions RollbarInfrastructureOptions
        {
            get;
        }

        /// <summary>
        /// Gets the rollbar telemetry options.
        /// </summary>
        /// <value>The rollbar telemetry options.</value>
        IRollbarTelemetryOptions RollbarTelemetryOptions
        {
            get;
        }

        /// <summary>
        /// Gets the rollbar offline store options.
        /// </summary>
        /// <value>The rollbar offline store options.</value>
        IRollbarOfflineStoreOptions RollbarOfflineStoreOptions
        {
            get;
        }

        /// <summary>
        /// Gets the rollbar logger configuration.
        /// </summary>
        /// <value>The rollbar logger configuration.</value>
        IRollbarLoggerConfig RollbarLoggerConfig
        {
            get;
        }

    }
}
