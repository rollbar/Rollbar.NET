using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RollbarDotNet {
    public class Body {
        public Body(IEnumerable<System.Exception> exceptions) {
            if (exceptions == null) {
                throw new ArgumentNullException("exceptions");
            }
            var allExceptions = exceptions as System.Exception[] ?? exceptions.ToArray();
            if (!allExceptions.Any()) {
                throw new ArgumentException("Trace Chains must have at least one Trace", "exceptions");
            }
            TraceChain = allExceptions.Select(e => new Trace(e)).ToArray();
        }

        public Body(AggregateException exception) : this(exception.InnerExceptions) {
        }

        public Body(System.Exception exception) {
            if (exception == null) {
                throw new ArgumentNullException("exception");
            }
            Trace = new Trace(exception);
        }

        public Body(Message message) {
            if (message == null) {
                throw new ArgumentNullException("message");
            }
            Message = message;
        }

        public Body(string crashReport) {
            if (string.IsNullOrWhiteSpace(crashReport)) {
                throw new ArgumentNullException("crashReport");
            }
            CrashReport = new Dictionary<string, string> {
                { "raw", crashReport },
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
