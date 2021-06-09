namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    public class RollbarInfrastructureOptions
        : ReconfigurableBase<RollbarInfrastructureOptions, IRollbarInfrastructureOptions>
        , IRollbarInfrastructureOptions
    {
        private readonly static int? defaultMaxReportsPerMinute = null;
        private const int defaultReportingQueueDepth = 20;

        public RollbarInfrastructureOptions()
            : this(RollbarInfrastructureOptions.defaultMaxReportsPerMinute)
        {
        }

        public RollbarInfrastructureOptions(
            int? maxReportsPerMinute, 
            int reportingQueueDepth = RollbarInfrastructureOptions.defaultReportingQueueDepth
            )
        {
            MaxReportsPerMinute = maxReportsPerMinute;
            ReportingQueueDepth = reportingQueueDepth;
        }

        public int? MaxReportsPerMinute
        {
            get;
            set;
        }
        public int ReportingQueueDepth
        {
            get;
            set;
        }

        public IRollbarInfrastructureOptions Reconfigure(IRollbarInfrastructureOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

        public override Validator GetValidator()
        {
            return null;
        }
    }
}
