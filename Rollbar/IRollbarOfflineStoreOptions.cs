namespace Rollbar
{

    using Rollbar.Common;


#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
#pragma warning disable CS1658 // Warning is overriding an error
    /// <summary>
    /// Interface IRollbarOfflineStoreOptions
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{Rollbar.IRollbarOfflineStoreOptions, Rollbar.IRollbarOfflineStoreOptions}" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IReconfigurable{Rollbar.IRollbarOfflineStoreOptions, Rollbar.IRollbarOfflineStoreOptions}" />
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
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
        string LocalPayloadStoreFileName
        {
            get;
        }

        /// <summary>
        /// Gets the local payload store location path.
        /// </summary>
        /// <value>The local payload store location path.</value>
        string LocalPayloadStoreLocationPath
        {
            get;
        }

    }
}
