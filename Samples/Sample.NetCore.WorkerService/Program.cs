namespace Sample.NetCore.WorkerService
{
    using System;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Rollbar;
    using Rollbar.AppSettings.Json;
    using Rollbar.NetPlatformExtensions;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((logging) => {

                    // seed Rollbar infrastructure configuration:
                    RollbarInfrastructureConfig rollbarInfrastructureConfig = new RollbarInfrastructureConfig();
                    
                    // reconfigure the seed from the appsettings.json file:
                    if(!AppSettingsUtility.LoadAppSettings(rollbarInfrastructureConfig))
                    {
                        throw new ApplicationException("Couldn't load Rollbar configuration!");
                    }
                    
                    // init the Rollbar infrastructure with the loaded configuration:
                    RollbarInfrastructure.Instance.Init(rollbarInfrastructureConfig);

                    // add a well-configured RollbarLoggerProvider:
                    logging.ClearProviders();
                    logging.AddProvider(new RollbarLoggerProvider(rollbarInfrastructureConfig.RollbarLoggerConfig));

                    }
                )
                .ConfigureServices((hostContext,services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}
