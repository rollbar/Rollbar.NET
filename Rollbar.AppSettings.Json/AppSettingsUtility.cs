namespace Rollbar.AppSettings.Json
{
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Rollbar.Diagnostics;

    /// <summary>
    /// A utility class aiding in reading in settings from the .NET Core appsettings.json files.
    /// </summary>
    public static class AppSettingsUtility
    {

        #region RollbarConfig

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(RollbarInfrastructureConfig config)
        {
            Assumption.AssertNotNull(config, nameof(config));

            IConfiguration? appSettingsConfig = AppSettingsUtility.LoadAppSettings();
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtility.LoadAppSettings(config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(RollbarInfrastructureConfig config, string appSettingsFileName)
        {
            Assumption.AssertNotNull(config, nameof(config));

            IConfiguration? appSettingsConfig = AppSettingsUtility.LoadAppSettings(appSettingsFileName);
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtility.LoadAppSettings(config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFolderPath">The application settings folder path.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(RollbarInfrastructureConfig config, string appSettingsFolderPath, string appSettingsFileName)
        {
            Assumption.AssertNotNull(config, nameof(config));

            IConfiguration? appSettingsConfig = AppSettingsUtility.LoadAppSettings(appSettingsFolderPath, appSettingsFileName);
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtility.LoadAppSettings(config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettings">The application settings.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(RollbarInfrastructureConfig config, IConfiguration appSettings)
        {
            Assumption.AssertNotNull(config, nameof(config));

            const string rollbarAppConfigSectionName = "Rollbar";

            return AppSettingsUtility.LoadAppSettings(config, rollbarAppConfigSectionName, appSettings);
        }

        #endregion RollbarConfig

        #region TelemetryConfig

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(RollbarTelemetryOptions config)
        {
            Assumption.AssertNotNull(config, nameof(config));

            IConfiguration? appSettingsConfig = AppSettingsUtility.LoadAppSettings();
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtility.LoadAppSettings(config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(RollbarTelemetryOptions config, string appSettingsFileName)
        {
            Assumption.AssertNotNull(config, nameof(config));

            IConfiguration? appSettingsConfig = AppSettingsUtility.LoadAppSettings(appSettingsFileName);
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtility.LoadAppSettings(config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFolderPath">The application settings folder path.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(RollbarTelemetryOptions config, string appSettingsFolderPath, string appSettingsFileName)
        {
            Assumption.AssertNotNull(config, nameof(config));

            IConfiguration? appSettingsConfig = AppSettingsUtility.LoadAppSettings(appSettingsFolderPath, appSettingsFileName);
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtility.LoadAppSettings(config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettings">The application settings.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(RollbarTelemetryOptions config, IConfiguration appSettings)
        {
            Assumption.AssertNotNull(config, nameof(config));

            const string rollbarAppConfigSectionName = "RollbarTelemetry";

            return AppSettingsUtility.LoadAppSettings(config, rollbarAppConfigSectionName, appSettings);
        }

        #endregion TelemetryConfig

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <returns>Microsoft.Extensions.Configuration.IConfiguration.</returns>
        private static IConfiguration? LoadAppSettings()
        {
            return AppSettingsUtility.LoadAppSettings("appsettings.json");
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        /// <returns>Microsoft.Extensions.Configuration.IConfiguration.</returns>
        private static IConfiguration? LoadAppSettings(string appSettingsFileName)
        {
            return AppSettingsUtility.LoadAppSettings(
                Directory.GetCurrentDirectory(),
                appSettingsFileName
                );
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        /// <returns>Microsoft.Extensions.Configuration.IConfiguration.</returns>
        private static IConfiguration? LoadAppSettings(string folderPath, string appSettingsFileName)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.WriteLine($"Folder: {folderPath} does not exist...");
                return null;
            }

            string fileFullName = Path.Combine(folderPath, appSettingsFileName);
            if (!File.Exists(fileFullName))
            {
                //NOTE: the following line causes the RollbarTraceListener into infinite creation loop and subsequent stck overflow:
                //Debug.WriteLine($"File: {fileFullName} does not exist...");

                return null;
            }

            IConfiguration appConfiguration = new ConfigurationBuilder()
                .SetBasePath(folderPath)
                .AddJsonFile(appSettingsFileName)
                .Build();

            return appConfiguration;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <typeparam name="TSection">The type of the t section.</typeparam>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="appSettings">The application settings.</param>
        /// <returns>TSection.</returns>
        private static TSection LoadAppSettings<TSection>(string sectionName, IConfiguration appSettings)
        {
            return appSettings.GetSection(sectionName).Get<TSection>();
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <typeparam name="TSection">The type of the t section.</typeparam>
        /// <param name="section">The section.</param>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="appSettings">The application settings.</param>
        /// <returns>false when the specified section was not found, otherwise true.</returns>
        private static bool LoadAppSettings<TSection>(TSection section, string sectionName, IConfiguration appSettings)
        {
            IConfigurationSection configurationSection = appSettings.GetSection(sectionName);
            if (configurationSection == null)
            {
                return false;
            }

            configurationSection.Bind(section);
            return true;
        }
    }
}
