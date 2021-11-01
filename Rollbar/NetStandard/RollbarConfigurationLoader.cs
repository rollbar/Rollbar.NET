namespace Rollbar.NetStandard
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    using Rollbar.Common;

    /// <summary>
    /// Class RollbarConfigurationLoader.
    /// Implements the <see cref="Rollbar.NetStandard.IRollbarConfigurationLoader" />
    /// </summary>
    /// <seealso cref="Rollbar.NetStandard.IRollbarConfigurationLoader" />
    public class RollbarConfigurationLoader 
        : IRollbarConfigurationLoader
    {
        /// <summary>
        /// The trace source
        /// </summary>
        private static readonly TraceSource traceSource = new TraceSource(typeof(RollbarConfigurationLoader).FullName ?? "RollbarConfigurationLoader");

        /// <summary>
        /// The known assembly names
        /// </summary>
        private readonly static string[] knownAssemblyNames = {
            "Rollbar.App.Config",
            "Rollbar.AppSettings.Json",
            };

        /// <summary>
        /// The known loader type name
        /// </summary>
        private readonly static string knownLoaderTypeName = nameof(RollbarConfigurationLoader);

        /// <summary>
        /// The configuration loader type
        /// </summary>
        private readonly static Type? configurationLoaderType = null;

        /// <summary>
        /// The loader
        /// </summary>
        private readonly IRollbarConfigurationLoader? _loader = null;


        /// <summary>
        /// Initializes static members of the <see cref="RollbarConfigurationLoader" /> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3963:\"static\" fields should be initialized inline", Justification = "Keeps code more maintainable.")]
        static RollbarConfigurationLoader()
        {
            string sdkAssembliesPath = RuntimeEnvironmentUtility.GetSdkRuntimeLocationPath();
            traceSource.TraceInformation($"Rollbar SDK runtime location: {sdkAssembliesPath}");
            Assembly? assembly = null;
            foreach(var name in RollbarConfigurationLoader.knownAssemblyNames)
            {
                string dllName = $"{name}.dll";
                string dllFullPath = Path.Combine(sdkAssembliesPath, dllName);
                if (!File.Exists(dllFullPath))
                {
                    continue;
                }

                traceSource.TraceInformation($"Located optional Rollbar SDK assembly: {dllFullPath}");
                assembly = ReflectionUtility.LoadSdkModuleAssembly(dllFullPath);
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
        /// Initializes a new instance of the <see cref="RollbarConfigurationLoader" /> class.
        /// </summary>
        public RollbarConfigurationLoader()
        {
            if (RollbarConfigurationLoader.configurationLoaderType != null)
            {
               var ctor = RollbarConfigurationLoader.configurationLoaderType.GetConstructor(new Type[0]);
               this._loader = (ctor?.Invoke(new object[0])) as IRollbarConfigurationLoader;
            }
        }

        /// <summary>
        /// Loads the provided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        public bool Load(RollbarInfrastructureConfig config)
        {
            return (this._loader != null && this._loader.Load(config));
        }

        /// <summary>
        /// Loads the provided configuration object based on found configuration store (if any).
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if configuration was found, <c>false</c> otherwise.</returns>
        public bool Load(RollbarTelemetryOptions config)
        {
            return (this._loader != null && this._loader.Load(config));
        }

        /// <summary>
        /// Loads the rollbar configuration.
        /// </summary>
        /// <returns>IRollbarConfig or null if no configuration store was found.</returns>
        public IRollbarInfrastructureConfig? LoadRollbarConfig()
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
        public IRollbarTelemetryOptions? LoadTelemetryConfig()
        {
            if (this._loader != null) 
                return this._loader.LoadTelemetryConfig();
            else
                return null;
        }
    }
}
