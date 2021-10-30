namespace Rollbar.PayloadStore
{
    using System;

    /// <summary>
    /// Class PayloadStoreConstants.
    /// </summary>
    public static class PayloadStoreConstants
    {
        /// <summary>
        /// The default rollbar store database file
        /// </summary>
        /// <value>The default rollbar store database file.</value>
        public static string DefaultRollbarStoreDbFile { get; set; } = "RollbarPayloadsStore.db";


        /// <summary>
        /// The default rollbar store database file location
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Not applicable to all targeted .NET versions.")]
        public static readonly string? DefaultRollbarStoreDbFileLocation = 
            !Environment.OSVersion.VersionString.Contains("Windows") 
            ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) 
            : null;
    }
}
