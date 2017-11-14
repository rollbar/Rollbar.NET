namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Defines an interface of a traceable object.
    /// </summary>
    public interface ITraceable
    {
        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>String rendering of this instance.</returns>
        string TraceAsString(string indent = "");
    }
}
