namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    public interface IRollbarInfrastructureOptions
        : IReconfigurable<IRollbarInfrastructureOptions, IRollbarInfrastructureOptions>
    {
        /// <summary>
        /// Gets the maximum reports per minute.
        /// </summary>
        /// <value>
        /// The maximum reports per minute.
        /// </value>
        int? MaxReportsPerMinute
        {
            get;
        }

        /// <summary>
        /// Gets the reporting queue depth.
        /// </summary>
        /// <value>
        /// The reporting queue depth.
        /// </value>
        int ReportingQueueDepth
        {
            get;
        }

    }
}
