using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rollbar
{
    public class RollbarTraceListener
        : TraceListener
    {
        private RollbarConfig GetRollbarConfig()
        {
            var config = new RollbarConfig(this.Attributes["rollbarAccessToken"]) // minimally required Rollbar configuration
            {
                Environment = this.Attributes["rollbarEnvironment"],
            };

            return config;
        }

        protected override string[] GetSupportedAttributes()
        {
            //return base.GetSupportedAttributes();
            return new string[] { "rollbarAccessToken", "rollbarEnvironment", };
        }

        public override void Write(string message)
        {
            //throw new NotImplementedException();
        }

        public override void WriteLine(string message)
        {
            RollbarLocator.RollbarInstance.Configure(this.GetRollbarConfig());
            RollbarLocator.RollbarInstance.Info(message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            //base.TraceEvent(eventCache, source, eventType, id, message);

            // the code below is simplified way to report events to Rollbar API,
            // in production code we can do better job mapping the available event data
            // into proper Rollbar data body:
            var custom = new Dictionary<string, object>();
            custom["callStack"] = eventCache.Callstack;
            custom["logicalOperationStack"] = eventCache.LogicalOperationStack;
            custom["processID"] = eventCache.ProcessId;
            custom["threadID"] = eventCache.ThreadId;
            custom["eventType"] = eventType;
            custom["eventMessage"] = message;

            switch (eventType)
            {
                case TraceEventType.Critical:
                    RollbarLocator.RollbarInstance.Critical(message, custom);
                    break;
                case TraceEventType.Error:
                    RollbarLocator.RollbarInstance.Error(message, custom);
                    break;
                case TraceEventType.Warning:
                    RollbarLocator.RollbarInstance.Warning(message, custom);
                    break;
                case TraceEventType.Information:
                    RollbarLocator.RollbarInstance.Info(message, custom);
                    break;
                default:
                    break;
            }
        }

    }
}
