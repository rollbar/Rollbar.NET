namespace Rollbar.Net.AspNet
{
    using System;
    using System.Web;

    /// <summary>
    /// Class RollbarHttpModule.
    /// Implements the <see cref="System.Web.IHttpModule" /></summary>
    /// <seealso cref="System.Web.IHttpModule" />
    /// <remarks>
    /// This HttpModule relies on proper Rollbar configuration specified within the Web.config file.
    /// For example:
    /// <code>
    ///     <configSections>
    ///         <section name = "rollbar" type="Rollbar.NetFramework.RollbarConfigSection, Rollbar"/>
    ///         <section name = "rollbarTelemetry" type="Rollbar.NetFramework.RollbarTelemetryConfigSection, Rollbar"/>
    ///       </configSections>
    ///       
    ///     <rollbar
    ///       accessToken = "17965fa5041749b6bf7095a190001ded"
    ///       environment="unit-tests"
    ///       enabled="true"
    ///       scrubFields="ThePassword, Secret"
    ///       scrubSafelistFields="ThePassword"
    ///       logLevel="Info"
    ///       maxReportsPerMinute="160"
    ///       reportingQueueDepth="120"
    ///       personDataCollectionPolicies="Username, Email"
    ///       ipAddressCollectionPolicy="CollectAnonymized"
    ///       />
    ///     
    ///     <rollbarTelemetry
    ///       telemetryEnabled = "true"
    ///       telemetryQueueDepth="100"
    ///       telemetryAutoCollectionTypes="Network, Log, Error"
    ///       telemetryAutoCollectionInterval="00:00:00.3000000"
    ///       />
    /// </code>
    /// </remarks>
    public class RollbarHttpModule
        : IHttpModule
    {
        private readonly IRollbar _rollbar = RollbarFactory.CreateNew();
        // NOTE: a valid Rollbar configuration must be specified within Web.config...

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule" />.
        /// </summary>
        public void Dispose()
        {
            this._rollbar.Dispose();
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication" />
        /// that provides access to the methods, properties, and events common to all application objects within an ASP.NET application
        /// </param>
        public void Init(HttpApplication context)
        {
            context.Error += Context_Error;
        }

        private void Context_Error(object sender, EventArgs e)
        {
            if (sender is not HttpApplication httpApplication
                || httpApplication.Context == null
                || httpApplication.Context.AllErrors == null
                || httpApplication.Context.AllErrors.Length == 0
                )
            {
                return;
            }

            AggregateException exception = 
                new("RollbarHttpModule's context error(s)", httpApplication.Context.AllErrors);

            IRollbarPackage package = 
                new ExceptionPackage(exception, "HttpApplication.Context.AllErrors", true);
            
            // The HttpContext decorator already takes care of the HttpRequest and HttpResponse internally:
            package = 
                new HttpContextPackageDecorator(package, httpApplication.Context);

            if (httpApplication.User?.Identity != null && !string.IsNullOrWhiteSpace(httpApplication.User.Identity.Name))
            {
                package = new PersonPackageDecorator(package, httpApplication.User.Identity.Name, httpApplication.User.Identity.Name, null);
            }

            this._rollbar.Error(package);
        }
    }
}
