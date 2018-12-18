using Rollbar;
using System;

namespace Sample.RollbarWithinStronglyNamedAssembly
{
    class Program
    {
        static void Main(string[] args)
        {
            const string rollbarAccessToken = "17965fa5041749b6bf7095a190001ded";
            const string rollbarEnvironment = "RollbarNetSamples";

            var config = new RollbarConfig(rollbarAccessToken) // minimally required Rollbar configuration
            {
                Environment = rollbarEnvironment,
                ScrubFields = new string[]
                {
                    "access_token", // normally, you do not want scrub this specific field (it is operationally critical), but it just proves safety net built into the notifier... 
                    "username",
                }
            };
            RollbarLocator.RollbarInstance
                // minimally required Rollbar configuration:
                .Configure(config)
                ;

            Console.WriteLine("Hello World!");

            RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(5))
                .Info("Sample.RollbarWithinStronglyNamedAssembly sample: Basic info log example.")
                .Debug("Sample.RollbarWithinStronglyNamedAssembly sample: First debug log.")
                .Error(new NullReferenceException("Sample.RollbarWithinStronglyNamedAssembly sample: null reference exception."))
                .Error(new System.Exception("Sample.RollbarWithinStronglyNamedAssembly sample: trying out the TraceChain", new NullReferenceException()))
                ;

            Console.WriteLine("Hello Rollbar!");
        }

    }
}
