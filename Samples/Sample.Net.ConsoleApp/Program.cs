namespace Sample.Net.ConsoleApp
{
    using GameDomainModel;
    using Rollbar;
    using Rollbar.DTOs;

    using Samples;

    using System;

    class Program
    {
        static void Main(string[] args)
        {
            ConfigureRollbarSingleton();

            // ConfigureRollbarSingleton() is called above,
            // the next code line could be commented out:
            RollbarLocator.RollbarInstance
                .InternalEvent += OnRollbarInternalEvent;

            RollbarLocator.RollbarInstance
                .Info("ConsoleApp sample: Basic info log example.");
            RollbarLocator.RollbarInstance
                .Debug("ConsoleApp sample: First debug log.");
            RollbarLocator.RollbarInstance
                .Error(new NullReferenceException("ConsoleApp sample: null reference exception."));
            RollbarLocator.RollbarInstance
                .Error(new System.Exception("ConsoleApp sample: trying out the TraceChain", new NullReferenceException()));

            DemonstrateExceptionSourceStateCapture();

            RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(5))
                .Info("Via no-blocking mechanism.");


            Console.WriteLine("Press Enter key to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// Configures the Rollbar singleton-like notifier.
        /// </summary>
        private static void ConfigureRollbarSingleton()
        {
            RollbarInfrastructureConfig rollbarInfrastructureConfig = new RollbarInfrastructureConfig(
                RollbarSamplesSettings.AccessToken,
                RollbarSamplesSettings.Environment
                );

            RollbarDataSecurityOptions dataSecurityOptions = new RollbarDataSecurityOptions();
            dataSecurityOptions.ScrubFields = new string[]
            {
                "access_token", // normally, you do not want scrub this specific field (it is operationally critical), but it just proves safety net built into the notifier... 
                "username",
            };
            rollbarInfrastructureConfig.RollbarLoggerConfig.RollbarDataSecurityOptions.Reconfigure(dataSecurityOptions);

            RollbarPayloadAdditionOptions payloadAdditionOptions = new RollbarPayloadAdditionOptions();
            payloadAdditionOptions.Person = new Person("007")
            {
                UserName = "JBOND",
                Email = "jbond@mi6.uk"
            };
            rollbarInfrastructureConfig.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Reconfigure(payloadAdditionOptions);

            RollbarInfrastructure.Instance.Init(rollbarInfrastructureConfig);

            // optionally, if you would like to monitor all Rollbar instances' internal events within your application:
            RollbarInfrastructure.Instance.QueueController.InternalEvent += OnRollbarInternalEvent;

            // optionally, if you would like to monitor this Rollbar instance's internal events within your application:
            //RollbarLocator.RollbarInstance.InternalEvent += OnRollbarInternalEvent;
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
