namespace Rollbar.App.Config
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Class AppConfigUtility.
    /// It aids in configuration of Rollbar configuration objects based on content of an app.config file (if any). 
    /// </summary>
    public static class AppConfigUtility
    {
        private static readonly string[] listValueSplitters = new [] { ", ", "; ", " " };

        #region RollbarConfig

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S4136:Method overloads should be grouped together", Justification = "Intentionally split by settings kind.")]
        public static bool LoadAppSettings(RollbarInfrastructureConfig config)
        {
            return AppConfigUtility.LoadAppSettings(config, RollbarConfigSection.GetConfiguration());
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="rollbarConfigSection">The application settings.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(RollbarInfrastructureConfig config, RollbarConfigSection? rollbarConfigSection)
        {
            if (rollbarConfigSection == null)
            {
                return false;
            }

            LoadInfrastructureOptions(config, rollbarConfigSection);
            LoadOfflinePayloadStoreOptions(config, rollbarConfigSection);
            LoadDestinationOptions(config, rollbarConfigSection);
            LoadDeveloperOptions(config, rollbarConfigSection);
            LoadHttpProxyOptions(config, rollbarConfigSection);
            LoadDataSecurityOptions(config, rollbarConfigSection);
            LoadTelemetryOptions(config);

            var validationResults = config.Validate();
            bool configLoadingResult = 
                (validationResults == null) || (validationResults.Count == 0);
            Debug.Assert(configLoadingResult);
            return configLoadingResult;
        }

        private static void LoadInfrastructureOptions(RollbarInfrastructureConfig config, RollbarConfigSection rollbarConfigSection)
        {
            RollbarInfrastructureOptions infrastructureOptions = new();

            if (rollbarConfigSection.MaxReportsPerMinute.HasValue)
            {
                infrastructureOptions.MaxReportsPerMinute = rollbarConfigSection.MaxReportsPerMinute.Value;
            }

            if (rollbarConfigSection.ReportingQueueDepth.HasValue)
            {
                infrastructureOptions.ReportingQueueDepth = rollbarConfigSection.ReportingQueueDepth.Value;
            }

            if (rollbarConfigSection.MaxItems.HasValue)
            {
                infrastructureOptions.MaxItems = rollbarConfigSection.MaxItems.Value;
            }

            if (rollbarConfigSection.CaptureUncaughtExceptions.HasValue)
            {
                infrastructureOptions.CaptureUncaughtExceptions = rollbarConfigSection.CaptureUncaughtExceptions.Value;
            }

            if (rollbarConfigSection.PayloadPostTimeout.HasValue)
            {
                infrastructureOptions.PayloadPostTimeout = rollbarConfigSection.PayloadPostTimeout.Value;
            }

            config.RollbarInfrastructureOptions.Reconfigure(infrastructureOptions);
        }

        private static void LoadOfflinePayloadStoreOptions(RollbarInfrastructureConfig config, RollbarConfigSection rollbarConfigSection)
        {
            RollbarOfflineStoreOptions offlineStoreOptions = new();

            if (rollbarConfigSection.EnableLocalPayloadStore.HasValue)
            {
                offlineStoreOptions.EnableLocalPayloadStore = rollbarConfigSection.EnableLocalPayloadStore.Value;
            }

            if (!string.IsNullOrWhiteSpace(rollbarConfigSection.LocalPayloadStoreFileName))
            {
                offlineStoreOptions.LocalPayloadStoreFileName = rollbarConfigSection.LocalPayloadStoreFileName;
            }

            if (!string.IsNullOrWhiteSpace(rollbarConfigSection.LocalPayloadStoreLocationPath))
            {
                offlineStoreOptions.LocalPayloadStoreLocationPath = rollbarConfigSection.LocalPayloadStoreLocationPath;
            }

            config.RollbarOfflineStoreOptions.Reconfigure(offlineStoreOptions);
        }

        private static void LoadDestinationOptions(RollbarInfrastructureConfig config, RollbarConfigSection rollbarConfigSection)
        {
            if (!string.IsNullOrWhiteSpace(rollbarConfigSection.AccessToken))
            {
                RollbarDestinationOptions destinationOptions = new(rollbarConfigSection.AccessToken);

                if (!string.IsNullOrWhiteSpace(rollbarConfigSection.Environment))
                {
                    destinationOptions.Environment = rollbarConfigSection.Environment;
                }

                if (!string.IsNullOrWhiteSpace(rollbarConfigSection.EndPoint))
                {
                    destinationOptions.EndPoint = rollbarConfigSection.EndPoint;
                }

                config.RollbarLoggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);
            }
        }

        private static void LoadDeveloperOptions(RollbarInfrastructureConfig config, RollbarConfigSection rollbarConfigSection)
        {
            if (rollbarConfigSection.Enabled.HasValue)
            {
                config.RollbarLoggerConfig.RollbarDeveloperOptions.Enabled = rollbarConfigSection.Enabled.Value;
            }

            if (rollbarConfigSection.Transmit.HasValue)
            {
                config.RollbarLoggerConfig.RollbarDeveloperOptions.Transmit = rollbarConfigSection.Transmit.Value;
            }

            if (rollbarConfigSection.RethrowExceptionsAfterReporting.HasValue)
            {
                config.RollbarLoggerConfig.RollbarDeveloperOptions.RethrowExceptionsAfterReporting = rollbarConfigSection.RethrowExceptionsAfterReporting.Value;
            }

            if (rollbarConfigSection.LogLevel.HasValue)
            {
                config.RollbarLoggerConfig.RollbarDeveloperOptions.LogLevel = rollbarConfigSection.LogLevel.Value;
            }
        }

        private static void LoadHttpProxyOptions(RollbarInfrastructureConfig config, RollbarConfigSection rollbarConfigSection)
        {
            if (!string.IsNullOrWhiteSpace(rollbarConfigSection.ProxyAddress))
            {
                HttpProxyOptions httpProxyOptions = new(rollbarConfigSection.ProxyAddress);

                if (!string.IsNullOrWhiteSpace(rollbarConfigSection.ProxyUsername))
                {
                    httpProxyOptions.ProxyUsername = rollbarConfigSection.ProxyUsername;
                }

                if (!string.IsNullOrWhiteSpace(rollbarConfigSection.ProxyPassword))
                {
                    httpProxyOptions.ProxyPassword = rollbarConfigSection.ProxyPassword;
                }

                config.RollbarLoggerConfig.HttpProxyOptions.Reconfigure(httpProxyOptions);
            }
        }

        private static void LoadDataSecurityOptions(RollbarInfrastructureConfig config, RollbarConfigSection rollbarConfigSection)
        {
            RollbarDataSecurityOptions dataSecurityOptions = new();

            if (rollbarConfigSection.ScrubFields != null && rollbarConfigSection.ScrubFields.Length > 0)
            {
                dataSecurityOptions.ScrubFields =
                    string.IsNullOrEmpty(rollbarConfigSection.ScrubFields) ? Array.Empty<string>()
                    : rollbarConfigSection.ScrubFields.Split(listValueSplitters, StringSplitOptions.RemoveEmptyEntries);
            }

            if (rollbarConfigSection.ScrubSafelistFields != null && rollbarConfigSection.ScrubSafelistFields.Length > 0)
            {
                dataSecurityOptions.ScrubSafelistFields =
                    string.IsNullOrEmpty(rollbarConfigSection.ScrubSafelistFields) ? Array.Empty<string>()
                    : rollbarConfigSection.ScrubSafelistFields.Split(listValueSplitters, StringSplitOptions.RemoveEmptyEntries);
            }

            if (rollbarConfigSection.PersonDataCollectionPolicies.HasValue)
            {
                dataSecurityOptions.PersonDataCollectionPolicies = rollbarConfigSection.PersonDataCollectionPolicies.Value;
            }

            if (rollbarConfigSection.IpAddressCollectionPolicy.HasValue)
            {
                dataSecurityOptions.IpAddressCollectionPolicy = rollbarConfigSection.IpAddressCollectionPolicy.Value;
            }

            config.RollbarLoggerConfig.RollbarDataSecurityOptions.Reconfigure(dataSecurityOptions);
        }

        private static void LoadTelemetryOptions(RollbarInfrastructureConfig config)
        {
            RollbarTelemetryOptions telemetryOptions = new();
            if (AppConfigUtility.LoadAppSettings(telemetryOptions))
            {
                config.RollbarTelemetryOptions.Reconfigure(telemetryOptions);
            }
        }


        #endregion RollbarConfig

        #region TelemetryConfig

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="telemetryConfig">The configuration.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(RollbarTelemetryOptions telemetryConfig)
        {
            return AppConfigUtility.LoadAppSettings(telemetryConfig, RollbarTelemetryConfigSection.GetConfiguration());
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="telemetryConfig">The configuration.</param>
        /// <param name="telemetryConfigSection">The application settings.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(RollbarTelemetryOptions telemetryConfig, RollbarTelemetryConfigSection? telemetryConfigSection)
        {
            if (telemetryConfigSection == null)
            {
                return false;
            }

            if (telemetryConfigSection.TelemetryEnabled.HasValue)
            {
                telemetryConfig.TelemetryEnabled = telemetryConfigSection.TelemetryEnabled.Value;
            }
            if (telemetryConfigSection.TelemetryQueueDepth.HasValue)
            {
                telemetryConfig.TelemetryQueueDepth = telemetryConfigSection.TelemetryQueueDepth.Value;
            }
            if (telemetryConfigSection.TelemetryAutoCollectionTypes.HasValue)
            {
                telemetryConfig.TelemetryAutoCollectionTypes = telemetryConfigSection.TelemetryAutoCollectionTypes.Value;
            }
            if (telemetryConfigSection.TelemetryAutoCollectionInterval.HasValue)
            {
                telemetryConfig.TelemetryAutoCollectionInterval = telemetryConfigSection.TelemetryAutoCollectionInterval.Value;
            }

            return true;
        }
    }

    #endregion TelemetryConfig
}
