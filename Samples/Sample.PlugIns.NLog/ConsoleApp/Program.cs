namespace ConsoleApp
{
    using System;
    using NLog;

    class Program
    {
        // Initialize a log4net's ILog instance:
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            // an example of an informational log via log4net:
            Log.Info("Hello world from NLog!");

            try
            {
                throw new ApplicationException("Oy vey via NLog!");
            }
            catch(Exception ex)
            {
                // an example of an error log via NLog:
                Log.Error("Just FYI...", ex);
            }
        }
    }
}
