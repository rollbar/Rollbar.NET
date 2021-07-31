namespace Sample.NetCore.ConsoleApp
{
    using Rollbar;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Rollbar.Common;
    using Samples;

    class Program
    {
        static void Main(string[] args)
        {
            InitEventCounters();

            ConfigureRollbarSingleton();

            DemonstrateRollbarUsage();
            //ManualPayloadPersistenceDemo();

            Thread.Sleep(TimeSpan.FromSeconds(10));

            PrintEvents();
            PrintEventCounters();
        }

        /// <summary>
        /// Demonstrates the Rollbar usage.
        /// </summary>
        private static void DemonstrateRollbarUsage()
        {
            RollbarInfrastructure.Instance.TelemetryCollector.Capture(
                new Telemetry(
                    TelemetrySource.Client, 
                    TelemetryLevel.Info, 
                    new LogTelemetry("Info log telemetry")
                    )
                );

            Dictionary<string, object> customFields = new Dictionary<string, object>();
            customFields.Add("Hebrew", "אספירין");
            customFields.Add("Hindi", "एस्पिरि");
            customFields.Add("Chinese", "阿司匹林");
            customFields.Add("Japanese", "アセチルサリチル酸");
            customFields.Add("path1", "d:\\Work\\\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\\branches\\v2\\...");
            customFields.Add("path2", @"d:\Work\אספירין\branches\v2\...");

            var exceptionWithData = new ArgumentNullException("Exception with Data");
            exceptionWithData.Data["argumentName"] = "requiredOne";

            int count = 3;
            while(count-- > 0)
            {

                RollbarLocator.RollbarInstance
                    .Info($"ConsoleApp sample: Basic info log example @ {DateTime.Now} - {count}.", customFields);
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }


            RollbarInfrastructure.Instance.TelemetryCollector.Capture(
                new Telemetry(
                    TelemetrySource.Client,
                    TelemetryLevel.Info,
                    new LogTelemetry("Something interesting happened.")
                    )
                );
            RollbarLocator.RollbarInstance
                .Debug("ConsoleApp sample: First debug log.");

            RollbarInfrastructure.Instance.TelemetryCollector.Capture(
                new Telemetry(
                    TelemetrySource.Client,
                    TelemetryLevel.Error,
                    new ErrorTelemetry(new System.Exception("Worth mentioning!"))
                    )
                );
            RollbarLocator.RollbarInstance
                .Error(new NullReferenceException("ConsoleApp sample: null reference exception."));

            RollbarInfrastructure.Instance.TelemetryCollector.Capture(
                new Telemetry(
                    TelemetrySource.Client,
                    TelemetryLevel.Error,
                    new ManualTelemetry(new Dictionary<string, object>() { { "somthing", "happened" }, })
                    )
                );
            RollbarLocator.RollbarInstance
                .Error(new System.Exception("ConsoleApp sample: trying out the TraceChain", new NullReferenceException()));


            RollbarInfrastructure.Instance.TelemetryCollector.Capture(
                new Telemetry(
                    TelemetrySource.Client,
                    TelemetryLevel.Error,
                    new ManualTelemetry(new Dictionary<string, object>() { { "param1", "value1" }, { "param2", "value2" }, })
                    )
                );
            RollbarLocator.RollbarInstance
                .Error(exceptionWithData, customFields)
                ;

            var demoObj = new InstanceType();
            demoObj.DemonstrateStateCapture();

            Stopwatch stopwatch = Stopwatch.StartNew();
            RollbarLocator.RollbarInstance
                .Info("Via no-blocking mechanism.")
                ;
            stopwatch.Stop();
            string msg = "*** 1. No-blocking report took " + stopwatch.Elapsed.TotalMilliseconds + " [msec].";
            System.Diagnostics.Trace.WriteLine(msg);
            Console.WriteLine(msg);

            stopwatch = Stopwatch.StartNew();
            try
            {
                RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromMilliseconds(10000))
                    .Info("Via blocking mechanism.")
                    ;
            }
            catch(System.TimeoutException ex)
            {
                msg = "*** Blocking call with too short timeout. Exception: " + Environment.NewLine + ex;
                System.Diagnostics.Trace.WriteLine(msg);
                Console.WriteLine(msg);
            }
            stopwatch.Stop();
            msg = "*** 2. Blocking (long timeout) report took " + stopwatch.Elapsed.TotalMilliseconds + " [msec].";
            System.Diagnostics.Trace.WriteLine(msg);
            Console.WriteLine(msg);

            stopwatch = Stopwatch.StartNew();
            try
            {
                RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromMilliseconds(500))
                    .Info("Via blocking mechanism with short timeout.")
                    ;
            }
            catch(System.TimeoutException ex)
            {
                msg = "*** 3. Blocking call with too short timeout. Exception: " + Environment.NewLine + ex;
                System.Diagnostics.Trace.WriteLine(msg);
                Console.WriteLine(msg);
            }
            stopwatch.Stop();
            msg = "*** 3. Blocking (short timeout) report took " + stopwatch.Elapsed.TotalMilliseconds + " [msec].";
            System.Diagnostics.Trace.WriteLine(msg);
            Console.WriteLine(msg);
        }

        /// <summary>
        /// Manuals the payload persistence demo.
        /// </summary>
        /// <remarks>
        /// Keep connecting/disconnecting network cable during this demo run while counters keeps increasing.
        /// Connect the cable after the counter stops and until the stats are printed.
        /// All the payloads (2 X maxCounterValue) are expected to be delivered to Rollbar API by the time the stats are printed.
        /// </remarks>
        private static void ManualPayloadPersistenceDemo() 
        {
            int count = 0;
            while (count++ < 30)
            {
                Console.WriteLine(count);
                RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromMilliseconds(9000))
                    .Info("Pumping payloads via blocking mechanism with short timeout.")
                    ;
                RollbarLocator.RollbarInstance
                    .Info("Pumping payloads via async mechanism with short timeout.")
                    ;

                Thread.Sleep(TimeSpan.FromMilliseconds(500));
            }

            Thread.Sleep(TimeSpan.FromSeconds(10));
        }


        /// <summary>
        /// Configures the Rollbar singleton-like notifier.
        /// </summary>
        private static void ConfigureRollbarSingleton()
        {
            // minimally required Rollbar configuration:
            RollbarInfrastructureConfig config = 
                new RollbarInfrastructureConfig(
                    RollbarSamplesSettings.AccessToken, 
                    RollbarSamplesSettings.Environment
                    );

            // optional:
            RollbarOfflineStoreOptions offlineStoreOptions = new RollbarOfflineStoreOptions();
            offlineStoreOptions.EnableLocalPayloadStore = true;
            config.RollbarOfflineStoreOptions.Reconfigure(offlineStoreOptions);

            // optional:
            RollbarTelemetryOptions telemetryOptions = new RollbarTelemetryOptions(true, 3);
            config.RollbarTelemetryOptions.Reconfigure(telemetryOptions);

            // optional:
            //HttpProxyOptions proxyOptions = new HttpProxyOptions("http://something.com");
            //config.RollbarLoggerConfig.HttpProxyOptions.Reconfigure(proxyOptions);

            // optional:
            RollbarDataSecurityOptions dataSecurityOptions = new RollbarDataSecurityOptions();
            dataSecurityOptions.ScrubFields = new string[]
            {
                "access_token", // normally, you do not want scrub this specific field (it is operationally critical), but it just proves safety net built into the notifier... 
                "username",
                "criticalObj[Sample.NetCore.ConsoleApp.Program+InstanceType]._baseNullField",
                "data.custom.criticalObj[Sample.NetCore.ConsoleApp.Program+InstanceType].<TypeName>k__BackingField",
            };
            config.RollbarLoggerConfig.RollbarDataSecurityOptions.Reconfigure(dataSecurityOptions);

            // optional:
            RollbarPayloadAdditionOptions payloadAdditionOptions = new RollbarPayloadAdditionOptions();
            payloadAdditionOptions.Person = new Person() 
            { 
                Id = "007", 
                Email = "jbond@mi6.uk", 
                UserName = "JBOND", 
            };
            config.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Reconfigure(payloadAdditionOptions);

            // initialize the Rollbar Infrastructure:
            RollbarInfrastructure.Instance.Init(config);

            // optional step if you would like to monitor all Rollbar instances' internal events within your application:
            RollbarInfrastructure.Instance.QueueController.InternalEvent += OnRollbarInternalEvent;

            // optional step if you would like to monitor this Rollbar instance's internal events within your application:
            //RollbarLocator.RollbarInstance.InternalEvent += OnRollbarInternalEvent;
        }

        /// <summary>
        /// Called when rollbar internal event is detected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        private static void OnRollbarInternalEvent(object sender, RollbarEventArgs e)
        {
            //Console.WriteLine(e.TraceAsString());

            switch (e)
            {
                case InternalErrorEventArgs rollbarEvent:
                    // handle this specific type of Rollbar event...
                    break;
                case RollbarApiErrorEventArgs rollbarEvent:
                    // handle this specific type of Rollbar event...
                    break;
                case CommunicationErrorEventArgs rollbarEvent:
                    // handle this specific type of Rollbar event...
                    break;
                case TransmissionOmittedEventArgs rollbarEvent:
                    // handle this specific type of Rollbar event...
                    break;
                case PayloadDropEventArgs rollbarEvent:
                    // handle this specific type of Rollbar event...
                    break;
                case CommunicationEventArgs rollbarEvent:
                    // handle this specific type of Rollbar event...
                    break;
                default:
                    // handle this specific type of Rollbar event...
                    break;
            }

            _eventCounters[e.GetType().Name].Add(e);
        }

        private static readonly Dictionary<string, List<RollbarEventArgs>> _eventCounters = 
            new Dictionary<string, List<RollbarEventArgs>>();

        /// <summary>
        /// Initializes the event counters.
        /// </summary>
        private static void InitEventCounters()
        {
            _eventCounters[typeof(InternalErrorEventArgs).Name] = new List<RollbarEventArgs>();
            _eventCounters[typeof(RollbarApiErrorEventArgs).Name] = new List<RollbarEventArgs>();
            _eventCounters[typeof(CommunicationErrorEventArgs).Name] = new List<RollbarEventArgs>();
            _eventCounters[typeof(TransmissionOmittedEventArgs).Name] = new List<RollbarEventArgs>();
            _eventCounters[typeof(PayloadDropEventArgs).Name] = new List<RollbarEventArgs>();
            _eventCounters[typeof(CommunicationEventArgs).Name] = new List<RollbarEventArgs>();
        }

        /// <summary>
        /// Prints the event counters.
        /// </summary>
        private static void PrintEventCounters() 
        {
            foreach (var eventCounterKey in _eventCounters.Keys)
            {
                Console.WriteLine(eventCounterKey + " = " + _eventCounters[eventCounterKey].Count);
            }
        }

        /// <summary>
        /// Prints the events.
        /// </summary>
        private static void PrintEvents() 
        {
            foreach (var eventCounterKey in _eventCounters.Keys)
            {
                Console.WriteLine("==============================");
                Console.WriteLine(eventCounterKey.ToUpper() + ":");
                Console.WriteLine(eventCounterKey + " = " + _eventCounters[eventCounterKey].Count);
                foreach (var @event in  _eventCounters[eventCounterKey])
                {
                    Console.WriteLine(@event.TraceAsString());
                }
                Console.WriteLine("==============================");
            }
        }

        #region data mocks

        static class StaticType
        {
            // 1
            private const int BaseConstant = 10;

            // 2
            private static int _baseIntField = 3;

            // 3
            public static object BaseNullProperty
            {
                get { return StaticType._baseNullField; }
            }
            private static object _baseNullField = null;

            // 4
            public static string BaseAutoProperty { get; set; } = "BaseAutoProperty value";

        }

        class InstanceType
            : InstanceTypeBase
        {
            // 1
            public int AutoProperty { get; set; } = 99;

            // 2
            public static string TypeName { get; } = nameof(InstanceType);

            public void DemonstrateStateCapture()
            {
                var criticalObj = new InstanceType();
                criticalObj.AutoProperty = -100;

                try
                {
                    ///...
                    /// oh, no - we have an exception:
                    throw new System.Exception("An exception with state capture!");
                    ///...
                }
                catch (System.Exception ex)
                {
                    // capture state of this instance:
                    var state = RollbarAssistant.CaptureState(this, "Self"); 
                    // also, capture state of some other critical object:
                    state = new Dictionary<string, object>(state.Concat(RollbarAssistant.CaptureState(criticalObj, nameof(criticalObj))));
                    // also, capture current state of a static type:
                    state = new Dictionary<string, object>(state.Concat(RollbarAssistant.CaptureState(typeof(StaticType))));

                    // report the captured states along with the caught exception:
                    //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromMilliseconds(10000)).Error(ex, state);
                    RollbarLocator.RollbarInstance.Error(ex, state);
                }
            }
        }

        abstract class InstanceTypeBase
        {
            // 1
            private const int BaseConstant = 10;

            // 2
            private int _baseIntField = 3;

            // 3
            public object BaseNullProperty
            {
                get { return this._baseNullField; }
            }
            private object _baseNullField = null;

            // 4
            public string BaseAutoProperty { get; set; } = "BaseAutoProperty value";
        }

        #endregion data mocks

    }
}
