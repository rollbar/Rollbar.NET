namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Rollbar.Common;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Models Rollbar Body DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    public class Body
        : DtoBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Body"/> class.
        /// </summary>
        /// <param name="trace">The trace.</param>
        internal Body(Trace trace)
        {
            Assumption.AssertNotNull(trace, nameof(trace));

            this.Trace = trace;

            Validate();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Body"/> class.
        /// </summary>
        /// <param name="exceptions">The exceptions.</param>
        public Body(IEnumerable<System.Exception> exceptions)
        {
            Assumption.AssertNotNullOrEmpty(exceptions, nameof(exceptions));

            if (exceptions.Count() > 1)
            {
                this.OriginalException = new AggregateException(exceptions);
            }
            else
            {
                this.OriginalException = exceptions.FirstOrDefault();
            }

            var allExceptions = exceptions as System.Exception[] ?? exceptions.ToArray();
            TraceChain = allExceptions.Select(e => new Trace(e)).ToArray();

            Validate();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Body"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public Body(System.Exception exception)
        {
            Assumption.AssertNotNull(exception, nameof(exception));

            this.OriginalException = exception;

            AggregateException? aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                TraceChain = aggregateException.InnerExceptions.Select(e => new Trace(e)).ToArray();
            }
            else if (exception.InnerException != null)
            {
                var exceptionList = new List<System.Exception>();
                var outerException = exception;
                while (outerException != null)
                {
                    exceptionList.Add(outerException);
                    outerException = outerException.InnerException;
                }
                TraceChain = exceptionList.Select(e => new Trace(e)).ToArray();
            }
            else
            {
                Trace = new Trace(exception);
            }

            Validate();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Body"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public Body(Message message)
        {
            Assumption.AssertNotNull(message, nameof(message));

            Message = message;
            Validate();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Body"/> class.
        /// </summary>
        /// <param name="crashReport">The crash report.</param>
        public Body(string crashReport)
        {
            Assumption.AssertNotNullOrWhiteSpace(crashReport, nameof(crashReport));

            this.CrashReport = new CrashReport(crashReport);
            Validate();
        }

        [JsonIgnore]
        internal readonly System.Exception? OriginalException = null;

        #region These are mutually exclusive properties - only one of them can be not null

        /// <summary>
        /// Gets the optional telemetry.
        /// </summary>
        /// <value>
        /// The telemetry.
        /// </value>
        [JsonProperty("telemetry", 
            Required = Required.AllowNull, 
            DefaultValueHandling = DefaultValueHandling.Ignore
            )]
        public Telemetry[]? Telemetry { get; internal set; }

        /// <summary>
        /// Gets the trace.
        /// </summary>
        /// <value>
        /// The trace.
        /// </value>
        [JsonProperty("trace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Trace? Trace { get; private set; }

        /// <summary>
        /// Gets the trace chain.
        /// </summary>
        /// <value>
        /// The trace chain.
        /// </value>
        [JsonProperty("trace_chain", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Trace[]? TraceChain { get; private set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Message? Message { get; private set; }

        /// <summary>
        /// Gets the crash report.
        /// </summary>
        /// <value>
        /// The crash report.
        /// </value>
        [JsonProperty("crash_report", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CrashReport? CrashReport { get; private set; }

        #endregion These are mutually exclusive properties - only one of them can be not null

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator GetValidator()
        {
            var validator = new Validator<Body, Body.BodyValidationRule>()
                    .AddValidation(
                        Body.BodyValidationRule.OnlyOneBodyContentRequired,
                        (body) => {
                            const int expectedBodyContentVariationsCount = 1;
                            int bodyContentVariationsCount = 0;

                            if (this.Trace != null)
                            {
                                this.Trace.Validate();
                                bodyContentVariationsCount++;
                            }
                            if (this.TraceChain != null)
                            {
                                bodyContentVariationsCount++;
                            }
                            if (this.Message != null)
                            {
                                this.Message.Validate();
                                bodyContentVariationsCount++;
                            }
                            if (this.CrashReport != null)
                            {
                                bodyContentVariationsCount++;
                            }

                            return (bodyContentVariationsCount == expectedBodyContentVariationsCount);
                        }
                        )
                    .AddValidation(
                        Body.BodyValidationRule.ValidCrashReportIfAny,
                        (body) => body.CrashReport,
                        this.CrashReport?.GetValidator() as Validator<CrashReport?>
                        )
                    .AddValidation(
                        Body.BodyValidationRule.ValidMessageIfAny,
                        (body) => body.Message,
                        this.Message?.GetValidator() as Validator<Message?>
                        )
                    .AddValidation(
                        Body.BodyValidationRule.ValidTraceIfAny,
                        (body) => body.Trace,
                        this.Trace?.GetValidator() as Validator<Trace?>
                        )
                    .AddValidation(
                        Body.BodyValidationRule.ValidTraceChainIfAny,
                        (body) => {
                            if (body.TraceChain == null)
                            {
                                return true; // it is OK not to have one....
                            }
                            const int minExpectedTraceChainLength = 1;
                            return (body.TraceChain.Length > minExpectedTraceChainLength);
                        }
                        )
                    ;

            return validator;
        }

        /// <summary>
        /// Enum BodyValidationRule
        /// </summary>
        public enum BodyValidationRule
        {
            /// <summary>
            /// The only one body content required
            /// </summary>
            OnlyOneBodyContentRequired,

            /// <summary>
            /// The valid crash report if any
            /// </summary>
            ValidCrashReportIfAny,

            /// <summary>
            /// The valid message if any
            /// </summary>
            ValidMessageIfAny,

            /// <summary>
            /// The valid trace if any
            /// </summary>
            ValidTraceIfAny,

            /// <summary>
            /// The valid trace chain if any
            /// </summary>
            ValidTraceChainIfAny,
        }
    }
}
