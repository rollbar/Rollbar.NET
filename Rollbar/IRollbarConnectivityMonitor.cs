namespace Rollbar
{
    /// <summary>
    /// Interface IRollbarConnectivityMonitor
    /// </summary>
    public interface IRollbarConnectivityMonitor
    {
        /// <summary>
        /// Gets a value indicating whether this instance is connectivity on.
        /// </summary>
        /// <value><c>true</c> if this instance is connectivity on; otherwise, <c>false</c>.</value>
        bool IsConnectivityOn
        {
            get;
        }
        /// <summary>
        /// Gets a value indicating whether this instance is disabled.
        /// </summary>
        /// <value><c>true</c> if this instance is disabled; otherwise, <c>false</c>.</value>
        bool IsDisabled
        {
            get;
        }
    }
}