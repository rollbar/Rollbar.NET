namespace ConsoleApp
{
    using System;
    using Logary;
    using Logary.Configuration;
    using Logary.CSharp;
    using Logary.Targets;

    public class Program
    {
        static void Main(string[] args)
        {
            var logary = LogaryFactory
                .New(
                    "Logary.CSharpExample",
                    "laptop", 
                    with => 
                        with.InternalLogger(ILogger.NewConsole(LogLevel.Debug))
                            .Target<LiterateConsole.Builder>("console1")
                    )
                .Result;
            var logger = logary.GetLogger("main");

            logger.LogSimple(MessageModule.Event(LogLevel.Info, "Hello world! From Logary..."));

            try
            {
                throw new ApplicationException("thing went haywire");
            }
            catch (Exception e)
            {
                logger.LogEventFormat(LogLevel.Fatal, "Unhandled {exception}!", e).Wait();
            }

            System.Console.ReadLine();
        }
    }
}
