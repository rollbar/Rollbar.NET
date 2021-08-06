namespace Sample.WpfApp
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Navigation;
    using System.Windows.Threading;
    using Rollbar;
    using Rollbar.DTOs;

    using Samples;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Setup the Rollbar:
            ConfigureRollbar();

            // Let's log this:
            RollbarLocator.RollbarInstance.Info("WpfApp sample: Rollbar.NET is ready to roll...");

            // Add the event handler for the application unhandled exceptions event:
            base.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Add the event handler for handling non-UI thread exceptions to the event:
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.OnStartup(e);
        }

        protected override void OnLoadCompleted(NavigationEventArgs e)
        {
            base.OnLoadCompleted(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ReportAsCriticalToRollbar(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ReportAsCriticalToRollbar(e.ExceptionObject);
        }

        #region Rollbar integration

        /// <summary>
        /// Reports a data object to Rollbar as critical .
        /// </summary>
        /// <param name="data">The data.</param>
        private void ReportAsCriticalToRollbar(object data)
        {
            TimeSpan rollbarDeliveryTimeout = TimeSpan.FromSeconds(3);
            RollbarLocator.RollbarInstance.AsBlockingLogger(rollbarDeliveryTimeout).Critical(data);
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
        
        #endregion Rollbar integration
    }
}
