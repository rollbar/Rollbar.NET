[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace ConsoleApp
{
    using System;
    using log4net;

    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            log.Info("Hello world from Log4net!");

            try
            {
                throw new ApplicationException("Oy vey via Log4net!");
            }
            catch(Exception ex)
            {
                log.Error("Just FYI...", ex);
            }
            //Console.WriteLine("Hit enter to exit...");
            //Console.ReadLine();
        }
    }
}
