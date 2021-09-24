namespace Rollbar
{
    using System;

    using Rollbar.Common;

    /// <summary>
    /// Interface IRollbarInfrastructureOptions
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    public interface IRollbarInfrastructureOptions
        : IReconfigurable<IRollbarInfrastructureOptions, IRollbarInfrastructureOptions>
    {
        /// <summary>
        /// Gets the maximum reports per minute.
        /// </summary>
        /// <value>The maximum reports per minute.</value>
        int? MaxReportsPerMinute
        {
            get;
        }

        /// <summary>
        /// Gets the reporting queue depth.
        /// </summary>
        /// <value>The reporting queue depth.</value>
        int ReportingQueueDepth
        {
            get;
        }


        /// <summary>
        /// Gets the payload post timeout.
        /// </summary>
        /// <value>The payload post timeout.</value>
        TimeSpan PayloadPostTimeout
        {
            get;
        }

        /// <summary>
        /// Gets or sets the maximum items.
        /// </summary>
        /// <value>The maximum items.</value>
        int MaxItems
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to capture uncaught exceptions.
        /// </summary>
        /// <value><c>true</c> if to capture uncaught exceptions; otherwise, <c>false</c>.</value>
        bool CaptureUncaughtExceptions
        {
            get; set;
        }
    }
}
