namespace ConsoleApp
{
    using System;
    using Serilog;

    public class Program
    {
        static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                 .MinimumLevel.Information()
                 .WriteTo.Console()
                 .CreateLogger();

            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;

            log.Information("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
        }
    }
}
