namespace ConsoleApp
{
    using System;
    using Serilog;
    using Rollbar;
    using Rollbar.PlugIns.Serilog;
    using Samples;

    public class Program
    {
        static void Main(string[] args)
        {
            // Define RollbarInfrastructureConfig:
            RollbarInfrastructureConfig rollbarInfrastructureConfig = 
                new RollbarInfrastructureConfig(
                    RollbarSamplesSettings.AccessToken, 
                    RollbarSamplesSettings.Environment
                    );

            // Add RollbarSink to the Serilog Logger using pre-configured RollbarConfig:
            Serilog.Core.Logger log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.RollbarSink(rollbarInfrastructureConfig, TimeSpan.FromSeconds(3))
                .CreateLogger();

            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;

            // an informational trace via Serilog:
            log.Information("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            // let's simulate some exception logging via Serilog:
            try
            {
                throw new ApplicationException("Oy vey via Serilog!");
            }
            catch(Exception ex)
            {
                log.Error(ex, "What happened?");
            }
        }
    }
}
