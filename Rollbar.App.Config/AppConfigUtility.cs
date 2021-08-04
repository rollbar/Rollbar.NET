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
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
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
        public static bool LoadAppSettings(RollbarInfrastructureConfig config, RollbarConfigSection rollbarConfigSection)
        {
            if (rollbarConfigSection == null)
            {
                return false;
            }

            // infrastructure options:
            //////////////////////////
            
            RollbarInfrastructureOptions infrastructureOptions = new RollbarInfrastructureOptions();

            if(rollbarConfigSection.MaxReportsPerMinute.HasValue)
            {
                infrastructureOptions.MaxReportsPerMinute = rollbarConfigSection.MaxReportsPerMinute.Value;
            }

            if(rollbarConfigSection.ReportingQueueDepth.HasValue)
            {
                infrastructureOptions.ReportingQueueDepth = rollbarConfigSection.ReportingQueueDepth.Value;
            }

            if(rollbarConfigSection.MaxItems.HasValue)
            {
                infrastructureOptions.MaxItems = rollbarConfigSection.MaxItems.Value;
            }

            if(rollbarConfigSection.CaptureUncaughtExceptions.HasValue)
            {
                infrastructureOptions.CaptureUncaughtExceptions = rollbarConfigSection.CaptureUncaughtExceptions.Value;
            }

            if(rollbarConfigSection.PayloadPostTimeout.HasValue)
            {
                infrastructureOptions.PayloadPostTimeout = rollbarConfigSection.PayloadPostTimeout.Value;
            }

            config.RollbarInfrastructureOptions.Reconfigure(infrastructureOptions);

            // telemetry options:
            /////////////////////

            RollbarTelemetryOptions telemetryOptions = new RollbarTelemetryOptions();
            if(AppConfigUtility.LoadAppSettings(telemetryOptions))
            {
                config.RollbarTelemetryOptions.Reconfigure(telemetryOptions);
            }

            //if(telemetryConfigSection.TelemetryEnabled.HasValue)
            //{
            //    telemetryOptions.TelemetryEnabled = telemetryConfigSection.TelemetryEnabled.Value;
            //}
            //if(telemetryConfigSection.TelemetryQueueDepth.HasValue)
            //{
            //    telemetryOptions.TelemetryQueueDepth = telemetryConfigSection.TelemetryQueueDepth.Value;
            //}
            //if(telemetryConfigSection.TelemetryAutoCollectionTypes.HasValue)
            //{
            //    telemetryOptions.TelemetryAutoCollectionTypes = telemetryConfigSection.TelemetryAutoCollectionTypes.Value;
            //}
            //if(telemetryConfigSection.TelemetryAutoCollectionInterval.HasValue)
            //{
            //    telemetryOptions.TelemetryAutoCollectionInterval = telemetryConfigSection.TelemetryAutoCollectionInterval.Value;
            //}

            //config.RollbarTelemetryOptions.Reconfigure(telemetryOptions);

            // offline payload store options:
            /////////////////////////////////

            RollbarOfflineStoreOptions offlineStoreOptions = new RollbarOfflineStoreOptions();

            if(rollbarConfigSection.EnableLocalPayloadStore.HasValue)
            {
                offlineStoreOptions.EnableLocalPayloadStore = rollbarConfigSection.EnableLocalPayloadStore.Value;
            }

            if(!string.IsNullOrWhiteSpace(rollbarConfigSection.LocalPayloadStoreFileName))
            {
                offlineStoreOptions.LocalPayloadStoreFileName = rollbarConfigSection.LocalPayloadStoreFileName;
            }

            if(!string.IsNullOrWhiteSpace(rollbarConfigSection.LocalPayloadStoreLocationPath))
            {
                offlineStoreOptions.LocalPayloadStoreLocationPath = rollbarConfigSection.LocalPayloadStoreLocationPath;
            }

            config.RollbarOfflineStoreOptions.Reconfigure(offlineStoreOptions);

            // logger destination options:
            //////////////////////////////

            if(!string.IsNullOrWhiteSpace(rollbarConfigSection.AccessToken))
            {
                RollbarDestinationOptions destinationOptions = 
                    new RollbarDestinationOptions(rollbarConfigSection.AccessToken);

                if(!string.IsNullOrWhiteSpace(rollbarConfigSection.Environment))
                {
                    destinationOptions.Environment = rollbarConfigSection.Environment;
                }

                if(!string.IsNullOrWhiteSpace(rollbarConfigSection.EndPoint))
                {
                    destinationOptions.EndPoint = rollbarConfigSection.EndPoint;
                }

                config.RollbarLoggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);
            }

            // logger developer options:
            ////////////////////////////
            
            if(rollbarConfigSection.Enabled.HasValue)
            {
                config.RollbarLoggerConfig.RollbarDeveloperOptions.Enabled = rollbarConfigSection.Enabled.Value;
            }

            if(rollbarConfigSection.Transmit.HasValue)
            {
                config.RollbarLoggerConfig.RollbarDeveloperOptions.Transmit = rollbarConfigSection.Transmit.Value;
            }

            if(rollbarConfigSection.RethrowExceptionsAfterReporting.HasValue)
            {
                config.RollbarLoggerConfig.RollbarDeveloperOptions.RethrowExceptionsAfterReporting = rollbarConfigSection.RethrowExceptionsAfterReporting.Value;
            }

            if(rollbarConfigSection.LogLevel.HasValue)
            {
                config.RollbarLoggerConfig.RollbarDeveloperOptions.LogLevel = rollbarConfigSection.LogLevel.Value;
            }

            // logger HTTP proxy options:
            /////////////////////////////

            if(!string.IsNullOrWhiteSpace(rollbarConfigSection.ProxyAddress))
            {
                HttpProxyOptions httpProxyOptions = new HttpProxyOptions(rollbarConfigSection.ProxyAddress);

                if(!string.IsNullOrWhiteSpace(rollbarConfigSection.ProxyUsername))
                {
                    httpProxyOptions.ProxyUsername = rollbarConfigSection.ProxyUsername;
                }

                if(!string.IsNullOrWhiteSpace(rollbarConfigSection.ProxyPassword))
                {
                    httpProxyOptions.ProxyPassword = rollbarConfigSection.ProxyPassword;
                }

                config.RollbarLoggerConfig.HttpProxyOptions.Reconfigure(httpProxyOptions);
            }

            // logger data security options:
            ////////////////////////////////

            RollbarDataSecurityOptions dataSecurityOptions = new RollbarDataSecurityOptions();

            if(rollbarConfigSection.ScrubFields != null && rollbarConfigSection.ScrubFields.Length > 0)
            {
                dataSecurityOptions.ScrubFields =
                    string.IsNullOrEmpty(rollbarConfigSection.ScrubFields) ? new string[0]
                    : rollbarConfigSection.ScrubFields.Split(listValueSplitters, StringSplitOptions.RemoveEmptyEntries);
            }

            if(rollbarConfigSection.ScrubSafelistFields != null && rollbarConfigSection.ScrubSafelistFields.Length > 0)
            {
                dataSecurityOptions.ScrubSafelistFields =
                    string.IsNullOrEmpty(rollbarConfigSection.ScrubSafelistFields) ? new string[0]
                    : rollbarConfigSection.ScrubSafelistFields.Split(listValueSplitters, StringSplitOptions.RemoveEmptyEntries);
            }

            if(rollbarConfigSection.PersonDataCollectionPolicies.HasValue)
            {
                dataSecurityOptions.PersonDataCollectionPolicies = rollbarConfigSection.PersonDataCollectionPolicies.Value;
            }

            if(rollbarConfigSection.IpAddressCollectionPolicy.HasValue)
            {
                dataSecurityOptions.IpAddressCollectionPolicy = rollbarConfigSection.IpAddressCollectionPolicy.Value;
            }

            config.RollbarLoggerConfig.RollbarDataSecurityOptions.Reconfigure(dataSecurityOptions);


            var validationResults = config.Validate();
            bool configLoadingResult = 
                (validationResults == null) || (validationResults.Count == 0);
            Debug.Assert(configLoadingResult);
            return configLoadingResult;
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
        public static bool LoadAppSettings(RollbarTelemetryOptions telemetryConfig, RollbarTelemetryConfigSection telemetryConfigSection)
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
