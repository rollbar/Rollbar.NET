using System;
using System.Collections.Generic;

namespace Rollbar
{
    /// <summary>
    /// Simplified setup class for Rollbar that provides easy 2-3 line initialization.
    /// This is a convenience wrapper around the existing Rollbar infrastructure.
    /// </summary>
    public static class RollbarEasySetup
    {
        /// <summary>
        /// Initialize Rollbar with just an access token (minimal setup).
        /// Uses sensible defaults for production environment.
        /// </summary>
        /// <param name="accessToken">Your Rollbar access token</param>
        /// <example>
        /// RollbarEasySetup.Init("your-access-token");
        /// RollbarLocator.RollbarInstance.Error(exception);
        /// </example>
        public static void Init(string accessToken)
        {
            Init(accessToken, "production");
        }

        /// <summary>
        /// Initialize Rollbar with access token and environment.
        /// </summary>
        /// <param name="accessToken">Your Rollbar access token</param>
        /// <param name="environment">Environment name (e.g., "production", "staging", "development")</param>
        /// <example>
        /// RollbarEasySetup.Init("your-access-token", "development");
        /// RollbarLocator.RollbarInstance.Error(exception);
        /// </example>
        public static void Init(string accessToken, string environment)
        {
            var config = new RollbarInfrastructureConfig(accessToken, environment);
            RollbarInfrastructure.Instance.Init(config);
        }

        /// <summary>
        /// Initialize Rollbar with simplified options using a configuration delegate.
        /// </summary>
        /// <param name="configure">Configuration delegate for advanced options</param>
        /// <example>
        /// RollbarEasySetup.Init(options => {
        ///     options.AccessToken = "your-token";
        ///     options.Environment = "development";
        ///     options.ScrubFields = new[] { "password", "secret" };
        ///     options.PersonId = "user123";
        /// });
        /// </example>
        public static void Init(Action<SimpleRollbarOptions> configure)
        {
            var options = new SimpleRollbarOptions();
            configure(options);

            var config = new RollbarInfrastructureConfig(options.AccessToken, options.Environment);
            
            // Apply simple options to the full configuration
            options.ApplyTo(config);
            
            RollbarInfrastructure.Instance.Init(config);
        }

        /// <summary>
        /// Capture an exception with Rollbar (convenience method).
        /// Must call Init() first.
        /// </summary>
        /// <param name="exception">The exception to capture</param>
        /// <param name="customData">Optional custom data to include</param>
        public static void CaptureException(Exception exception, IDictionary<string, object?>? customData = null)
        {
            RollbarLocator.RollbarInstance.Error(exception, customData);
        }

        /// <summary>
        /// Capture a message with Rollbar (convenience method).
        /// Must call Init() first.
        /// </summary>
        /// <param name="message">The message to capture</param>
        /// <param name="level">The error level (defaults to Info)</param>
        /// <param name="customData">Optional custom data to include</param>
        public static void CaptureMessage(string message, ErrorLevel level = ErrorLevel.Info, IDictionary<string, object?>? customData = null)
        {
            RollbarLocator.RollbarInstance.Log(level, message, customData);
        }

        /// <summary>
        /// Flush any pending messages and wait for them to be sent.
        /// Useful for console applications before shutdown.
        /// </summary>
        /// <param name="timeout">Maximum time to wait (defaults to 5 seconds)</param>
        public static void Flush(TimeSpan? timeout = null)
        {
            if (RollbarInfrastructure.Instance?.QueueController != null)
            {
                RollbarInfrastructure.Instance.QueueController.FlushQueues();
            }
        }
    }

    /// <summary>
    /// Simplified configuration options for Rollbar.
    /// Provides easy access to the most commonly used settings.
    /// </summary>
    public class SimpleRollbarOptions
    {
        /// <summary>
        /// Your Rollbar access token (required).
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Environment name (defaults to "production").
        /// </summary>
        public string Environment { get; set; } = "production";

        /// <summary>
        /// Minimum log level to capture (defaults to Info).
        /// </summary>
        public ErrorLevel MinLevel { get; set; } = ErrorLevel.Info;

        /// <summary>
        /// Fields to scrub from payloads for security (e.g., "password", "secret").
        /// </summary>
        public string[]? ScrubFields { get; set; }

        /// <summary>
        /// User ID for associating errors with users.
        /// </summary>
        public string? PersonId { get; set; }

        /// <summary>
        /// Username for associating errors with users.
        /// </summary>
        public string? PersonUsername { get; set; }

        /// <summary>
        /// Email for associating errors with users.
        /// </summary>
        public string? PersonEmail { get; set; }

        /// <summary>
        /// Whether to enable Rollbar (defaults to true).
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether to transmit data to Rollbar (defaults to true).
        /// Set to false for testing.
        /// </summary>
        public bool Transmit { get; set; } = true;

        /// <summary>
        /// Custom endpoint URL (optional, uses Rollbar default if not set).
        /// </summary>
        public string? EndPoint { get; set; }

        /// <summary>
        /// Host/server name for identifying the source of errors.
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Code version/release identifier (e.g., "1.2.3", "v2.1.0", git commit hash).
        /// </summary>
        public string? CodeVersion { get; set; }

        /// <summary>
        /// Source control branch name (e.g., "main", "develop", "feature/xyz").
        /// </summary>
        public string? Branch { get; set; }

        /// <summary>
        /// Fluent API for setting scrub fields.
        /// </summary>
        /// <param name="fields">Fields to scrub</param>
        /// <returns>This options instance for chaining</returns>
        public SimpleRollbarOptions WithScrubFields(params string[] fields)
        {
            ScrubFields = fields;
            return this;
        }

        /// <summary>
        /// Fluent API for setting person information.
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <param name="username">Username (optional)</param>
        /// <param name="email">Email (optional)</param>
        /// <returns>This options instance for chaining</returns>
        public SimpleRollbarOptions WithPerson(string id, string? username = null, string? email = null)
        {
            PersonId = id;
            PersonUsername = username;
            PersonEmail = email;
            return this;
        }

        /// <summary>
        /// Fluent API for setting deployment information.
        /// </summary>
        /// <param name="codeVersion">Code version/release identifier</param>
        /// <param name="branch">Source control branch name (optional)</param>
        /// <returns>This options instance for chaining</returns>
        public SimpleRollbarOptions WithDeployment(string codeVersion, string? branch = null)
        {
            CodeVersion = codeVersion;
            if (!string.IsNullOrEmpty(branch))
            {
                Branch = branch;
            }
            return this;
        }

        /// <summary>
        /// Fluent API for setting host/server information.
        /// </summary>
        /// <param name="host">Host/server name</param>
        /// <returns>This options instance for chaining</returns>
        public SimpleRollbarOptions WithHost(string host)
        {
            Host = host;
            return this;
        }

        /// <summary>
        /// Apply these simplified options to the full Rollbar infrastructure configuration.
        /// </summary>
        /// <param name="config">The infrastructure configuration to modify</param>
        internal void ApplyTo(RollbarInfrastructureConfig config)
        {
            // Apply basic settings to developer options
            var devOptions = config.RollbarLoggerConfig.RollbarDeveloperOptions as RollbarDeveloperOptions;
            if (devOptions != null)
            {
                devOptions.LogLevel = MinLevel;
                devOptions.Enabled = Enabled;
                devOptions.Transmit = Transmit;
            }

            // Apply endpoint if specified
            if (!string.IsNullOrEmpty(EndPoint))
            {
                var destOptions = config.RollbarLoggerConfig.RollbarDestinationOptions as RollbarDestinationOptions;
                if (destOptions != null)
                {
                    destOptions.EndPoint = EndPoint;
                }
            }

            // Apply scrub fields if specified
            if (ScrubFields?.Length > 0)
            {
                var securityOptions = config.RollbarLoggerConfig.RollbarDataSecurityOptions as RollbarDataSecurityOptions;
                if (securityOptions != null)
                {
                    securityOptions.ScrubFields = ScrubFields;
                }
            }

            // Apply person information if specified
            if (!string.IsNullOrEmpty(PersonId))
            {
                var person = new DTOs.Person(PersonId!)
                {
                    UserName = PersonUsername,
                    Email = PersonEmail
                };

                var additionOptions = config.RollbarLoggerConfig.RollbarPayloadAdditionOptions as RollbarPayloadAdditionOptions;
                if (additionOptions != null)
                {
                    additionOptions.Person = person;
                }
            }

            // Apply code version if specified
            if (!string.IsNullOrEmpty(CodeVersion))
            {
                var additionOptions = config.RollbarLoggerConfig.RollbarPayloadAdditionOptions as RollbarPayloadAdditionOptions;
                if (additionOptions != null)
                {
                    additionOptions.CodeVersion = CodeVersion;
                }
            }

            // Apply server information (host and branch) if specified
            if (!string.IsNullOrEmpty(Host) || !string.IsNullOrEmpty(Branch))
            {
                var additionOptions = config.RollbarLoggerConfig.RollbarPayloadAdditionOptions as RollbarPayloadAdditionOptions;
                if (additionOptions != null)
                {
                    // Create or update server information
                    if (additionOptions.Server == null)
                    {
                        additionOptions.Server = new DTOs.Server();
                    }

                    if (!string.IsNullOrEmpty(Host))
                    {
                        additionOptions.Server.Host = Host;
                    }

                    if (!string.IsNullOrEmpty(Branch))
                    {
                        additionOptions.Server.Branch = Branch;
                    }
                }
            }
        }
    }
}