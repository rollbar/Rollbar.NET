namespace Rollbar
{

    using Rollbar.Common;


    /// <summary>
    /// Interface IRollbarOfflineStoreOptions
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    public interface IRollbarOfflineStoreOptions
        : IReconfigurable<IRollbarOfflineStoreOptions, IRollbarOfflineStoreOptions>
    {
        /// <summary>
        /// Gets a value indicating whether to enable local payload store.
        /// </summary>
        /// <value><c>true</c> if to enable local payload store; otherwise, <c>false</c>.</value>
        bool EnableLocalPayloadStore
        {
            get;
        }

        /// <summary>
        /// Gets the name of the local payload store file.
        /// </summary>
        /// <value>The name of the local payload store file.</value>
        string? LocalPayloadStoreFileName
        {
            get;
        }

        /// <summary>
        /// Gets the local payload store location path.
        /// </summary>
        /// <value>The local payload store location path.</value>
        string? LocalPayloadStoreLocationPath
        {
            get;
        }

    }
}
