namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Implements empty/no-op disposable singleton.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class EmptyDisposable
        : IDisposable
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="EmptyDisposable"/> class from being created.
        /// </summary>
        private EmptyDisposable()
        {
        }

        /// <summary>
        /// Gets the singleton-like IRollbar instance.
        /// </summary>
        /// <value>
        /// The single shared IRollbar instance.
        /// </value>
        public static EmptyDisposable Instance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // no-op...
        }

        private sealed class NestedSingleInstance
        {
            static NestedSingleInstance()
            {
            }

            internal static readonly EmptyDisposable Instance = new EmptyDisposable();
        }
    }
}
