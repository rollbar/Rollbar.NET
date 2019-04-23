namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface IErrorCollector
    /// </summary>
    public interface IErrorCollector
    {
        /// <summary>
        /// Gets the collected exceptions.
        /// </summary>
        /// <value>The exceptions.</value>
        IReadOnlyCollection<Exception> Exceptions { get; }

        /// <summary>
        /// Registers the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void Register(Exception exception);
    }
}
