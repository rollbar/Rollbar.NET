#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Rollbar.Telemetry;
    using System;
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

        private readonly ILogger _logger = null;
        private readonly RollbarOptions _rollbarOptions = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarMiddleware" /> class.
        /// </summary>
        /// <param name="nextRequestProcessor">The next request processor.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="rollbarOptions">The rollbar options.</param>
        public RollbarMiddleware(
            RequestDelegate nextRequestProcessor
            , IConfiguration configuration
            , ILoggerFactory loggerFactory
            , IOptions<RollbarOptions> rollbarOptions
            )
        {
            this._nextRequestProcessor = nextRequestProcessor;
            this._logger = loggerFactory.CreateLogger<RollbarMiddleware>();
            this._rollbarOptions = rollbarOptions.Value;

            RollbarConfigurationUtil.DeduceRollbarTelemetryConfig(configuration);
            TelemetryCollector.Instance.StartAutocollection();
            RollbarConfigurationUtil.DeduceRollbarConfig(configuration);
        }

        /// <summary>
        /// Invokes this middleware instance on the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A middleware invocation/execution task.</returns>
        public async Task Invoke(HttpContext context)
        {
            // as we learned from a field issue, apparently a middleware can even be invoked without a valid HttPContext:
            string requestId = null;
            requestId = context?.Features?
                .Get<IHttpRequestIdentifierFeature>()?
                .TraceIdentifier;
            using (_logger.BeginScope($"Request: {requestId ?? string.Empty}"))
            {
                try
                {
                    RollbarScope.Current.HttpContext.HttpAttributes = new RollbarHttpAttributes(context);
                    await this._nextRequestProcessor(context);
                }
                catch (Exception ex)
                {
                    if (!RollbarLocator.RollbarInstance.Config.CaptureUncaughtExceptions)
                    {
                        // just rethrow since the Rollbar SDK is configured not to auto-capture uncaught exceptions:
                        throw ex; 
                    }

                    if (RollbarScope.Current != null 
                        && RollbarLocator.RollbarInstance.Config.MaxItems > 0
                        )
                    {
                        RollbarScope.Current.IncrementLogItemsCount();
                        if (RollbarScope.Current.LogItemsCount == RollbarLocator.RollbarInstance.Config.MaxItems)
                        {
                            // the Rollbar SDK just reached MaxItems limit, report this fact and pause further logging within this scope: 
                            RollbarLocator.RollbarInstance.Warning(RollbarScope.MaxItemsReachedWarning);
                            throw ex;
                        }
                        else if (RollbarScope.Current.LogItemsCount > RollbarLocator.RollbarInstance.Config.MaxItems)
                        {
                            // just rethrow since the Rollbar SDK already exceeded MaxItems limit:
                            throw ex;
                        }
                    }
                    else
                    {
                        // let's custom build the Data object that includes the exception 
                        // along with the current HTTP request context:
                        DTOs.Data data = new DTOs.Data(
                            config: RollbarLocator.RollbarInstance.Config,
                            body: new DTOs.Body(ex),
                            custom: null,
                            request: (context != null) ? new DTOs.Request(null, context.Request) : null
                            )
                        {
                            Level = ErrorLevel.Critical,
                        };

                        // log the Data object (the exception + the HTTP request data):
                        RollbarLocator.RollbarInstance.Log(data);
                    }

                    throw new Exception("The included internal exception processed by the Rollbar middleware", ex);
                }
                finally
                {
                    if (context != null && context.Response != null)
                    {
                        RollbarScope.Current.HttpContext.HttpAttributes.StatusCode = context.Response.StatusCode;
                    }
                }
            }
        }
    }
}

#endif
