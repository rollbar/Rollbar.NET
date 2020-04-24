using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                    logging.ClearProviders();
                    logging.AddProvider(new RollbarLoggerProvider());
                    })
                .ConfigureServices((hostContext,services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}
