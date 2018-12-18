using Rollbar;
using Rollbar.DTOs;
using Rollbar.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sample.NetCore.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureRollbarSingleton();
            ConfigureRollbarTelemetry();

            TelemetryCollector.Instance.Capture(
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

            RollbarLocator.RollbarInstance
                .Info("ConsoleApp sample: Basic info log example.", customFields);

            TelemetryCollector.Instance.Capture(
                new Telemetry(
                    TelemetrySource.Client,
                    TelemetryLevel.Info,
                    new LogTelemetry("Something interesting happened.")
                    )
                );
            RollbarLocator.RollbarInstance
                .Debug("ConsoleApp sample: First debug log.");

            TelemetryCollector.Instance.Capture(
                new Telemetry(
                    TelemetrySource.Client,
                    TelemetryLevel.Error,
                    new ErrorTelemetry(new System.Exception("Worth mentioning!"))
                    )
                );
            RollbarLocator.RollbarInstance
                .Error(new NullReferenceException("ConsoleApp sample: null reference exception."));

            TelemetryCollector.Instance.Capture(
                new Telemetry(
                    TelemetrySource.Client,
                    TelemetryLevel.Error,
                    new ManualTelemetry(new Dictionary<string, object>() { { "somthing", "happened" }, })
                    )
                );
            RollbarLocator.RollbarInstance
                .Error(new System.Exception("ConsoleApp sample: trying out the TraceChain", new NullReferenceException()));


            TelemetryCollector.Instance.Capture(
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
            catch (System.TimeoutException ex)
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
            catch (System.TimeoutException ex)
            {
                msg = "*** 3. Blocking call with too short timeout. Exception: " + Environment.NewLine + ex;
                System.Diagnostics.Trace.WriteLine(msg);
                Console.WriteLine(msg);
            }
            stopwatch.Stop();
            msg = "*** Blocking (short timeout) report took " + stopwatch.Elapsed.TotalMilliseconds + " [msec].";
            System.Diagnostics.Trace.WriteLine(msg);
            Console.WriteLine(msg);
        }

        /// <summary>
        /// Configures the rollbar telemetry.
        /// </summary>
        private static void ConfigureRollbarTelemetry()
        {
            TelemetryConfig telemetryConfig = new TelemetryConfig(
                telemetryEnabled: true,
                telemetryQueueDepth: 3
                );
            TelemetryCollector.Instance.Config.Reconfigure(telemetryConfig);
        }

        /// <summary>
        /// Configures the Rollbar singleton-like notifier.
        /// </summary>
        private static void ConfigureRollbarSingleton()
        {
            const string rollbarAccessToken = "17965fa5041749b6bf7095a190001ded";
            const string rollbarEnvironment = "RollbarNetSamples";

            var config = new RollbarConfig(rollbarAccessToken) // minimally required Rollbar configuration
            {
                Environment = rollbarEnvironment,
                ScrubFields = new string[]
                {
                    "access_token", // normally, you do not want scrub this specific field (it is operationally critical), but it just proves safety net built into the notifier... 
                    "username",
                }
            };
            RollbarLocator.RollbarInstance
                // minimally required Rollbar configuration:
                .Configure(config)
                // optional step if you would like to monitor this Rollbar instance's internal events within your application:
                .InternalEvent += OnRollbarInternalEvent
                ;

            // optional step if you would like to monitor all Rollbar instances' internal events within your application:
            RollbarQueueController.Instance.InternalEvent += OnRollbarInternalEvent;

            // Optional info about reporting Rollbar user:
            SetRollbarReportingUser("007", "jbond@mi6.uk", "JBOND");
        }

        /// <summary>
        /// Sets the rollbar reporting user.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="email">The email.</param>
        /// <param name="userName">Name of the user.</param>
        private static void SetRollbarReportingUser(string id, string email, string userName)
        {
            Person person = new Person(id);
            person.Email = email;
            person.UserName = userName;
            RollbarLocator.RollbarInstance.Config.Person = person;
        }

        /// <summary>
        /// Called when rollbar internal event is detected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        private static void OnRollbarInternalEvent(object sender, RollbarEventArgs e)
        {
            Console.WriteLine(e.TraceAsString());

            RollbarApiErrorEventArgs apiErrorEvent = e as RollbarApiErrorEventArgs;
            if (apiErrorEvent != null)
            {
                //TODO: handle/report Rollbar API communication error event...
                return;
            }
            CommunicationEventArgs commEvent = e as CommunicationEventArgs;
            if (commEvent != null)
            {
                //TODO: handle/report Rollbar API communication event...
                return;
            }
            CommunicationErrorEventArgs commErrorEvent = e as CommunicationErrorEventArgs;
            if (commErrorEvent != null)
            {
                //TODO: handle/report basic communication error while attempting to reach Rollbar API service... 
                return;
            }
            InternalErrorEventArgs internalErrorEvent = e as InternalErrorEventArgs;
            if (internalErrorEvent != null)
            {
                //TODO: handle/report basic internal error while using the Rollbar Notifier... 
                return;
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
                    RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromMilliseconds(10000)).Error(ex, state);
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
