namespace Rollbar
{
    using System;

    using Rollbar.Common;

    /// <summary>
    /// Class RollbarInfrastructureOptions.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// Implements the <see cref="Rollbar.IRollbarInfrastructureOptions" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// <seealso cref="Rollbar.IRollbarInfrastructureOptions" />
    public class RollbarInfrastructureOptions
        : ReconfigurableBase<RollbarInfrastructureOptions, IRollbarInfrastructureOptions>
        , IRollbarInfrastructureOptions
    {
        /// <summary>
        /// The default maximum reports per minute
        /// </summary>
        private readonly static int? defaultMaxReportsPerMinute = null;
        /// <summary>
        /// The default payload post timeout
        /// </summary>
        private readonly static TimeSpan defaultPayloadPostTimeout = TimeSpan.FromSeconds(30);
        /// <summary>
        /// The default reporting queue depth
        /// </summary>
        private const int defaultReportingQueueDepth = 20;
        /// <summary>
        /// The default maximum items
        /// </summary>
        private const int defaultMaxItems = 10;

        /// <summary>
        /// The default value to capture uncaught exceptions
        /// </summary>
        private const bool defaultCaptureUncaughtExceptions = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarInfrastructureOptions"/> class.
        /// </summary>
        public RollbarInfrastructureOptions()
            : this(RollbarInfrastructureOptions.defaultMaxReportsPerMinute, RollbarInfrastructureOptions.defaultPayloadPostTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarInfrastructureOptions"/> class.
        /// </summary>
        /// <param name="maxReportsPerMinute">The maximum reports per minute.</param>
        public RollbarInfrastructureOptions(int? maxReportsPerMinute)
            : this(maxReportsPerMinute, RollbarInfrastructureOptions.defaultPayloadPostTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarInfrastructureOptions"/> class.
        /// </summary>
        /// <param name="payloadPostTimeout">The payload post timeout.</param>
        public RollbarInfrastructureOptions(TimeSpan payloadPostTimeout)
            : this(RollbarInfrastructureOptions.defaultMaxReportsPerMinute, payloadPostTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarInfrastructureOptions"/> class.
        /// </summary>
        /// <param name="maxReportsPerMinute">The maximum reports per minute.</param>
        /// <param name="payloadPostTimeout">The payload post timeout.</param>
        /// <param name="reportingQueueDepth">The reporting queue depth.</param>
        /// <param name="maxItems">The maximum items.</param>
        /// <param name="captureUncaughtExceptions">if set to <c>true</c> to capture uncaught exceptions.</param>
        public RollbarInfrastructureOptions(
            int? maxReportsPerMinute,
            TimeSpan payloadPostTimeout,
            int reportingQueueDepth = RollbarInfrastructureOptions.defaultReportingQueueDepth,
            int maxItems = RollbarInfrastructureOptions.defaultMaxItems,
            bool captureUncaughtExceptions = RollbarInfrastructureOptions.defaultCaptureUncaughtExceptions
            )
        {
            this.MaxReportsPerMinute = maxReportsPerMinute;
            this.ReportingQueueDepth = reportingQueueDepth;
            this.PayloadPostTimeout = payloadPostTimeout;
            this.MaxItems = maxItems;
            this.CaptureUncaughtExceptions = captureUncaughtExceptions;
        }

        /// <summary>
        /// Gets the maximum reports per minute.
        /// </summary>
        /// <value>The maximum reports per minute.</value>
        public int? MaxReportsPerMinute
        {
            get;
            set;
        }
        /// <summary>
        /// Gets the reporting queue depth.
        /// </summary>
        /// <value>The reporting queue depth.</value>
        public int ReportingQueueDepth
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the payload post timeout.
        /// </summary>
        /// <value>The payload post timeout.</value>
        public TimeSpan PayloadPostTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum items.
        /// </summary>
        /// <value>The maximum items.</value>
        public int MaxItems
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to capture uncaught exceptions.
        /// </summary>
        /// <value><c>true</c> if to capture uncaught exceptions; otherwise, <c>false</c>.</value>
        public bool CaptureUncaughtExceptions
        {
            get; set;
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator? GetValidator()
        {
            return null;
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        public override RollbarInfrastructureOptions Reconfigure(IRollbarInfrastructureOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        IRollbarInfrastructureOptions IReconfigurable<IRollbarInfrastructureOptions, IRollbarInfrastructureOptions>.Reconfigure(IRollbarInfrastructureOptions likeMe)
        {
            return this.Reconfigure(likeMe);
        }
    }
}
