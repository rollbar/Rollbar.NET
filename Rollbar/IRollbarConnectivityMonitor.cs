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

        /// <summary>
        /// Disables this instance.
        /// </summary>
        /// <remarks>
        /// Any concrete Connectivity Monitor implementation may not be 100% accurate for all the possible 
        /// network environments. So, you may have to disable it in case it does not properly detect 
        /// specific network conditions. If disabled it will be assumed to always have its 
        /// IsConnectivityOn property returning true. 
        /// </remarks>
        void Disable();
    }
}