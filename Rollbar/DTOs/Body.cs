namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;

    public class Body
        : DtoBase
    {
        public Body(IEnumerable<System.Exception> exceptions)
        {
            if (exceptions == null)
            {
                throw new ArgumentNullException(nameof(exceptions));
            }

            var allExceptions = exceptions as System.Exception[] ?? exceptions.ToArray();
            if (!allExceptions.Any())
            {
                throw new ArgumentException("Trace Chains must have at least one Trace", nameof(exceptions));
            }

            TraceChain = allExceptions.Select(e => new Trace(e)).ToArray();

            Validate();
        }

        public Body(AggregateException exception) 
            : this(exception.InnerExceptions)
        {
        }

        public Body(System.Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (exception.InnerException != null)
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

        public Body(Message message)
        {
            Message = message;
            Validate();
        }

        public Body(string crashReport)
        {
            this.CrashReport = new CrashReport(crashReport);
            Validate();
        }

        #region These are mutually exclusive properties - only one of them can be not null
        
        [JsonProperty("trace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Trace Trace { get; private set; }

        [JsonProperty("trace_chain", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Trace[] TraceChain { get; private set; }

        [JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Message Message { get; private set; }

        [JsonProperty("crash_report", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CrashReport CrashReport { get; private set; }

        #endregion These are mutually exclusive properties - only one of them can be not null

        public override void Validate()
        {
            int bodyContentVariationsCount = 0;

            if (this.Trace != null)
            {
                this.Trace.Validate();
                bodyContentVariationsCount++;
            }
            if (this.TraceChain != null)
            {
                //this.TraceChain.va
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

            Assumption.AssertEqual(bodyContentVariationsCount, 1, nameof(bodyContentVariationsCount));
        }
    }
}
