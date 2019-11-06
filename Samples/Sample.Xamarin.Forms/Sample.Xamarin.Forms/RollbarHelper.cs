namespace Sample.Xamarin.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar;
    using Rollbar.DTOs;

    /// <summary>
    /// Class RollbarHelper.
    /// A utility class aiding in Rollbar SDK usage.
    /// </summary>
    public static class RollbarHelper
    {
        public static readonly TimeSpan RollbarTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Registers for global exception handling.
        /// </summary>
        public static void RegisterForGlobalExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var newExc = new System.Exception("CurrentDomainOnUnhandledException", args.ExceptionObject as System.Exception);
                RollbarLocator.RollbarInstance.AsBlockingLogger(RollbarTimeout).Critical(newExc);
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                var newExc = new ApplicationException("TaskSchedulerOnUnobservedTaskException", args.Exception);
                RollbarLocator.RollbarInstance.AsBlockingLogger(RollbarTimeout).Critical(newExc);
            };
        }
        /// <summary>
        /// Configures the Rollbar singleton-like notifier.
        /// </summary>
        public static void ConfigureRollbarSingleton()
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
                },
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
        /// <param name="e">The <see cref="RollbarEventArgs" /> instance containing the event data.</param>
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
