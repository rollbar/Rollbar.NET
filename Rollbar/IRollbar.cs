namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Defines IRollbar notifier interface.
    /// </summary>
    /// <seealso cref="Rollbar.ILogger" />
    /// <seealso cref="System.IDisposable" />
    public interface IRollbar
        : ILogger
        , IDisposable
    {
        /// <summary>
        /// Configures the using specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        IRollbar Configure(RollbarConfig settings);

        /// <summary>
        /// Configures using the specified access token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        IRollbar Configure(string accessToken);

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        RollbarConfig Config { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        ILogger Logger { get; }

        /// <summary>
        /// Occurs when a Rollbar internal event happens.
        /// </summary>
        event EventHandler<RollbarEventArgs> InternalEvent;
    }
}
