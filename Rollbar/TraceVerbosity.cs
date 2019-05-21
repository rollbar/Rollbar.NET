namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Enum TraceVerbosity
    /// </summary>
    public enum TraceVerbosity
    {
        /// <summary>
        /// The quiet
        /// Tracing is completely suspended.
        /// </summary>
        Quiet,

        /// <summary>
        /// The minimal
        /// Minimalistic tracing of most critical information.
        /// </summary>
        Minimal,
        
        /// <summary>
        /// The normal
        /// Tracing essential internal events of the SDK.
        /// </summary>
        Normal,

        /// <summary>
        /// The detailed
        /// Tracing essential internal events of the SDK with more details about the events.
        /// </summary>
        Detailed,
        
        /// <summary>
        /// The diagnostic
        /// Complete tracing all available information about all the internal events within the SDK.
        /// </summary>
        Diagnostic,
    }
}
