namespace Rollbar.Common
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;

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
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            string sdkAssembliesPath = Path.GetDirectoryName(thisAssembly.Location);
            return sdkAssembliesPath;
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
            StringBuilder result = new StringBuilder(theType.Assembly.GetName().Version.ToString(3));
            //var infoVersion = theType.Assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            //if (infoVersion != null && infoVersion.InformationalVersion.StartsWith("LTS"))
            //{
            //    result.Append($" ({infoVersion.InformationalVersion.TrimEnd('-')})");
            //}
            return result.ToString();
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
