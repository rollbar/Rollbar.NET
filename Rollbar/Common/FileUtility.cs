namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Class FileUtility.
    /// </summary>
    public static class FileUtility
    {
        /// <summary>
        /// Verifies if the specified application file (a file that is part of an application) exists.
        /// </summary>
        /// <param name="applicationRootRelativeFilePath">
        /// The application root relative file path.
        /// The relative path starting from the application root directory.
        /// </param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise.</returns>
        public static bool ApplicationFileExists(string applicationRootRelativeFilePath)
        {
            string absoluteFilePath = null;

            string currentDir = Environment.CurrentDirectory;
            absoluteFilePath = Path.Combine(currentDir, applicationRootRelativeFilePath);
            if (File.Exists(absoluteFilePath))
            {
                return true;
            }

            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDir = Path.GetDirectoryName(exePath);
            absoluteFilePath = Path.Combine(exeDir, applicationRootRelativeFilePath);
            if (File.Exists(absoluteFilePath))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets known application configuration file alternatives.
        /// </summary>
        /// <returns>IReadOnlyCollection&lt;System.String&gt;.</returns>
        public static IReadOnlyCollection<string> GetAppConfigFileAlternatives()
        {
            List<string> fileNames = new List<string>();
            fileNames.Add("web.config");
            Assembly exeAssembly = Assembly.GetEntryAssembly();
            if (exeAssembly != null)
            {
                fileNames.Add(Path.GetFileName(Assembly.GetEntryAssembly().Location) + ".config");
            }
            return fileNames;
        }

        /// <summary>
        /// Applications the configuration file present.
        /// </summary>
        /// <returns><c>true</c> if present, <c>false</c> otherwise.</returns>
        public static bool AppConfigFilePresent()
        {
            bool isAppConfigFilePresent = false;

            foreach (var fileName in FileUtility.GetAppConfigFileAlternatives())
            {
                isAppConfigFilePresent = FileUtility.ApplicationFileExists(fileName);
                if (isAppConfigFilePresent)
                {
                    break;
                }
            }

            return isAppConfigFilePresent;
        }

    }
}
