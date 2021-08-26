namespace Sample.RollbarWithinStronglyNamedAssembly
{
    using System;

    using Rollbar;

    using Samples;

    class Program
    {
        static void Main(string[] args)
        {
            const string rollbarAccessToken = RollbarSamplesSettings.AccessToken;
            const string rollbarEnvironment = RollbarSamplesSettings.Environment;

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
                .Error(new Exception("Sample.RollbarWithinStronglyNamedAssembly sample: trying out the TraceChain", new NullReferenceException()))
                ;

            Console.WriteLine("Hello Rollbar!");
        }

    }
}
