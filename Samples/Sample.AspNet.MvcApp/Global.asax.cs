namespace Sample.AspNet.MvcApp
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    using Rollbar;
    using Rollbar.Net.AspNet;

    using Samples;

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ConfigureRollbarSingleton();
            RollbarLocator.RollbarInstance.Info("Rollbar.NET is ready to roll!");


            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            var httpContext = HttpContext.Current;
            IRollbarPackage packagingStrategy = new ExceptionPackage(exception, "EXCEPTION intercepted by MvcApplication.Application_Error()");
            if (httpContext != null)
            {
                packagingStrategy = new HttpContextPackageDecorator(packagingStrategy, httpContext);
            }
            RollbarLocator.RollbarInstance.Critical(packagingStrategy);
        }


        /// <summary>
        /// Configures the Rollbar singleton-like notifier.
        /// </summary>
        private static void ConfigureRollbarSingleton()
        {
            // minimally required Rollbar configuration:
            RollbarInfrastructureConfig rollbarInfrastructureConfig = new RollbarInfrastructureConfig(
                RollbarSamplesSettings.AccessToken, 
                RollbarSamplesSettings.Environment
                );
            // optionally, add data scrubbing options:
            RollbarDataSecurityOptions dataSecurityOptions = new RollbarDataSecurityOptions();
            dataSecurityOptions.ScrubFields = new string[]
            {
                "access_token", // normally, you do not want scrub this specific field (it is operationally critical), but it just proves safety net built into the notifier... 
                "username",
            };
            rollbarInfrastructureConfig.RollbarLoggerConfig.RollbarDataSecurityOptions.Reconfigure(dataSecurityOptions);

            // initialize Rollbar infrastructure:
            RollbarInfrastructure.Instance.Init(rollbarInfrastructureConfig);

            // optionally, if you would like to monitor all Rollbar instances' internal events within your application:
            RollbarInfrastructure.Instance.QueueController.InternalEvent += OnRollbarInternalEvent;

            // optionally, if you would like to monitor this Rollbar instance's internal events within your application:
            RollbarLocator.RollbarInstance.InternalEvent += OnRollbarInternalEvent;

            // basic test:
            RollbarLocator.RollbarInstance.Info($"{typeof(MvcApplication).Namespace}: Rollbar is up and running!");
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
