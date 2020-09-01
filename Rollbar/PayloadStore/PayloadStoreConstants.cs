using System;
using System.Collections.Generic;
using System.Text;

namespace Rollbar.PayloadStore
{
    public static class PayloadStoreConstants
    {
        /// <summary>
        /// The default rollbar store database file
        /// </summary>
        public static string DefaultRollbarStoreDbFile { get; set; } = "RollbarPayloadsStore.db";

        /// <summary>
        /// The default rollbar store database file location
        /// </summary>
        public static readonly string DefaultRollbarStoreDbFileLocation = null;

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
