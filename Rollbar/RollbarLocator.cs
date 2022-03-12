namespace Rollbar
{
    using System;

    using System.Diagnostics;

    /// <summary>
    /// Singleton-like locator of the single shared instance of IRollbar component.
    /// </summary>
    public sealed class RollbarLocator
    {
        private static readonly TraceSource traceSource =
            new TraceSource(typeof(RollbarLocator).FullName ?? "RollbarLocator");

        #region singleton implementation

        private static readonly Lazy<IRollbar> lazySingleton =
            new Lazy<IRollbar>(() => RollbarFactory.CreateNew());

        /// <summary>
        /// Gets the singleton-like IRollbar instance.
        /// </summary>
        /// <value>The instance.</value>
        public static IRollbar RollbarInstance
        {
            get
            {
                return lazySingleton.Value;
            }
        }

        private RollbarLocator()
        {
            traceSource.TraceInformation($"Creating the {typeof(RollbarLocator).Name}...");
        }

        #endregion singleton implementation
    }
}
