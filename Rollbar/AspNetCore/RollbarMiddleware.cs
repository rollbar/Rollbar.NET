#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements an Asp.Net Core middleware component
    /// that invokes the Rollbar.NET component as a catcher all
    /// possible uncaught exceptions while executing/invoking
    /// other/inner middleware components.
    /// </summary>
    /// <example>
    /// Usage example:
    /// To register this middleware component, implement Startup.Configure(...) of your 
    /// Asp.Net Core application as
    /// 
    /// <code>
    /// // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    /// {
    ///     if (env.IsDevelopment())
    ///     {
    ///         app.UseDeveloperExceptionPage();
    ///     }
    /// 
    ///     app.UseRollbarMiddleware();
    ///     
    ///     // All the middleware components intended to be "monitored"
    ///     // by the Rollbar middleware to be added below this line:
    ///     app.UseAnyOtherInnerMiddleware();
    /// 
    ///     app.UseMvc();
    /// }
    /// 
    /// </code>
    /// 
    /// </example>
    public class RollbarMiddleware
    {
        /// <summary>
        /// The next request processor within the middleware pipeline
        /// </summary>
        private readonly RequestDelegate _nextRequestProcessor = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarMiddleware" /> class.
        /// </summary>
        /// <param name="nextRequestProcessor">The next request processor.</param>
        /// <param name="configuration">The configuration.</param>
        public RollbarMiddleware(RequestDelegate nextRequestProcessor, IConfiguration configuration)
        {
            this._nextRequestProcessor = nextRequestProcessor;

            if (RollbarLocator.RollbarInstance.Config.AccessToken == null)
            {
                // Here we assume that the Rollbar singleton was not explicitly preconfigured 
                // anywhere in the code (Program.cs or Startup.cs), 
                // so we are trying to configure it from IConfiguration:

                const string defaultAccessToken = "none";
                RollbarConfig rollbarConfig = new RollbarConfig(defaultAccessToken);
                configuration.GetSection("Rollbar").Bind(rollbarConfig);

                if (rollbarConfig.AccessToken == defaultAccessToken)
                {
                    const string error = "Rollbar.NET notifier is not configured properly.";
                    throw new Exception(error);
                }

                RollbarLocator.RollbarInstance
                    // minimally required Rollbar configuration:
                    .Configure(rollbarConfig);

            }
        }

        /// <summary>
        /// Invokes this middleware instance on the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A middleware invocation/execution task.</returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await this._nextRequestProcessor(context);
            }
            catch(Exception ex)
            {
                // let's custom build the Data object that includes the exception 
                // along with the current HTTP request context:
                Rollbar.DTOs.Data data = new Rollbar.DTOs.Data(
                    config:     RollbarLocator.RollbarInstance.Config,
                    body:       new Rollbar.DTOs.Body(ex),
                    custom:     null,
                    request:    new Rollbar.DTOs.Request(null, context.Request)
                    )
                {
                    Level = ErrorLevel.Critical,
                };

                // log the Data object (the exception + the HTTP request data):
                Rollbar.RollbarLocator.RollbarInstance.Log(data);

                throw new Exception("The included internal exception processed by the Rollbar middleware", ex);
            }
        }
    }
}

#endif
