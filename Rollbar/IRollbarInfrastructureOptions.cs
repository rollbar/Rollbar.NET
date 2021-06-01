namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IRollbarInfrastructureOptions
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
