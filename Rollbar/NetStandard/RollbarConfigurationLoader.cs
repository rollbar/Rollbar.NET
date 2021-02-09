namespace Rollbar.NetStandard
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    using Rollbar.Common;
    using Rollbar.Telemetry;

    public class RollbarConfigurationLoader 
        : IRollbarConfigurationLoader
    {
        private static readonly TraceSource traceSource = new TraceSource(typeof(RollbarConfigurationLoader).FullName);

        private readonly static string[] knownAssemblyNames = {
            "Rollbar.App.Config",
            "Rollbar.AppSettings.Json",
            };

        private readonly static string knownLoaderTypeName = nameof(RollbarConfigurationLoader);

        private readonly static Type configurationLoaderType = null;

        private readonly IRollbarConfigurationLoader _loader = null;

        /// <summary>
        /// Initializes static members of the <see cref="RollbarConfigurationLoader"/> class.
        /// </summary>
        static RollbarConfigurationLoader()
        {
            string sdkAssembliesPath = RuntimeEnvironmentUtility.GetSdkRuntimeLocationPath();
            traceSource.TraceInformation($"Rollbar SDK runtime location: {sdkAssembliesPath}");
            Assembly assembly = null;
            foreach(var name in RollbarConfigurationLoader.knownAssemblyNames)
            {
                string dllName = $"{name}.dll";
                string dllFullPath = Path.Combine(sdkAssembliesPath, dllName);
                if (!File.Exists(dllFullPath))
                {
                    continue;
                }

                traceSource.TraceInformation($"Located optional Rollbar SDK assembly: {dllFullPath}");
                assembly = Assembly.LoadFrom(dllFullPath);
                if (assembly != null)
                {
                    traceSource.TraceInformation($"Loaded optional Rollbar SDK assembly: {dllFullPath}");
                    string loaderTypeFullName = $"{name}.{RollbarConfigurationLoader.knownLoaderTypeName}";
                    RollbarConfigurationLoader.configurationLoaderType = assembly.GetType(loaderTypeFullName, false);
                    if (RollbarConfigurationLoader.configurationLoaderType != null)
                    {
                        traceSource.TraceInformation($"Using optional Rollbar SDK configuration loader: {RollbarConfigurationLoader.configurationLoaderType.FullName}");
                        // We assume our users prefer one of the known ways to store configurations (if any). 
                        // So, we only need the first known loader found (if any):
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarConfigurationLoader"/> class.
        /// </summary>
        public RollbarConfigurationLoader()
        {
            if (RollbarConfigurationLoader.configurationLoaderType != null)
            {
               var ctor = RollbarConfigurationLoader.configurationLoaderType.GetConstructor(new Type[0]);
               this._loader = (IRollbarConfigurationLoader) ctor?.Invoke(new object[0]);
            }
        }

        /// <summary>
        /// Loads the povided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        public bool Load(RollbarConfig config)
        {
            return (this._loader != null && this._loader.Load(config));
        }

        /// <summary>
        /// Loads the povided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        public bool Load(TelemetryConfig config)
        {
            return (this._loader != null && this._loader.Load(config));
        }

        /// <summary>
        /// Loads the rollbar configuration.
        /// </summary>
        /// <returns>IRollbarConfig or null if no configuration store was found.</returns>
        public IRollbarConfig LoadRollbarConfig()
        {
            if (this._loader != null) 
                return this._loader.LoadRollbarConfig();
            else
                return null;
        }

        /// <summary>
        /// Loads the telemetry configuration.
        /// </summary>
        /// <returns>ITelemetryConfig or null if no configuration store was found.</returns>
        public ITelemetryConfig LoadTelemetryConfig()
        {
            if (this._loader != null) 
                return this._loader.LoadTelemetryConfig();
            else
                return null;
        }

        /// <summary>
        /// Loads the rollbar configuration.
        /// </summary>
        /// <param name="configFileName">Name of the configuration file.</param>
        /// <param name="configFilePath">The configuration file path.</param>
        /// <returns>IRollbarConfig or null if no configuration store was found.</returns>
        //public IRollbarConfig LoadRollbarConfig(string configFileName,string configFilePath = null)
        //{
        //    if (this._loader != null) 
        //        return this._loader.LoadRollbarConfig(configFileName, configFilePath);
        //    else
        //        return null;
        //}

        /// <summary>
        /// Loads the telemetry configuration.
        /// </summary>
        /// <param name="configFileName">Name of the configuration file.</param>
        /// <param name="configFilePath">The configuration file path.</param>
        /// <returns>ITelemetryConfig or null if no configuration store was found.</returns>
        //public ITelemetryConfig LoadTelemetryConfig(string configFileName,string configFilePath = null)
        //{
        //    if (this._loader != null) 
        //        return this._loader.LoadTelemetryConfig(configFileName, configFilePath);
        //    else
        //        return null;
        //}
    }
}
