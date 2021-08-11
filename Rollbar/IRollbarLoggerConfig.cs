namespace Rollbar
{
    using System;

    using Rollbar.Common;

    /// <summary>
    /// Interface IRollbarLoggerConfig
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    /// Implements the <see cref="System.IEquatable{T}" />
    /// Implements the <see cref="Rollbar.ITraceable" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    /// <seealso cref="System.IEquatable{T}" />
    /// <seealso cref="Rollbar.ITraceable" />
    public interface IRollbarLoggerConfig
        : IReconfigurable<IRollbarLoggerConfig, IRollbarLoggerConfig>
        , IEquatable<IRollbarLoggerConfig>
        , ITraceable
    {
        /// <summary>
        /// Gets the rollbar destination options.
        /// </summary>
        /// <value>The rollbar destination options.</value>
        IRollbarDestinationOptions RollbarDestinationOptions { get; }

        /// <summary>
        /// Gets the HTTP proxy options.
        /// </summary>
        /// <value>The HTTP proxy options.</value>
        IHttpProxyOptions HttpProxyOptions { get; }

        /// <summary>
        /// Gets the rollbar developer options.
        /// </summary>
        /// <value>The rollbar developer options.</value>
        IRollbarDeveloperOptions RollbarDeveloperOptions { get; }

        /// <summary>
        /// Gets the rollbar data security options.
        /// </summary>
        /// <value>The rollbar data security options.</value>
        IRollbarDataSecurityOptions RollbarDataSecurityOptions { get; }

        /// <summary>
        /// Gets the rollbar payload addition options.
        /// </summary>
        /// <value>The rollbar payload addition options.</value>
        IRollbarPayloadAdditionOptions RollbarPayloadAdditionOptions { get; }

        /// <summary>
        /// Gets the rollbar payload manipulation options.
        /// </summary>
        /// <value>The rollbar payload manipulation options.</value>
        IRollbarPayloadManipulationOptions RollbarPayloadManipulationOptions { get; }
    }
}
