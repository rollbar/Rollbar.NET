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
            string path = null;

#if (!NETFX || NETFX_461nNewer)
            path = AppContext.BaseDirectory;
#endif

            if (string.IsNullOrWhiteSpace(path))
            {
                Assembly thisAssembly = Assembly.GetExecutingAssembly();
                string sdkAssembliesPath = Path.GetDirectoryName(thisAssembly.Location);
                path = sdkAssembliesPath;
            }

            if (path != null)
            {
                return path;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the type assembly product.
        /// </summary>
        /// <param name="theType">The type.</param>
        /// <returns>System.String.</returns>
        public static string GetTypeAssemblyProduct(Type theType)
        {
            var product = theType.Assembly.GetCustomAttribute(typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
            if (product != null)
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
            //StringBuilder result = new StringBuilder(theType.Assembly.GetName().Version.ToString(3));
            //return result.ToString();

            var infoVersion = theType.Assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            return infoVersion.InformationalVersion;
            //if (infoVersion != null && infoVersion.InformationalVersion.StartsWith("LTS"))
            //{
            //    result.Append($" ({infoVersion.InformationalVersion.TrimEnd('-')})");
            //}
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
        public static string GetCpuArchitecture()
        {
#if NETFX_47nOlder
            return null;
#else
            return RuntimeInformation.OSArchitecture.ToString();
#endif
        }
    }
}
