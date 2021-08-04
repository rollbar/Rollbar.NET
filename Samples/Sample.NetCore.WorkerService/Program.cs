using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Rollbar;
using Rollbar.AppSettings.Json;
using Rollbar.NetPlatformExtensions;

namespace Sample.NetCore.WorkerService
{
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
