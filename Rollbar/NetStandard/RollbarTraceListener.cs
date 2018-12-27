namespace Rollbar.NetStandard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

#pragma warning disable CS1570 // XML comment has badly formed XML
    /// <summary>
    /// Class RollbarTraceListener.
    /// </summary>
    /// <seealso cref="System.Diagnostics.TraceListener" />
    /// <remarks>
    /// This class implements a Rollbar Notifier's plug-in for the standard .NET tracing infrastructure.
    /// It, internally, uses the RollbarLogger to asynchronously forward the "herd" tracing events to
    /// the Rollbar API Service.
    /// So, if your codebase is already using the .NET tracing extensively, you can gain Rollbar remote
    /// error/log monitoring benefits by simply adding RollbarTraceListener as one more trace listener.
    /// This listener instance can either be added via code or via app.config file. In the app.config file,
    /// the listener can be either configured on very basic level by specifying the access token and environment
    /// attributes or by using more advanced configuration parameters via dedicated Rollbar configuration
    /// section of app.config file.
    /// Since the .NET does not seem to support yet addition of trace listeners via appsettings.json file,
    /// this trace listener can be added only via code, but configured either via code or via appropriate
    /// Rollbar section of the appsettings.json file.
    /// </remarks>
    /// <example>
    /// 
    /// <?xml version="1.0" encoding="utf-8"?>
    ///  <configuration>
    ///    <!--
    ///    <configSections>
    ///      <section name = "rollbar" type="Rollbar.NetFramework.RollbarConfigSection, Rollbar"/>
    ///    </configSections>
    ///      
    ///    <rollbar
    ///      accessToken = "17965fa5041749b6bf7095a190001ded"
    ///      environment="RollbarNetPrototypes"
    ///      />
    ///     -->
    ///   
    ///    <system.diagnostics>
    ///      <trace autoflush = "true" indentsize="4">
    ///        <listeners>
    ///          <add name = "textFileListener"
    ///               type="System.Diagnostics.TextWriterTraceListener" 
    ///               initializeData="TextTrace.log" 
    ///               traceOutputOptions="ProcessId, ThreadId, Timestamp, DateTime, Callstack,LogicalOperationStack"
    ///               />
    ///          <add name = "rollbarListener"
    ///               type="Rollbar.NetStandard.RollbarTraceListener,Rollbar" 
    ///               traceOutputOptions="ProcessId, ThreadId, Timestamp, DateTime, Callstack, LogicalOperationStack" 
    ///               rollbarAccessToken="17965fa5041749b6bf7095a190001ded" 
    ///               rollbarEnvironment="MyRollbarEnvironmentTag"
    ///               />
    ///          <remove name = "Default"
    ///                  />
    ///        </listeners>
    ///      </trace >
    ///    </system.diagnostics>
    ///      <startup>
    ///          <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.1"/>
    ///      </startup>
    ///  </configuration>
    ///
    /// </example>
#pragma warning restore CS1570 // XML comment has badly formed XML
    public class RollbarTraceListener
        : TraceListener
    {
        private static object typeSyncRoot = new object();

        /// <summary>
        /// The instance count
        /// </summary>
        public static int InstanceCount = 0;

        private IRollbar _rollbar = null;

        /// <summary>
        /// Gets the Rollbar.
        /// </summary>
        /// <value>The rollbar.</value>
        protected IRollbar Rollbar
        {
            get
            {
                if (this._rollbar == null)
                {
                    var config = this.GetRollbarTraceListenerConfig();
                    if (config != null)
                    {
                        this._rollbar = RollbarFactory.CreateNew();
                        this._rollbar.Configure(config);
                    }
                    else
                    {
                        // the best thing we can do at this point is to start using 
                        // the Rollbar singleton hoping it was pre-configured by now:
                        this._rollbar = RollbarLocator.RollbarInstance;
                    }
                }

                return this._rollbar;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarTraceListener"/> class.
        /// </summary>
        public RollbarTraceListener()
        {
            var config = this.GetRollbarTraceListenerConfig();
            if (config != null)
            {
                this._rollbar = RollbarFactory.CreateNew();
                this._rollbar.Configure(config);
            }

            lock(typeSyncRoot)
            {
                RollbarTraceListener.InstanceCount++;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarTraceListener"/> class.
        /// </summary>
        /// <param name="rollbarAccessToken">The rollbar access token.</param>
        public RollbarTraceListener(string rollbarAccessToken)
            : this(rollbarAccessToken, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarTraceListener"/> class.
        /// </summary>
        /// <param name="rollbarAccessToken">The rollbar access token.</param>
        /// <param name="rollbarEnvironment">The rollbar environment.</param>
        public RollbarTraceListener(string rollbarAccessToken, string rollbarEnvironment)
        {
            if (!string.IsNullOrWhiteSpace(rollbarAccessToken))
            {
                RollbarConfig config = new RollbarConfig(rollbarAccessToken);
                if (!string.IsNullOrWhiteSpace(rollbarEnvironment))
                {
                    config.Environment = rollbarEnvironment;
                }
                this._rollbar = RollbarFactory.CreateNew();
                this._rollbar.Configure(config);
            }
            lock (typeSyncRoot)
            {
                RollbarTraceListener.InstanceCount++;
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(string message)
        {
            this.WriteLine(message);
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string message)
        {
            this.Rollbar.Info(message);
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"></see> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"></see> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
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
            custom["timestamp"] = eventCache.Timestamp;
            custom["dateTime"] = eventCache.DateTime;
            custom["eventType"] = eventType;
            custom["eventMessage"] = message;

            if (!string.IsNullOrWhiteSpace(eventCache.Callstack) 
                && (message.Contains("Exception: ") || (eventType == TraceEventType.Critical) || (eventType == TraceEventType.Error)))
            {
                DTOs.Body body = new DTOs.Body(new DTOs.Trace(eventCache.Callstack, message));
                DTOs.Data data = new DTOs.Data(this.Rollbar.Config, body, custom);
                data.Level = RollbarTraceListener.Translate(eventType);
                this.Rollbar.Log(data);
                return;
            }
           
            switch (eventType)
            {
                case TraceEventType.Critical:
                    this.Rollbar.Critical(message, custom);
                    break;
                case TraceEventType.Error:
                    this.Rollbar.Error(message, custom);
                    break;
                case TraceEventType.Warning:
                    this.Rollbar.Warning(message, custom);
                    break;
                case TraceEventType.Information:
                    this.Rollbar.Info(message, custom);
                    break;
                default:
                    break;
            }
        }

        private static ErrorLevel Translate(TraceEventType traceEventType)
        {
            switch (traceEventType)
            {
                case TraceEventType.Critical:
                    return ErrorLevel.Critical;
                case TraceEventType.Error:
                    return ErrorLevel.Error;
                case TraceEventType.Warning:
                    return ErrorLevel.Warning;
                case TraceEventType.Information:
                    return ErrorLevel.Info;
                default:
                    return ErrorLevel.Critical;
            }
        }

        private enum RollbarTraceListenerAttributes
        {
            rollbarAccessToken,
            rollbarEnvironment,
        }

        /// <summary>
        /// Gets the Rollbar trace listener configuration.
        /// </summary>
        /// <returns>RollbarConfig.</returns>
        private RollbarConfig GetRollbarTraceListenerConfig()
        {
            if (string.IsNullOrWhiteSpace(this.Attributes[RollbarTraceListenerAttributes.rollbarAccessToken.ToString()]))
            {
                return null;
            }

            var config = new RollbarConfig(this.Attributes[RollbarTraceListenerAttributes.rollbarAccessToken.ToString()]) // minimally required Rollbar configuration
            {
                Environment = this.Attributes[RollbarTraceListenerAttributes.rollbarEnvironment.ToString()],
            };

            return config;
        }

        /// <summary>
        /// Gets the custom attributes supported by the trace listener.
        /// </summary>
        /// <returns>A string array naming the custom attributes supported by the trace listener, or null if there are no custom attributes.</returns>
        protected override string[] GetSupportedAttributes()
        {
            //return base.GetSupportedAttributes();
            return new string[] {
                RollbarTraceListenerAttributes.rollbarAccessToken.ToString(),
                RollbarTraceListenerAttributes.rollbarEnvironment.ToString(),
            };
        }

    }
}
