namespace Rollbar.PlugIns.Log4net
{
    using System;
    using log4net.Appender;
    using log4net.Core;

    /// <summary>
    /// Class RollbarAppender.
    /// Implements the <see cref="log4net.Appender.IAppender" />
    /// Implements the <see cref="log4net.Appender.AppenderSkeleton" />
    /// </summary>
    /// <seealso cref="log4net.Appender.AppenderSkeleton" />
    /// <seealso cref="log4net.Appender.IAppender" />
    public class RollbarAppender
        : AppenderSkeleton
        , IAppender
    {
        private readonly RollbarPlugInCore _rollbarPlugIn;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarAppender"/> class.
        /// </summary>
        public RollbarAppender()
            : this(null, RollbarPlugInCore.DefaultRollbarBlockingTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarAppender"/> class.
        /// </summary>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        public RollbarAppender(
            string rollbarAccessToken,
            string rollbarEnvironment,
            TimeSpan? rollbarBlockingLoggingTimeout
            )
            : this(
                  RollbarPlugInCore.CreateConfig(rollbarAccessToken: rollbarAccessToken, rollbarEnvironment: rollbarEnvironment),
                  rollbarBlockingLoggingTimeout
                  )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarAppender"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        public RollbarAppender(
            IRollbarInfrastructureConfig? rollbarConfig,
            TimeSpan? rollbarBlockingLoggingTimeout
            )
        {
            this._rollbarPlugIn = new RollbarPlugInCore(rollbarConfig, rollbarBlockingLoggingTimeout);
        }

        /// <summary>
        /// Gets the rollbar configuration.
        /// </summary>
        /// <value>The rollbar configuration.</value>
        public IRollbarLoggerConfig? RollbarConfig
        {
            get { return this._rollbarPlugIn.RollbarConfig; }
        }

        /// <summary>
        /// Raises the Close event.
        /// </summary>
        protected override void OnClose()
        {
            this._rollbarPlugIn.Dispose();

            base.OnClose();
        }

        /// <summary>
        /// Appends the specified logging event.
        /// </summary>
        /// <param name="loggingEvent">The logging event.</param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (loggingEvent != null)
            {
                this._rollbarPlugIn.ReportToRollbar(loggingEvent, loggingEvent.Level.Value);
            }
        }
    }
}
