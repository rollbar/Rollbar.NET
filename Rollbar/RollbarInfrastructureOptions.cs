namespace Rollbar
{
    using System;

    using Rollbar.Common;

    public class RollbarInfrastructureOptions
        : ReconfigurableBase<RollbarInfrastructureOptions, IRollbarInfrastructureOptions>
        , IRollbarInfrastructureOptions
    {
        private readonly static int? defaultMaxReportsPerMinute = null;
        private readonly static TimeSpan defaultPayloadPostTimeout = TimeSpan.FromSeconds(30);
        private const int defaultReportingQueueDepth = 20;
        private const int defaultMaxItems = 10;

        public RollbarInfrastructureOptions()
            : this(RollbarInfrastructureOptions.defaultMaxReportsPerMinute, RollbarInfrastructureOptions.defaultPayloadPostTimeout)
        {
        }

        public RollbarInfrastructureOptions(int? maxReportsPerMinute)
            : this(maxReportsPerMinute, RollbarInfrastructureOptions.defaultPayloadPostTimeout)
        {
        }

        public RollbarInfrastructureOptions(TimeSpan payloadPostTimeout)
            : this(RollbarInfrastructureOptions.defaultMaxReportsPerMinute, payloadPostTimeout)
        {
        }

        public RollbarInfrastructureOptions(
            int? maxReportsPerMinute,
            TimeSpan payloadPostTimeout,
            int reportingQueueDepth = RollbarInfrastructureOptions.defaultReportingQueueDepth,
            int maxItems = RollbarInfrastructureOptions.defaultMaxItems
            )
        {
            MaxReportsPerMinute = maxReportsPerMinute;
            ReportingQueueDepth = reportingQueueDepth;
            PayloadPostTimeout = payloadPostTimeout;
            MaxItems = maxItems;
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

        public TimeSpan PayloadPostTimeout
        {
            get;
            set;
        }

        public int MaxItems
        {
            get; set;
        }

        public bool CaptureUncaughtExceptions
        {
            get; set;
        }

        public override Validator GetValidator()
        {
            return null;
        }

        public IRollbarInfrastructureOptions Reconfigure(IRollbarInfrastructureOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

    }
}
