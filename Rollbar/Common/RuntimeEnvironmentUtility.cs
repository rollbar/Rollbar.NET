namespace Rollbar.Common
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;
    using System.IO;

    /// <summary>
    /// A utility class aiding discovery of the current runtime environment.
    /// </summary>
    public static class RuntimeEnvironmentUtility
    {
        /// <summary>
        /// Gets the SDK runtime location path.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string GetSdkRuntimeLocationPath()
        {

#if (!NETFX || NETFX_461nNewer)
            string? path = AppContext.BaseDirectory;

            if (string.IsNullOrWhiteSpace(path))
            {
                Assembly thisAssembly = Assembly.GetExecutingAssembly();
                string? sdkAssembliesPath = Path.GetDirectoryName(thisAssembly.Location);
                path = sdkAssembliesPath;
            }

            if (path != null)
            {
                return path;
            }
#endif

            return string.Empty;
        }

        /// <summary>
        /// Gets the type assembly product.
        /// </summary>
        /// <param name="theType">The type.</param>
        /// <returns>System.String.</returns>
        public static string GetTypeAssemblyProduct(Type theType)
        {
            if (theType.Assembly.GetCustomAttribute(typeof(AssemblyProductAttribute)) is AssemblyProductAttribute product)
            {
                return product.Product;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the type assembly version.
        /// </summary>
        /// <param name="theType">The type.</param>
        /// <returns></returns>
        public static string GetTypeAssemblyVersion(Type theType)
        {
            if (theType.Assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute infoVersion)
            {
                return infoVersion.InformationalVersion;
            }
                
            return string.Empty;
        }

        /// <summary>
        /// Gets the assembly target frameworks.
        /// </summary>
        /// <param name="typeFromAssembly">The type from assembly.</param>
        /// <returns></returns>
        public static string[] GetAssemblyTargetFrameworks(Type typeFromAssembly)
        {
            return RuntimeEnvironmentUtility.GetAssemblyTargetFrameworks(typeFromAssembly.Assembly);
        }

        /// <summary>
        /// Gets the assembly target frameworks.
        /// </summary>
        /// <param name="theAssembly">The assembly.</param>
        /// <returns></returns>
        public static string[] GetAssemblyTargetFrameworks(Assembly theAssembly)
        {
            var attributes = theAssembly
                .GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
                .Cast<TargetFrameworkAttribute>()
                .ToArray();

            var targetFrameworks = attributes
                .Select(a=>a.FrameworkName)
                .ToArray();

            return targetFrameworks;
        }

        /// <summary>
        /// Gets the hosting CLR version.
        /// </summary>
        /// <returns>Version.</returns>
        public static Version GetHostingClrVersion()
        {
            var ver = Environment.Version;
            return ver;
        }
        /// <summary>
        /// Gets the dot net runtime description.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string GetDotNetRuntimeDescription()
        {
#if NETFX_47nOlder
            string dotNetRuntime = $".NET {Environment.Version}";
#else
            string dotNetRuntime = RuntimeInformation.FrameworkDescription;
#endif
            return dotNetRuntime;
        }

        /// <summary>
        /// Gets the OS description.
        /// </summary>
        /// <returns></returns>
        public static string GetOSDescription()
        {
#if NETFX_47nOlder
            return Environment.OSVersion.VersionString;
#else
            return RuntimeInformation.OSDescription;
#endif
        }

        /// <summary>
        /// Gets the CPU architecture.
        /// </summary>
        /// <returns></returns>
        public static string? GetCpuArchitecture()
        {
#if NETFX_47nOlder
            return null;
#else
            return RuntimeInformation.OSArchitecture.ToString();
#endif
        }
    }
}
