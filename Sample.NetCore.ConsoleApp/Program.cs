using Rollbar;
using Rollbar.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sample.NetCore.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureRollbarSingleton();

            Dictionary<string, object> customFields = new Dictionary<string, object>();
            customFields.Add("Hebrew", "אספירין");
            customFields.Add("Hindi", "एस्पिरि");
            customFields.Add("Chinese", "阿司匹林");
            customFields.Add("Japanese", "アセチルサリチル酸");
            customFields.Add("path1", "d:\\Work\\\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\\branches\\v2\\...");
            customFields.Add("path2", @"d:\Work\אספירין\branches\v2\...");

            RollbarLocator.RollbarInstance
                .Info("ConsoleApp sample: Basic info log example.", customFields)
                .Debug("ConsoleApp sample: First debug log.")
                .Error(new NullReferenceException("ConsoleApp sample: null reference exception."))
                .Error(new System.Exception("ConsoleApp sample: trying out the TraceChain", new NullReferenceException()))
                ;

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
                // optional step if you would like to monitor Rollbar internal events within your application:
                .InternalEvent += OnRollbarInternalEvent
                ;

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
    }
}
