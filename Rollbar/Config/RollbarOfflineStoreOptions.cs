namespace Rollbar
{
    using System.IO;

    using Rollbar.Common;
    using Rollbar.PayloadStore;

    /// <summary>
    /// Class RollbarOfflineStoreOptions.
    /// Implements the <see cref="ReconfigurableBase{T, TBase}" />
    /// Implements the <see cref="IRollbarOfflineStoreOptions" />
    /// </summary>
    /// <seealso cref="ReconfigurableBase{T, TBase}" />
    /// <seealso cref="IRollbarOfflineStoreOptions" />
    public class RollbarOfflineStoreOptions
        : ReconfigurableBase<RollbarOfflineStoreOptions, IRollbarOfflineStoreOptions>
        , IRollbarOfflineStoreOptions
    {

        private const bool defaultEnableLocalPayloadStore = false;
        private static readonly string defaultLocalPayloadStoreFileName = PayloadStoreConstants.DefaultRollbarStoreDbFile;
        private static readonly string? defaultLocalPayloadStoreLocationPath = PayloadStoreConstants.DefaultRollbarStoreDbFileLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarOfflineStoreOptions"/> class.
        /// </summary>
        public RollbarOfflineStoreOptions()
            : this(defaultEnableLocalPayloadStore, defaultLocalPayloadStoreFileName, defaultLocalPayloadStoreLocationPath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarOfflineStoreOptions"/> class.
        /// </summary>
        /// <param name="enablePayloadStore">if set to <c>true</c> [enable payload store].</param>
        /// <param name="localPayloadStoreFileName">Name of the local payload store file.</param>
        /// <param name="LocalPayloadStoreLocationPath">The local payload store location path.</param>
        public RollbarOfflineStoreOptions(bool enablePayloadStore, string localPayloadStoreFileName, string? LocalPayloadStoreLocationPath)
        {
            this.EnableLocalPayloadStore = enablePayloadStore;
            this.LocalPayloadStoreFileName = localPayloadStoreFileName;
            this.LocalPayloadStoreLocationPath = LocalPayloadStoreLocationPath;
        }
        /// <summary>
        /// Gets a value indicating whether to enable local payload store.
        /// </summary>
        /// <value><c>true</c> if to enable local payload store; otherwise, <c>false</c>.</value>
        public bool EnableLocalPayloadStore
        {
            get;
            set;
        }
        /// <summary>
        /// Gets the name of the local payload store file.
        /// </summary>
        /// <value>The name of the local payload store file.</value>
        public string? LocalPayloadStoreFileName
        {
            get;
            set;
        }
        /// <summary>
        /// Gets the local payload store location path.
        /// </summary>
        /// <value>The local payload store location path.</value>
        public string? LocalPayloadStoreLocationPath
        {
            get;
            set;
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        public override RollbarOfflineStoreOptions Reconfigure(IRollbarOfflineStoreOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator? GetValidator()
        {
            return null;
        }

        /// <summary>
        /// Gets the name of the local payload store full path.
        /// </summary>
        /// <returns>System.String.</returns>
        internal string? GetLocalPayloadStoreFullPathName()
        {
            string? dbLocation = string.IsNullOrWhiteSpace(this.LocalPayloadStoreLocationPath)
                ? PayloadStoreConstants.DefaultRollbarStoreDbFileLocation
                : this.LocalPayloadStoreLocationPath;

            string? dbFile = string.IsNullOrWhiteSpace(this.LocalPayloadStoreFileName)
                ? PayloadStoreConstants.DefaultRollbarStoreDbFile
                : this.LocalPayloadStoreFileName;

            string? result = string.IsNullOrWhiteSpace(dbLocation)
                ? dbFile
                : Path.Combine(dbLocation, dbFile);

            return result;
        }

        IRollbarOfflineStoreOptions IReconfigurable<IRollbarOfflineStoreOptions, IRollbarOfflineStoreOptions>.Reconfigure(IRollbarOfflineStoreOptions likeMe)
        {
            return this.Reconfigure(likeMe);
        }
    }
}
