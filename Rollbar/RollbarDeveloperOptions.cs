namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    public class RollbarDeveloperOptions
        : ReconfigurableBase<RollbarDeveloperOptions, IRollbarDeveloperOptions>
        , IRollbarDeveloperOptions
    {
        private readonly static TimeSpan defaultPayloadPostTimeout = TimeSpan.FromSeconds(30);
        private const ErrorLevel defaultLogLevel = ErrorLevel.Debug;
        private const bool defaultEnabled = true;
        private const bool defaultTransmit = true;
        private const bool defaultRethrowExceptionsAfterReporting = false;

        public RollbarDeveloperOptions(
            ErrorLevel logLevel = RollbarDeveloperOptions.defaultLogLevel,
            bool enabled = RollbarDeveloperOptions.defaultEnabled,
            bool transmit = RollbarDeveloperOptions.defaultTransmit,
            bool rethrowExceptionsAfterReporting = RollbarDeveloperOptions.defaultRethrowExceptionsAfterReporting)
            : this(logLevel, enabled, transmit, rethrowExceptionsAfterReporting, RollbarDeveloperOptions.defaultPayloadPostTimeout)
        {
        }

        public RollbarDeveloperOptions(
            ErrorLevel logLevel, 
            bool enabled, 
            bool transmit, 
            bool rethrowExceptionsAfterReporting, 
            TimeSpan payloadPostTimeout)
        {
            LogLevel = logLevel;
            Enabled = enabled;
            Transmit = transmit;
            RethrowExceptionsAfterReporting = rethrowExceptionsAfterReporting;
            PayloadPostTimeout = payloadPostTimeout;
        }

        public ErrorLevel LogLevel
        {
            get;
            set;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public bool Transmit
        {
            get;
            set;
        }

        public bool RethrowExceptionsAfterReporting
        {
            get;
            set;
        }

        public TimeSpan PayloadPostTimeout
        {
            get;
        }

        public IRollbarDeveloperOptions Reconfigure(IRollbarDeveloperOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

        public override Validator GetValidator()
        {
            return null;
        }
    }
}
