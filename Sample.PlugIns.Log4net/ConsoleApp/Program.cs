// Load a custom log4net config file: 
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace ConsoleApp
{
    using System;
    using log4net;

    class Program
    {
        // Initialize a log4net's ILog instance:
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            // an example of an informational log via log4net:
            log.Info("Hello world from Log4net!");

            try
            {
                throw new ApplicationException("Oy vey via Log4net!");
            }
            catch(Exception ex)
            {
                // an example of an error log via log4net:
                log.Error("Just FYI...", ex);
            }
        }
    }
}
