namespace Sample.Blazor.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpsPolicy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Rollbar;
    using Rollbar.NetCore.AspNet;

    using Sample.Blazor.Server.Data;

    using Samples;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration
        {
            get;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // STEP.1 - enable Http Context Accessor:
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // STEP.2 - Setup Rollbar Infrastructure:
            ConfigureRollbarInfrastructure();

            // STEP.3 - Add Rollbar logger with properly configured log level filter:
            services.AddRollbarLogger(loggerOptions =>
            {
                loggerOptions.Filter = (loggerName, loglevel) => loglevel >= LogLevel.Trace;
            });

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRollbarMiddleware();
            // Any other middleware component intended to be "monitored" by Rollbar middleware
            // go below this line:

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        // STEP.2 - Setup Rollbar Infrastructure:
        private void ConfigureRollbarInfrastructure()
        {
            RollbarInfrastructureConfig config = new RollbarInfrastructureConfig(
                RollbarSamplesSettings.AccessToken,
                RollbarSamplesSettings.Environment
                );
            RollbarDataSecurityOptions dataSecurityOptions = new RollbarDataSecurityOptions();
            dataSecurityOptions.ScrubFields = new string[]
            {
                "url",
                "method",
            };
            config.RollbarLoggerConfig.RollbarDataSecurityOptions.Reconfigure(dataSecurityOptions);

            RollbarInfrastructure.Instance.Init(config);

            // Optionally:
            RollbarInfrastructure.Instance.QueueController.InternalEvent += OnRollbarInternalEvent;
        }

        private static void OnRollbarInternalEvent(object sender, RollbarEventArgs e)
        {
            Console.WriteLine(e.TraceAsString());

            RollbarApiErrorEventArgs apiErrorEvent = e as RollbarApiErrorEventArgs;
            if(apiErrorEvent != null)
            {
                //TODO: handle/report Rollbar API communication error event...
                return;
            }
            CommunicationEventArgs commEvent = e as CommunicationEventArgs;
            if(commEvent != null)
            {
                //TODO: handle/report Rollbar API communication event...
                return;
            }
            CommunicationErrorEventArgs commErrorEvent = e as CommunicationErrorEventArgs;
            if(commErrorEvent != null)
            {
                //TODO: handle/report basic communication error while attempting to reach Rollbar API service... 
                return;
            }
            InternalErrorEventArgs internalErrorEvent = e as InternalErrorEventArgs;
            if(internalErrorEvent != null)
            {
                //TODO: handle/report basic internal error while using the Rollbar Notifier... 
                return;
            }

        }
    }
}
