namespace Rollbar.PlugIns.Serilog
{
    using System;
    using global::Serilog.Core;
    using global::Serilog.Events;

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
    /// <summary>
    /// Class RollbarSink for Serilog.
    /// Implements the <see cref="Serilog.Core.ILogEventSink" />
    /// </summary>
    /// <seealso cref="Serilog.Core.ILogEventSink" />
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
    public class RollbarSink
        : ILogEventSink
    {
        private readonly RollbarPlugInCore _rollbarPlugIn;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarSink"/> class.
        /// </summary>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <param name="formatProvider">The format provider.</param>
        public RollbarSink(
            string rollbarAccessToken, 
            string rollbarEnvironment,
            TimeSpan? rollbarBlockingLoggingTimeout,
            IFormatProvider? formatProvider
            )
            : this(
                  RollbarPlugInCore.CreateConfig(rollbarAccessToken: rollbarAccessToken, rollbarEnvironment: rollbarEnvironment),
                  rollbarBlockingLoggingTimeout,
                  formatProvider
                  )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarSink"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <param name="formatProvider">The format provider.</param>
        public RollbarSink(
            IRollbarInfrastructureConfig rollbarConfig,
            TimeSpan? rollbarBlockingLoggingTimeout,
            IFormatProvider? formatProvider
            )
        {
            this._rollbarPlugIn = 
                new RollbarPlugInCore(rollbarConfig, rollbarBlockingLoggingTimeout, formatProvider);
        }

        /// <summary>
        /// Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write.</param>
        public void Emit(LogEvent logEvent)
        {
            if (logEvent != null)
            {
                this._rollbarPlugIn.ReportToRollbar(logEvent, logEvent.Level);
            }
        }
    }
}
