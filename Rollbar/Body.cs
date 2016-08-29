using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RollbarDotNet 
{
    public class Body 
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
        }

#if NETFX_45
        public Body(AggregateException exception) : this(exception.InnerExceptions) 
        {
        }
#endif

        public Body(System.Exception exception) 
        {
            if (exception == null) 
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var exceptionList = new List<System.Exception>();
            if (exception.InnerException != null)
            {
                exceptionList.Add(exception);
                var currentException = exception;
                while (currentException.InnerException != null)
                {
                    exceptionList.Add(currentException.InnerException);
                    currentException = currentException.InnerException;
                }

                TraceChain = exceptionList.Select(e => new Trace(e)).ToArray();
            }
            else
            {
                Trace = new Trace(exception);
            }
        }

        public Body(Message message) 
        {
            if (message == null) 
            {
                throw new ArgumentNullException(nameof(message));
            }

            Message = message;
        }

        public Body(string crashReport) 
        {
            if (string.IsNullOrEmpty(crashReport?.Trim())) 
            {
                throw new ArgumentNullException(nameof(crashReport));
            }

            CrashReport = new Dictionary<string, string> 
            {
                { "raw", crashReport }
            };
        }

        [JsonProperty("trace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Trace Trace { get; private set; }

        [JsonProperty("trace_chain", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Trace[] TraceChain { get; private set; }

        [JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Message Message { get; private set; }

        [JsonProperty("crash_report", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, string> CrashReport { get; private set; }
    }
}
