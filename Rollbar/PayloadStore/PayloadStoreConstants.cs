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
        public static readonly string DefaultRollbarStoreDbFileLocation = null;

        /// <summary>
        /// Initializes static members of the <see cref="PayloadStoreConstants"/> class.
        /// </summary>
        static PayloadStoreConstants()
        {
            if (!Environment.OSVersion.VersionString.Contains("Windows"))
            {
                DefaultRollbarStoreDbFileLocation =
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
        }
    }
}
