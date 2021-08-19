namespace Rollbar
{
    using System;

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
        IRollbar Configure(IRollbarLoggerConfig settings);

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        IRollbarLoggerConfig Config { get; }

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
