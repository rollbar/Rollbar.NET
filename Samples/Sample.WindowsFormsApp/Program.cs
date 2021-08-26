namespace Sample.WindowsFormsApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Rollbar;
    using Rollbar.DTOs;

    using Samples;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Setup the Rollbar Notifier:
            ConfigureRollbar();

            // Let's log this:
            RollbarLocator.RollbarInstance.Info("WindowsFormsApp sample: Rollbar.NET is ready to roll...");

            // Add the event handler for handling UI thread exceptions to the event:
            Application.ThreadException += Application_ThreadException;

            // Set the unhandled exception mode to force all Windows Forms errors to go through
            // our handler:
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event:
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(3)).Critical(e.ExceptionObject);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(3)).Critical(e.Exception);
        }

        /// <summary>
        /// Configures the Rollbar singleton-like notifier.
        /// </summary>
        private static void ConfigureRollbar()
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
                Email = "jbond@mi6.uk", 
                UserName = "JBOND" 
            };
            rollbarInfrastructureConfig.RollbarLoggerConfig.RollbarPayloadAdditionOptions.Reconfigure(payloadAdditionOptions);

            RollbarInfrastructure.Instance.Init(rollbarInfrastructureConfig);

            // optionally, if you would like to monitor this Rollbar instance's internal events within your application:
            RollbarInfrastructure.Instance.QueueController.InternalEvent += OnRollbarInternalEvent;
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
