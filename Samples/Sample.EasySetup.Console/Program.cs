using System;
using Rollbar;

namespace Sample.EasySetup.Console
{
    /// <summary>
    /// Demonstrates the simplest possible Rollbar setup for console applications.
    /// This shows how to get started with Rollbar in just 3 lines of code.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Rollbar Easy Setup Console Sample");
            System.Console.WriteLine("==================================");

            // ============================================================================
            // MINIMAL SETUP (2 lines) - Just get started quickly
            // ============================================================================
            RollbarEasySetup.Init("your-access-token-here");
            RollbarEasySetup.CaptureMessage("Hello from console app!");

            // ============================================================================
            // BASIC USAGE EXAMPLES
            // ============================================================================
            
            // Capture exceptions
            try
            {
                throw new InvalidOperationException("This is a test exception");
            }
            catch (Exception ex)
            {
                RollbarEasySetup.CaptureException(ex);
            }

            // Capture messages with different levels
            RollbarEasySetup.CaptureMessage("Info message", ErrorLevel.Info);
            RollbarEasySetup.CaptureMessage("Warning message", ErrorLevel.Warning);
            RollbarEasySetup.CaptureMessage("Error message", ErrorLevel.Error);

            // Alternative: Use the traditional API (still works)
            RollbarLocator.RollbarInstance.Info("This also works with the traditional API");

            // ============================================================================
            // SLIGHTLY MORE ADVANCED SETUP (3 lines)
            // ============================================================================
            
            // Uncomment this block to see advanced configuration
            /*
            RollbarEasySetup.Init(options => {
                options.AccessToken = "your-access-token-here";
                options.Environment = "development";
                options.WithScrubFields("password", "secret")
                       .WithPerson("user123", "john.doe", "john@example.com");
            });
            
            RollbarEasySetup.CaptureMessage("Advanced setup message");
            */

            System.Console.WriteLine("Messages sent to Rollbar!");
            System.Console.WriteLine("Press any key to exit (will flush pending messages)...");
            System.Console.ReadKey();

            // Flush any pending messages before shutdown
            RollbarEasySetup.Flush();
        }
    }
}