namespace ConsoleApp
{
    using System;
    using Serilog;
    using Rollbar;
    using Rollbar.PlugIns.Serilog;

    public class Program
    {
        static void Main(string[] args)
        {
            // Define RollbarConfig:
            const string rollbarAccessToken = "17965fa5041749b6bf7095a190001ded";
            const string rollbarEnvironment = "RollbarNetSamples";
            RollbarConfig rollbarConfig = new RollbarConfig(rollbarAccessToken)
            {
                Environment = rollbarEnvironment,
            };

            // Add RollbarSink to the Serilog Logger using pre-configured RollbarConfig:
            Serilog.Core.Logger log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.RollbarSink(rollbarConfig, TimeSpan.FromSeconds(3))
                .CreateLogger();

            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;

            // an informational trace via Serilog:
            log.Information("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            // let's simulate some exception logging via Serilog:
            try
            {
                throw new ApplicationException("Oy vey!");
            }
            catch(Exception ex)
            {
                log.Error(ex, "What happened?");
            }
        }
    }
}
