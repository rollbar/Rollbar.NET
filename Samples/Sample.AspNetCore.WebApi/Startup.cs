namespace Sample.AspNetCore.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpsPolicy;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;

    using Rollbar;
    using Rollbar.NetCore.AspNet;

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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            ConfigureRollbarSingleton();

            services.AddRollbarLogger(loggerOptions =>
            {
                loggerOptions.Filter = (loggerName, loglevel) => loglevel >= LogLevel.Information;
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sample.AspNetCore.WebApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample.AspNetCore.WebApi v1"));
            }

            app.UseRollbarMiddleware();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// Configures the Rollbar singleton-like notifier.
        /// </summary>
        private void ConfigureRollbarSingleton()
        {
            RollbarInfrastructureConfig config = new RollbarInfrastructureConfig(
                RollbarSamplesSettings.AccessToken,
                RollbarSamplesSettings.Environment
                );
            config.RollbarLoggerConfig.RollbarDeveloperOptions.LogLevel = ErrorLevel.Debug;
            config.RollbarInfrastructureOptions.MaxItems = 500;
            //RollbarDataSecurityOptions dataSecurityOptions = new RollbarDataSecurityOptions();
            //dataSecurityOptions.ScrubFields = new string[]
            //{
            //    "url",
            //    "method",
            //};
            //config.RollbarLoggerConfig.RollbarDataSecurityOptions.Reconfigure(dataSecurityOptions);

            RollbarInfrastructure.Instance.Init(config);
            RollbarInfrastructure.Instance.QueueController.InternalEvent += OnRollbarInternalEvent;
        }

        /// <summary>
        /// Called when Rollbar internal event detected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
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
