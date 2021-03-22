namespace Rollbar
{
    using System;

    /// <summary>
    /// Defines an interface of a traceable object.
    /// </summary>
    public interface ITraceable
    {
        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <returns>System.String.</returns>
        string TraceAsString();

        /// <summary>
        /// Traces as a string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>String rendering of this instance.</returns>
        string TraceAsString(string indent);
    }
}
