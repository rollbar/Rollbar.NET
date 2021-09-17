namespace Rollbar
{
    using System;

    using Rollbar.Common;

    /// <summary>
    /// Class RollbarDeveloperOptions.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// Implements the <see cref="Rollbar.IRollbarDeveloperOptions" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// <seealso cref="Rollbar.IRollbarDeveloperOptions" />
    public class RollbarDeveloperOptions
        : ReconfigurableBase<RollbarDeveloperOptions, IRollbarDeveloperOptions>
        , IRollbarDeveloperOptions
    {
        /// <summary>
        /// The default payload post timeout
        /// </summary>
        private readonly static TimeSpan defaultPayloadPostTimeout = TimeSpan.FromSeconds(30);
        /// <summary>
        /// The default value for log level
        /// </summary>
        private const ErrorLevel defaultLogLevel = ErrorLevel.Debug;
        /// <summary>
        /// The default value for enabled
        /// </summary>
        private const bool defaultEnabled = true;
        /// <summary>
        /// The default value for transmit
        /// </summary>
        private const bool defaultTransmit = true;
        /// <summary>
        /// The default value for rethrow exceptions after reporting
        /// </summary>
        private const bool defaultRethrowExceptionsAfterReporting = false;
        /// <summary>
        /// The default value for  wrap reported exception with rollbar exception
        /// </summary>
        private const bool defaultWrapReportedExceptionWithRollbarException = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarDeveloperOptions"/> class.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="transmit">if set to <c>true</c> [transmit].</param>
        /// <param name="rethrowExceptionsAfterReporting">if set to <c>true</c> [rethrow exceptions after reporting].</param>
        /// <param name="wrapReportedExceptionWithRollbarException">if set to <c>true</c> [wrap reported exception with rollbar exception].</param>
        public RollbarDeveloperOptions(
            ErrorLevel logLevel = RollbarDeveloperOptions.defaultLogLevel,
            bool enabled = RollbarDeveloperOptions.defaultEnabled,
            bool transmit = RollbarDeveloperOptions.defaultTransmit,
            bool rethrowExceptionsAfterReporting = RollbarDeveloperOptions.defaultRethrowExceptionsAfterReporting,
            bool wrapReportedExceptionWithRollbarException = RollbarDeveloperOptions.defaultWrapReportedExceptionWithRollbarException
            )
            : this(
                  logLevel, 
                  enabled, 
                  transmit, 
                  rethrowExceptionsAfterReporting, 
                  wrapReportedExceptionWithRollbarException, 
                  RollbarDeveloperOptions.defaultPayloadPostTimeout
                  )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarDeveloperOptions"/> class.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="transmit">if set to <c>true</c> [transmit].</param>
        /// <param name="rethrowExceptionsAfterReporting">if set to <c>true</c> [rethrow exceptions after reporting].</param>
        /// <param name="wrapReportedExceptionWithRollbarException">if set to <c>true</c> wrap reported exception with a Rollbar exception.</param>
        /// <param name="payloadPostTimeout">The payload post timeout.</param>
        public RollbarDeveloperOptions(
            ErrorLevel logLevel, 
            bool enabled, 
            bool transmit, 
            bool rethrowExceptionsAfterReporting,
            bool wrapReportedExceptionWithRollbarException,
            TimeSpan payloadPostTimeout
            )
        {
            this.LogLevel = logLevel;
            this.Enabled = enabled;
            this.Transmit = transmit;
            this.RethrowExceptionsAfterReporting = rethrowExceptionsAfterReporting;
            this.WrapReportedExceptionWithRollbarException = wrapReportedExceptionWithRollbarException;
            this.PayloadPostTimeout = payloadPostTimeout;
        }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>The log level.</value>
        public ErrorLevel LogLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Rollbar logger, configured with this <see cref="IRollbarDeveloperOptions" />, is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        /// <remarks>Default: true</remarks>
        public bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Rollbar logger will actually transmit the payloads to the Rollbar API server.
        /// </summary>
        /// <value><c>true</c> if transmit; otherwise, <c>false</c>.</value>
        /// <remarks>Should the SDK actually perform HTTP requests to Rollbar API. This is useful if you are trying to run Rollbar in dry run mode for development or tests.
        /// If this is false then we do all of the report processing except making the post request at the end of the pipeline.
        /// Default: true</remarks>
        public bool Transmit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to rethrow exceptions after reporting them to Rollbar API.
        /// </summary>
        /// <value><c>true</c> if to rethrow exceptions after reporting them to Rollbar API; otherwise, <c>false</c>.</value>
        public bool RethrowExceptionsAfterReporting
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to wrap reported exception with a Rollbar exception.
        /// </summary>
        /// <value><c>true</c> if to wrap reported exception with a Rollbar exception; otherwise, <c>false</c>.</value>
        public bool WrapReportedExceptionWithRollbarException
        {
            get; 
            set;
        }

        /// <summary>
        /// Gets the payload POST timeout.
        /// </summary>
        /// <value>The payload POST timeout.</value>
        public TimeSpan PayloadPostTimeout
        {
            get;
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        public override RollbarDeveloperOptions Reconfigure(IRollbarDeveloperOptions likeMe)
        {
            return base.Reconfigure(likeMe);
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
        IRollbarDeveloperOptions IReconfigurable<IRollbarDeveloperOptions, IRollbarDeveloperOptions>.Reconfigure(IRollbarDeveloperOptions likeMe)
        {
            return this.Reconfigure(likeMe);
        }
    }
}
