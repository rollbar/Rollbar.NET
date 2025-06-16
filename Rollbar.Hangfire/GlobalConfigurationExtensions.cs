using System;
using Hangfire;

namespace Rollbar.Hangfire
{
    /// <summary>
    /// Extension methods for Hangfire GlobalConfiguration to easily integrate Rollbar.
    /// </summary>
    public static class GlobalConfigurationExtensions
    {
        /// <summary>
        /// Configures Hangfire to use Rollbar with default options.
        /// This will automatically capture job exceptions and track job lifecycle events.
        /// </summary>
        /// <param name="configuration">The Hangfire global configuration</param>
        /// <returns>The configuration instance for method chaining</returns>
        /// <example>
        /// <code>
        /// builder.Services.AddHangfire(config => config.UseRollbar());
        /// </code>
        /// </example>
        public static IGlobalConfiguration UseRollbar(this IGlobalConfiguration configuration)
        {
            return configuration.UseRollbar(new RollbarHangfireOptions());
        }

        /// <summary>
        /// Configures Hangfire to use Rollbar with the specified options.
        /// </summary>
        /// <param name="configuration">The Hangfire global configuration</param>
        /// <param name="options">Configuration options for the Rollbar integration</param>
        /// <returns>The configuration instance for method chaining</returns>
        /// <example>
        /// <code>
        /// builder.Services.AddHangfire(config => config.UseRollbar(new RollbarHangfireOptions
        /// {
        ///     CaptureJobArguments = true,
        ///     CapturePerformanceMetrics = true,
        ///     ScrubFields = new[] { "password", "apiKey" }
        /// }));
        /// </code>
        /// </example>
        public static IGlobalConfiguration UseRollbar(this IGlobalConfiguration configuration, RollbarHangfireOptions options)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var filter = new RollbarHangfireFilter(options);
            return configuration.UseFilter(filter);
        }

        /// <summary>
        /// Configures Hangfire to use Rollbar with a configuration delegate.
        /// </summary>
        /// <param name="configuration">The Hangfire global configuration</param>
        /// <param name="configureOptions">Action to configure the Rollbar integration options</param>
        /// <returns>The configuration instance for method chaining</returns>
        /// <example>
        /// <code>
        /// builder.Services.AddHangfire(config => config.UseRollbar(options => {
        ///     options.CaptureJobArguments = true;
        ///     options.MinimumJobFailureLevel = ErrorLevel.Warning;
        ///     options.ScrubFields = new[] { "password", "secret", "token" };
        ///     options.CaptureSuccessfulJobs = false;
        /// }));
        /// </code>
        /// </example>
        public static IGlobalConfiguration UseRollbar(this IGlobalConfiguration configuration, Action<RollbarHangfireOptions> configureOptions)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            var options = new RollbarHangfireOptions();
            configureOptions(options);
            
            return configuration.UseRollbar(options);
        }

        /// <summary>
        /// Configures Hangfire to use Rollbar with a specific Rollbar instance and options.
        /// This overload is useful when you have multiple Rollbar instances or need to use a specific configuration.
        /// </summary>
        /// <param name="configuration">The Hangfire global configuration</param>
        /// <param name="rollbar">The specific Rollbar instance to use</param>
        /// <param name="options">Configuration options for the Rollbar integration</param>
        /// <returns>The configuration instance for method chaining</returns>
        /// <example>
        /// <code>
        /// var customRollbar = RollbarFactory.CreateNew(customConfig);
        /// builder.Services.AddHangfire(config => config.UseRollbar(customRollbar, new RollbarHangfireOptions()));
        /// </code>
        /// </example>
        public static IGlobalConfiguration UseRollbar(this IGlobalConfiguration configuration, IRollbar rollbar, RollbarHangfireOptions options)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            
            if (rollbar == null)
                throw new ArgumentNullException(nameof(rollbar));
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var filter = new RollbarHangfireFilter(options, rollbar);
            return configuration.UseFilter(filter);
        }

        /// <summary>
        /// Configures Hangfire to use Rollbar with a specific Rollbar instance and configuration delegate.
        /// </summary>
        /// <param name="configuration">The Hangfire global configuration</param>
        /// <param name="rollbar">The specific Rollbar instance to use</param>
        /// <param name="configureOptions">Action to configure the Rollbar integration options</param>
        /// <returns>The configuration instance for method chaining</returns>
        /// <example>
        /// <code>
        /// var customRollbar = RollbarFactory.CreateNew(customConfig);
        /// builder.Services.AddHangfire(config => config.UseRollbar(customRollbar, options => {
        ///     options.CaptureJobArguments = false; // Don't capture args with this instance
        ///     options.MinimumJobFailureLevel = ErrorLevel.Critical;
        /// }));
        /// </code>
        /// </example>
        public static IGlobalConfiguration UseRollbar(this IGlobalConfiguration configuration, IRollbar rollbar, Action<RollbarHangfireOptions> configureOptions)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            
            if (rollbar == null)
                throw new ArgumentNullException(nameof(rollbar));
            
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            var options = new RollbarHangfireOptions();
            configureOptions(options);
            
            return configuration.UseRollbar(rollbar, options);
        }
    }
}