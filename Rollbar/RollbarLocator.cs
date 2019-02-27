namespace Rollbar
{

    /// <summary>
    /// Singleton-like locator of the single shared instance of IRollbar component.
    /// </summary>
    public class RollbarLocator
    {
        /// <summary>
        /// Gets the singleton-like IRollbar instance.
        /// </summary>
        /// <value>
        /// The single shared IRollbar instance.
        /// </value>
        public static IRollbar RollbarInstance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        private sealed class NestedSingleInstance
        {
            private NestedSingleInstance()
            {
            }

            internal static readonly IRollbar Instance = RollbarFactory.CreateNew(true);
        }
    }
}
