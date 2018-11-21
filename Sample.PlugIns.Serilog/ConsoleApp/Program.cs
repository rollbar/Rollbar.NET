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
            const string rollbarAccessToken = "17965fa5041749b6bf7095a190001ded";
            const string rollbarEnvironment = "RollbarNetSamples";
            var rollbarConfig = new RollbarConfig(rollbarAccessToken)
            {
                Environment = rollbarEnvironment,
            };

            var log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.RollbarSink(rollbarConfig, TimeSpan.FromSeconds(3))
                .CreateLogger();

            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;

            log.Information("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            //let's simulate some exception logging:
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
