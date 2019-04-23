namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class ErrorCollector.
    /// Implements the <see cref="Rollbar.Common.IErrorCollector" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IErrorCollector" />
    public class ErrorCollector
        : IErrorCollector
    {
        /// <summary>
        /// The exceptions
        /// </summary>
        private readonly List<Exception> _exceptions = new List<Exception>();

        /// <summary>
        /// Gets the collected exceptions.
        /// </summary>
        /// <value>The exceptions.</value>
        public IReadOnlyCollection<Exception> Exceptions
        {
            get { return this._exceptions; }
        }

        /// <summary>
        /// Registers the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Register(Exception exception)
        {
            this._exceptions.Add(exception);
        }
    }
}
