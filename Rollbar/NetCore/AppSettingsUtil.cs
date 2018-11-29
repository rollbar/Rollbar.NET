#if NETCOREAPP || NETSTANDARD

namespace Rollbar.NetCore
{
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Rollbar.Diagnostics;
    using Rollbar.Telemetry;

    /// <summary>
    /// A utility class aiding in reading in settings from the .NET Core AppSettings files.
    /// </summary>
    public static class AppSettingsUtil
    {

        #region RollbarConfig

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void LoadAppSettings(ref RollbarConfig config)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings();
            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        public static void LoadAppSettings(ref RollbarConfig config, string appSettingsFileName)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings(appSettingsFileName);
            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFolderPath">The application settings folder path.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        public static void LoadAppSettings(ref RollbarConfig config, string appSettingsFolderPath, string appSettingsFileName)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings(appSettingsFolderPath, appSettingsFileName);
            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettings">The application settings.</param>
        public static void LoadAppSettings(ref RollbarConfig config, IConfiguration appSettings)
        {
            const string rollbarAppConfigSectionName = "Rollbar";

            AppSettingsUtil.LoadAppSettings(ref config, rollbarAppConfigSectionName, appSettings);
        }

        #endregion RollbarConfig

        #region TelemetryConfig

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void LoadAppSettings(ref TelemetryConfig config)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings();
            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        public static void LoadAppSettings(ref TelemetryConfig config, string appSettingsFileName)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings(appSettingsFileName);
            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettingsFolderPath">The application settings folder path.</param>
        /// <param name="appSettingsFileName">Name of the application settings file.</param>
        public static void LoadAppSettings(ref TelemetryConfig config, string appSettingsFolderPath, string appSettingsFileName)
        {
            IConfiguration appSettingsConfig = AppSettingsUtil.LoadAppSettings(appSettingsFolderPath, appSettingsFileName);
            AppSettingsUtil.LoadAppSettings(ref config, appSettingsConfig);
        }

        /// <summary>
        /// Loads the application settings.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="appSettings">The application settings.</param>
        public static void LoadAppSettings(ref TelemetryConfig config, IConfiguration appSettings)
        {
            const string rollbarAppConfigSectionName = "RollbarTelemetry";

            AppSettingsUtil.LoadAppSettings(ref config, rollbarAppConfigSectionName, appSettings);
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
            Assumption.AssertTrue(
                Directory.Exists(folderPath), 
                nameof(folderPath)
                );
            Assumption.AssertTrue(
                File.Exists(Path.Combine(folderPath, appSettingsFileName)), 
                nameof(appSettingsFileName)
                );

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

        private static void LoadAppSettings<TSection>(ref TSection section, string sectionName, IConfiguration appSettings)
        {
            appSettings.GetSection(sectionName).Bind(section);
        }
    }
}

#endif