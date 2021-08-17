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

            // TODO: implement as needed:

            //context.BeginRequest += Context_BeginRequest;
            //context.EndRequest += Context_EndRequest;

            //context.AcquireRequestState += Context_AcquireRequestState;
            //context.AuthenticateRequest += Context_AuthenticateRequest;
            //context.AuthorizeRequest += Context_AuthorizeRequest;
            //context.Disposed += Context_Disposed;
            //context.LogRequest += Context_LogRequest;
            //context.MapRequestHandler += Context_MapRequestHandler;
            //context.ReleaseRequestState += Context_ReleaseRequestState;
            //context.RequestCompleted += Context_RequestCompleted;
            //context.ResolveRequestCache += Context_ResolveRequestCache;
            //context.UpdateRequestCache += Context_UpdateRequestCache;

            //context.PostAcquireRequestState += Context_PostAcquireRequestState;
            //context.PostAuthenticateRequest += Context_PostAuthenticateRequest;
            //context.PostAuthorizeRequest += Context_PostAuthorizeRequest;
            //context.PostLogRequest += Context_PostLogRequest;
            //context.PostMapRequestHandler += Context_PostMapRequestHandler;
            //context.PostReleaseRequestState += Context_PostReleaseRequestState;
            //context.PostRequestHandlerExecute += Context_PostRequestHandlerExecute;
            //context.PostResolveRequestCache += Context_PostResolveRequestCache;
            //context.PostUpdateRequestCache += Context_PostUpdateRequestCache;

            //context.PreRequestHandlerExecute += Context_PreRequestHandlerExecute;
            //context.PreSendRequestContent += Context_PreSendRequestContent;
            //context.PreSendRequestHeaders += Context_PreSendRequestHeaders;

        }

        private void Context_Error(object sender, EventArgs e)
        {
            HttpApplication? httpApplication = sender as HttpApplication;
            if (httpApplication == null 
                || httpApplication.Context == null 
                || httpApplication.Context.AllErrors == null 
                || httpApplication.Context.AllErrors.Length == 0
                )
            {
                return;
            }

            // potential objects to snap as a Rollbar payload:
            //************************************************

            //httpApplication.Context.AllErrors;
            //httpApplication.Context;
            //httpApplication.Request;
            //httpApplication.Response;
            //httpApplication.Session;
            //httpApplication.User;
            //httpApplication.Server;
            //httpApplication.Application;
            //httpApplication.Modules;

            AggregateException exception = 
                new AggregateException("RollbarHttpModule's context error(s)", httpApplication.Context.AllErrors);

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

        private void Context_BeginRequest(object sender, EventArgs e)
        {
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
        }

        private void Context_UpdateRequestCache(object sender, EventArgs e)
        {
        }

        private void Context_ResolveRequestCache(object sender, EventArgs e)
        {
        }

        private void Context_RequestCompleted(object sender, EventArgs e)
        {
        }

        private void Context_ReleaseRequestState(object sender, EventArgs e)
        {
        }

        private void Context_PreSendRequestHeaders(object sender, EventArgs e)
        {
        }

        private void Context_PreSendRequestContent(object sender, EventArgs e)
        {
        }

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
        }

        private void Context_PostUpdateRequestCache(object sender, EventArgs e)
        {
        }

        private void Context_PostResolveRequestCache(object sender, EventArgs e)
        {
        }

        private void Context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
        }

        private void Context_PostReleaseRequestState(object sender, EventArgs e)
        {
        }

        private void Context_PostMapRequestHandler(object sender, EventArgs e)
        {
        }

        private void Context_PostLogRequest(object sender, EventArgs e)
        {
        }

        private void Context_PostAuthorizeRequest(object sender, EventArgs e)
        {
        }

        private void Context_PostAuthenticateRequest(object sender, EventArgs e)
        {
        }

        private void Context_PostAcquireRequestState(object sender, EventArgs e)
        {
        }

        private void Context_MapRequestHandler(object sender, EventArgs e)
        {
        }

        private void Context_LogRequest(object sender, EventArgs e)
        {
        }

        private void Context_Disposed(object sender, EventArgs e)
        {
        }

        private void Context_AuthorizeRequest(object sender, EventArgs e)
        {
        }

        private void Context_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        private void Context_AcquireRequestState(object sender, EventArgs e)
        {
        }
    }
}
