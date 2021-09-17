namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    /// <summary>
    /// Interface IRollbarDeveloperOptions
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    public interface IRollbarDeveloperOptions
        : IReconfigurable<IRollbarDeveloperOptions, IRollbarDeveloperOptions>
    {
        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>The log level.</value>
        ErrorLevel LogLevel
        {
            get; set;
        }


        /// <summary>
        /// Gets or sets a value indicating whether the Rollbar logger, configured with this <see cref="IRollbarDeveloperOptions" />, is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        /// <remarks>Default: true</remarks>
        bool Enabled
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Rollbar logger will actually transmit the payloads to the Rollbar API server.
        /// </summary>
        /// <value><c>true</c> if transmit; otherwise, <c>false</c>.</value>
        /// <remarks>Should the SDK actually perform HTTP requests to Rollbar API. This is useful if you are trying to run Rollbar in dry run mode for development or tests.
        /// If this is false then we do all of the report processing except making the post request at the end of the pipeline.
        /// Default: true</remarks>
        bool Transmit
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to rethrow exceptions after reporting them to Rollbar API.
        /// </summary>
        /// <value><c>true</c> if to rethrow exceptions after reporting them to Rollbar API; otherwise, <c>false</c>.</value>
        bool RethrowExceptionsAfterReporting
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to wrap reported exception with a Rollbar exception.
        /// </summary>
        /// <value><c>true</c> if to wrap reported exception with a Rollbar exception; otherwise, <c>false</c>.</value>
        bool WrapReportedExceptionWithRollbarException
        {
            get; set;
        }

        /// <summary>
        /// Gets the payload POST timeout.
        /// </summary>
        /// <value>The payload POST timeout.</value>
        TimeSpan PayloadPostTimeout
        {
            get;
        }

    }
}
