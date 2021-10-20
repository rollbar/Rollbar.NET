namespace Rollbar.NetCore.AspNet
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Rollbar.DTOs;
    using Rollbar.AppSettings.Json;
    using System;
    using System.Threading.Tasks;
    using Rollbar.NetPlatformExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;

    /// <summary>
    /// Implements an Asp.Net Core middleware component
    /// that invokes the Rollbar.NET component as a catcher all
    /// possible uncaught exceptions while executing/invoking
    /// other/inner middleware components.
    /// </summary>
    /// <example>
    /// Usage example:
    /// To pre-configure Rollbar middleware, implement Startup.ConfigureServices(...) of your 
    /// Asp.Net Core application as
    /// 
    /// <code>
    /// 
    /// // This method gets called by the runtime. Use this method to add services to the container.
    /// public void ConfigureServices(IServiceCollection services)
    /// {
    ///   // Pre-configure Rollbar (preferably at the very top of this function implementation):
    ///   ConfigureRollbar(services);
    ///   
    ///   // ....
    /// }
    /// 
    /// private void ConfigureRollbar(IServiceCollection services)
    /// {
    ///   RollbarInfrastructureConfig config = new RollbarInfrastructureConfig(
    ///     RollbarSamplesSettings.AccessToken,
    ///     RollbarSamplesSettings.Environment
    ///     );
    ///   config.RollbarLoggerConfig.RollbarDeveloperOptions.LogLevel = ErrorLevel.Debug;
    ///   config.RollbarInfrastructureOptions.MaxItems = 500;
    ///   //RollbarDataSecurityOptions dataSecurityOptions = new RollbarDataSecurityOptions();
    ///   //dataSecurityOptions.ScrubFields = new string[]
    ///   //{
    ///   //    "url",
    ///   //    "method",
    ///   //};
    ///   //config.RollbarLoggerConfig.RollbarDataSecurityOptions.Reconfigure(dataSecurityOptions);
    ///   
    ///   RollbarMiddleware.ConfigureServices(services, LogLevel.Information, config, OnRollbarInternalEvent);
    /// }
    /// 
    /// private static void OnRollbarInternalEvent(object sender, RollbarEventArgs e)
    /// {
    ///     Console.WriteLine(e.TraceAsString());
    ///     
    ///     RollbarApiErrorEventArgs apiErrorEvent = e as RollbarApiErrorEventArgs;
    ///     if(apiErrorEvent != null)
    ///     {
    ///       // handle/report Rollbar API communication error event...
    ///       return;
    ///     }
    ///     
    ///     CommunicationEventArgs commEvent = e as CommunicationEventArgs;
    ///     if(commEvent != null)
    ///     {
    ///         // handle/report Rollbar API communication event...
    ///         return;
    ///     }
    ///     CommunicationErrorEventArgs commErrorEvent = e as CommunicationErrorEventArgs;
    ///     if(commErrorEvent != null)
    ///     {
    ///         // handle/report basic communication error while attempting to reach Rollbar API service... 
    ///         return;
    ///     }
    ///     InternalErrorEventArgs internalErrorEvent = e as InternalErrorEventArgs;
    ///     if(internalErrorEvent != null)
    ///     {
    ///         // handle/report basic internal error while using the Rollbar Notifier... 
    ///         return;
    ///     }
    /// }
    /// 
    /// </code>
    /// 
    /// To register this middleware component, implement Startup.Configure(...) of your 
    /// Asp.Net Core application as
    /// 
    /// <code>
    ///
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
        private readonly RequestDelegate _nextRequestProcessor;

        private readonly ILogger _logger;

        private readonly RollbarOptions _rollbarOptions;

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
            RollbarInfrastructure.Instance?.TelemetryCollector?.StartAutocollection();
            RollbarConfigurationUtil.DeduceRollbarConfig(configuration);
        }

        /// <summary>
        /// Invokes this middleware instance on the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A middleware invocation/execution task.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                return;
            }
            // as we learned from a field issue, apparently a middleware can even be invoked without a valid HttpContext:
            string? requestId = context.Features?.Get<IHttpRequestIdentifierFeature>()?.TraceIdentifier;

            context.Request.EnableBuffering();  // so that we can rewind the body stream once we are done

            using(_logger.BeginScope($"Request: {requestId ?? string.Empty}"))
            {
                NetworkTelemetry? networkTelemetry = null;

                try
                {
                    networkTelemetry = CreateTelemetryEvent(context);
                    await this._nextRequestProcessor(context);
                }
                catch (System.Exception ex)
                {
                    FinalizeTelemetryEvent(networkTelemetry, context);

                    if (!RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.CaptureUncaughtExceptions)
                    {
                        // just rethrow since the Rollbar SDK is configured not to auto-capture 
                        // uncaught exceptions:
                        throw; 
                    }

                    if (RollbarScope.Current != null 
                        && RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.MaxItems > 0
                        )
                    {
                        RollbarScope.Current.IncrementLogItemsCount();
                        if (RollbarScope.Current.LogItemsCount == RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.MaxItems)
                        {
                            // the Rollbar SDK just reached MaxItems limit, report this fact and pause further logging within this scope: 
                            RollbarLocator.RollbarInstance.Warning(RollbarScope.MaxItemsReachedWarning);
                            throw;
                        }
                        else if (RollbarScope.Current.LogItemsCount > RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.MaxItems)
                        {
                            // just rethrow since the Rollbar SDK already exceeded MaxItems limit:
                            throw;
                        }
                        else
                        {
                            CaptureWithRollbar(ex, context);
                        }
                    }

                    if(RollbarLocator.RollbarInstance.Config.RollbarDeveloperOptions.WrapReportedExceptionWithRollbarException)
                    {
                        throw new RollbarMiddlewareException(ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    FinalizeTelemetryEvent(networkTelemetry, context);
                }
            }
        }

        /// <summary>
        /// Captures tha provided data with Rollbar.
        /// </summary>
        /// <param name="exception">
        /// The ex.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        private static void CaptureWithRollbar(System.Exception exception, HttpContext context)
        {
            IRollbarPackage rollbarPackage = new ExceptionPackage(exception, $"{nameof(RollbarMiddleware)} processed uncaught exception.");
            if (context != null)
            {
                if (context.Request != null)
                {
                    rollbarPackage = new HttpRequestPackageDecorator(rollbarPackage, context.Request, true);
                }

                if (context.Response != null)
                {
                    rollbarPackage = new HttpResponsePackageDecorator(rollbarPackage, context.Response, true);
                }
            }

            RollbarLocator.RollbarInstance.Critical(rollbarPackage);
        }

        /// <summary>
        /// Creates the telemetry event.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>System.Nullable&lt;NetworkTelemetry&gt;.</returns>
        private static NetworkTelemetry? CreateTelemetryEvent(HttpContext context)
        {
            if (RollbarInfrastructure.Instance != null
                && RollbarInfrastructure.Instance.TelemetryCollector != null
                && RollbarInfrastructure.Instance.TelemetryCollector.IsAutocollecting
                && context.Request != null
                )
            {
                int? telemetryStatusCode = context.Response?.StatusCode;

                NetworkTelemetry networkTelemetry = new(
                    method: context!.Request.Method,
                    url: context.Request.Host.Value + context.Request.Path,
                    eventStart: DateTime.UtcNow,
                    eventEnd: null,
                    statusCode: telemetryStatusCode
                    );

                RollbarInfrastructure
                    .Instance
                    .TelemetryCollector
                    .Capture(new Telemetry(TelemetrySource.Server, TelemetryLevel.Info, networkTelemetry));

                return networkTelemetry;
            }

            return null;
        }

        /// <summary>
        /// Finalizes the telemetry event.
        /// </summary>
        /// <param name="networkTelemetry">
        /// The network telemetry.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        private static void FinalizeTelemetryEvent(NetworkTelemetry? networkTelemetry, HttpContext context)
        {
            if (networkTelemetry != null)
            {
                if (string.IsNullOrWhiteSpace(networkTelemetry.StatusCode))
                {
                    networkTelemetry.StatusCode = context?.Response?.StatusCode.ToString();
                }
                networkTelemetry.FinalizeEvent();
            }
        }

        /// <summary>
        /// Configures Rollbar-specific services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="rollbarLogLevel">The LogLevel starting from which to log with Rollbar.</param>
        /// <param name="rollbarInfrastructureConfig">The valid Rollbar Infrastructure Configuration.</param>
        /// <param name="rollbarInternalEventHandler">Optional Rollbar Internal Event Handler.</param>
        /// <returns>N/A</returns>
        public static void ConfigureServices(
            IServiceCollection services,
            LogLevel rollbarLogLevel,
            RollbarInfrastructureConfig rollbarInfrastructureConfig,
            EventHandler<RollbarEventArgs>? rollbarInternalEventHandler = null
            )
        {
            if(!services.Any(s => s.ServiceType == typeof(IHttpContextAccessor)))
            {
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            }

            RollbarInfrastructure.Instance.Init(rollbarInfrastructureConfig);

            if (rollbarInternalEventHandler != null 
                && RollbarInfrastructure.Instance.QueueController != null
                )
            {
                RollbarInfrastructure.Instance.QueueController.InternalEvent += rollbarInternalEventHandler;
            }

            services.AddRollbarLogger(loggerOptions =>
            {
                loggerOptions.Filter = (loggerName, logLevel) => logLevel >= rollbarLogLevel;
            });
        }

    }
}
