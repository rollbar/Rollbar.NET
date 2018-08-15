namespace Sample.Net.ConsoleApp
{
    using GameDomainModel;
    using Rollbar;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    class Program
    {
        static void Main(string[] args)
        {
            // NOTE: when the next line is commented out, 
            // the Rollbar notifier will still be properly configured via app.config:
            //ConfigureRollbarSingleton();

            // ConfigureRollbarSingleton() is called above,
            // the next code line could be commented out:
            RollbarLocator.RollbarInstance
                .InternalEvent += OnRollbarInternalEvent
                ;

            RollbarLocator.RollbarInstance
                .Info("ConsoleApp sample: Basic info log example.")
                .Debug("ConsoleApp sample: First debug log.")
                .Error(new NullReferenceException("ConsoleApp sample: null reference exception."))
                .Error(new System.Exception("ConsoleApp sample: trying out the TraceChain", new NullReferenceException()))
                ;

            DemonstrateExceptionSourceStateCapture();

            RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(5))
                .Info("Via no-blocking mechanism.")
                ;


            Console.WriteLine("Press Enter key to exit...");
            Console.ReadLine();
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
            //RollbarQueueController.Instance.InternalEvent += OnRollbarInternalEvent;

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


        private static void DemonstrateExceptionSourceStateCapture()
        {
            Vehicle vehicle = new Sedan(200) {
                Brand = "Audi",
                Model = "A4 Quattro",
                Type = Sedan.SedanType.PassengerCar,
            };

            try
            {
                vehicle.Start();
            }
            catch (System.Exception ex)
            {
                // capture state of vehicle instance:
                var state = RollbarAssistant.CaptureState(vehicle, "StartedVehicle");
                // also, capture state of the Game static type:
                RollbarAssistant.CaptureState(typeof(Game), state);
                // report the captured states along with the caught exception:
                RollbarLocator.RollbarInstance
                    .AsBlockingLogger(TimeSpan.FromMilliseconds(10000))
                    .Error(new ApplicationException("Application exception with a state capture.", ex), state);
            }
        }

    }
}
