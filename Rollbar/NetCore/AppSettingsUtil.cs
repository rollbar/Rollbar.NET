#if NETCOREAPP || NETSTANDARD

namespace Rollbar.NetCore
{
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Rollbar.Telemetry;

    /// <summary>
    /// A utility class aiding in reading in settings from the .NET Core appsettings.json files.
    /// </summary>
    public static class AppSettingsUtil
    {

        #region RollbarConfig

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(ref RollbarConfig config)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings();
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(ref RollbarConfig config, string appSettingsFileName)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings(appSettingsFileName);
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFolderPath">The application settings folder path.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(ref RollbarConfig config, string appSettingsFolderPath, string appSettingsFileName)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings(appSettingsFolderPath, appSettingsFileName);
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettings">The application settings.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(ref RollbarConfig config, IConfiguration appSettings)
        {
            const string rollbarAppConfigSectionName = "Rollbar";

            return AppSettingsUtil.LoadAppSettings(ref config, rollbarAppConfigSectionName, appSettings);
        }

        #endregion RollbarConfig

        #region TelemetryConfig

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(ref TelemetryConfig config)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings();
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(ref TelemetryConfig config, string appSettingsFileName)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings(appSettingsFileName);
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFolderPath">The application settings folder path.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(ref TelemetryConfig config, string appSettingsFolderPath, string appSettingsFileName)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings(appSettingsFolderPath, appSettingsFileName);
            if (appSettingsConfig == null)
            {
                return false;
            }

            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
            return true;
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettings">The application settings.</param>
        /// <returns>false when the configuration was not found, otherwise true.</returns>
        public static bool LoadAppSettings(ref TelemetryConfig config, IConfiguration appSettings)
        {
            const string rollbarAppConfigSectionName = "RollbarTelemetry";

            return AppSettingsUtil.LoadAppSettings(ref config, rollbarAppConfigSectionName, appSettings);
        }

        #endregion TelemetryConfig

        private static IConfiguration LoadAppSettings()
        {
            return AppSettingsUtil.LoadAppSettings("appsettings.json");
        }

        private static IConfiguration LoadAppSettings(string appSettingsFileName)
        {
            return AppSettingsUtil.LoadAppSettings(
                Directory.GetCurrentDirectory(), 
                appSettingsFileName
                );
        }

        private static IConfiguration LoadAppSettings(string folderPath, string appSettingsFileName)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.WriteLine($"Folder: {folderPath} does not exist...");
                return null;
            }

            string fileFullName = Path.Combine(folderPath, appSettingsFileName);
            if (!File.Exists(fileFullName))
            {
                Debug.WriteLine($"File: {fileFullName} does not exist...");
                return null;
            }

            IConfiguration appConfiguration = new ConfigurationBuilder()
                .SetBasePath(folderPath)
                .AddJsonFile(appSettingsFileName)
                .Build();

            return appConfiguration;
        }

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
        private static bool LoadAppSettings<TSection>(ref TSection section, string sectionName, IConfiguration appSettings)
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

#endif