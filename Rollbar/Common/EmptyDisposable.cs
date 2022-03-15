namespace Rollbar.Common
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Implements empty/no-op disposable singleton.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class EmptyDisposable
        : IDisposable
    {
        private static readonly TraceSource traceSource =
            new TraceSource(typeof(EmptyDisposable).FullName ?? "EmptyDisposable");

        #region singleton implementation

        private static readonly Lazy<EmptyDisposable> lazySingleton =
            new Lazy<EmptyDisposable>(() => new EmptyDisposable());

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static EmptyDisposable Instance
        {
            get
            {
                return lazySingleton.Value;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="EmptyDisposable" /> class from being created.
        /// </summary>
        private EmptyDisposable()
        {
            traceSource.TraceInformation($"Creating the {typeof(EmptyDisposable).Name}...");
        }

        #endregion singleton implementation


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // no-op...
        }
    }
}
