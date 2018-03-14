#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Models Rollbar middleware usage options.
    /// </summary>
    public class RollbarOptions
    {
        /// <summary>
        /// Logging filter function based on the logger name and the log level.
        /// </summary>
        public Func<string, LogLevel, bool> Filter { get; set; } 
            = (loggerName, logLevel) => true
            ;
    }
}

#endif