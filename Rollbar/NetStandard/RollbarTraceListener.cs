namespace Rollbar.NetStandard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

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
    /// &lt;?xml version="1.0" encoding="utf-8"?&gt;
    ///  &lt;configuration&gt;
    ///    &lt;!--
    ///    &lt;configSections&gt;
    ///      &lt;section name = "rollbar" type="Rollbar.NetFramework.RollbarConfigSection, Rollbar"/&gt;
    ///    &lt;/configSections&gt;
    ///      
    ///    &lt;rollbar
    ///      accessToken = "17965fa5041749b6bf7095a190001ded"
    ///      environment="RollbarNetPrototypes"
    ///      /?&gt;
    ///     --?&gt;
    ///   
    ///    &lt;system.diagnostics>
    ///      &lt;trace autoflush = "true" indentsize="4"&gt;
    ///        &lt;listeners>
    ///          &lt;add name = "textFileListener"
    ///               type="System.Diagnostics.TextWriterTraceListener" 
    ///               initializeData="TextTrace.log" 
    ///               traceOutputOptions="ProcessId, ThreadId, Timestamp, DateTime, Callstack,LogicalOperationStack"
    ///               /&gt;
    ///          &lt;add name = "rollbarListener"
    ///               type="Rollbar.NetStandard.RollbarTraceListener,Rollbar" 
    ///               traceOutputOptions="ProcessId, ThreadId, Timestamp, DateTime, Callstack, LogicalOperationStack" 
    ///               rollbarAccessToken="17965fa5041749b6bf7095a190001ded" 
    ///               rollbarEnvironment="MyRollbarEnvironmentTag"
    ///               /&gt;
    ///          &lt;remove name = "Default"
    ///                  /&gt;
    ///        &lt;/listeners&gt;
    ///      &lt;/trace&gt;
    ///    &lt;/system.diagnostics&gt;
    ///      &lt;startup&gt;
    ///          &lt;supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.1"/&gt;
    ///      &lt;/startup&gt;
    ///  &lt;/configuration&gt;
    ///
    /// </example>
    public class RollbarTraceListener
        : TraceListener
    {
        private static object typeSyncRoot = new object();

        /// <summary>
        /// The instance count
        /// </summary>
        public static int InstanceCount { get; set; }

        private IRollbar? _rollbar;

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
                    lock(typeSyncRoot)
                    {
                        if(this._rollbar == null)
                        {
                            var config = this.GetRollbarTraceListenerConfig();
                            if(config != null)
                            {
                                this._rollbar = RollbarFactory.CreateNew(config);
                            }
                            else
                            {
                                // the best thing we can do at this point is to start using 
                                // the Rollbar singleton hoping it was pre-configured by now:
                                this._rollbar = RollbarLocator.RollbarInstance;
                            }
                        }
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
        public RollbarTraceListener(string rollbarAccessToken, string? rollbarEnvironment)
        {
            this.Attributes[RollbarTraceListenerAttributes.rollbarAccessToken.ToString()] = rollbarAccessToken;
            this.Attributes[RollbarTraceListenerAttributes.rollbarEnvironment.ToString()] = rollbarEnvironment;

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
        public override void Write(string? message)
        {
            this.WriteLine(message);
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string? message)
        {
            if (this.Rollbar != null && message != null && !string.IsNullOrWhiteSpace(message))
            {
                this.Rollbar.Info(message);
            }
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"></see> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"></see> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
        public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? message)
        {
            if (this.Rollbar == null || message == null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            // the code below is simplified way to report events to Rollbar API,
            // in production code we can do better job mapping the available event data
            // into proper Rollbar data body:
            var custom = new Dictionary<string, object?>();
            if (eventCache != null)
            {
                custom["callStack"] = eventCache.Callstack;
                custom["logicalOperationStack"] = eventCache.LogicalOperationStack;
                custom["processID"] = eventCache.ProcessId;
                custom["threadID"] = eventCache.ThreadId;
                custom["timestamp"] = eventCache.Timestamp;
                custom["dateTime"] = eventCache.DateTime;
                custom["eventType"] = eventType;
                custom["eventMessage"] = message;

                if (!string.IsNullOrWhiteSpace(eventCache.Callstack)
#pragma warning disable CA1307 // Specify StringComparison for clarity
                && (message.Contains("Exception: ") || (eventType == TraceEventType.Critical) || (eventType == TraceEventType.Error)))
#pragma warning restore CA1307 // Specify StringComparison for clarity
                {
                    DTOs.Body body = new DTOs.Body(new DTOs.Trace(eventCache.Callstack, message));
                    DTOs.Data data = new DTOs.Data(this.Rollbar.Config, body, custom);
                    data.Level = RollbarTraceListener.Translate(eventType);
                    this.Rollbar.Log(data);
                    return;
                }
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
                    this.Rollbar.Critical(message, custom);
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
        private IRollbarLoggerConfig? GetRollbarTraceListenerConfig()
        {
            if(string.IsNullOrWhiteSpace(this.Attributes[RollbarTraceListenerAttributes.rollbarAccessToken.ToString()]))
            {
                return null;
            }

            if(!RollbarInfrastructure.Instance.IsInitialized)
            {

#if !NETFX_47nOlder
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
                {
                    // We are running within Blazor WASM runtime environment that is known to be single threaded in its nature
                    // at least at the moment, so no background threads are allowed and our infrastructure depends on the threads.
                    // Hence, we can not initialize the infrastructure:
                    // NO-OP...
                    return null;
                }
#endif

                // It is safe to assume we can use the infrastructure:
                RollbarInfrastructureConfig rollbarInfrastructureConfig = new RollbarInfrastructureConfig(
                    this.Attributes[RollbarTraceListenerAttributes.rollbarAccessToken.ToString()],
                    this.Attributes[RollbarTraceListenerAttributes.rollbarEnvironment.ToString()]
                    );
                RollbarInfrastructure.Instance.Init(rollbarInfrastructureConfig);
                return rollbarInfrastructureConfig.RollbarLoggerConfig;
            }
            else
            {
                RollbarLoggerConfig rollbarLoggerConfig = new RollbarLoggerConfig(
                    this.Attributes[RollbarTraceListenerAttributes.rollbarAccessToken.ToString()],
                    this.Attributes[RollbarTraceListenerAttributes.rollbarEnvironment.ToString()]
                    );
                return rollbarLoggerConfig;
            }
        }

        /// <summary>
        /// Gets the custom attributes supported by the trace listener.
        /// </summary>
        /// <returns>A string array naming the custom attributes supported by the trace listener, or null if there are no custom attributes.</returns>
        protected override string[] GetSupportedAttributes()
        {
            return new [] {
                RollbarTraceListenerAttributes.rollbarAccessToken.ToString(),
                RollbarTraceListenerAttributes.rollbarEnvironment.ToString(),
            };
        }

    }
}
