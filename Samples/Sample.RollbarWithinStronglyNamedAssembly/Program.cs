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

            var config = new RollbarInfrastructureConfig(rollbarAccessToken, rollbarEnvironment); // minimally required Rollbar configuration
            RollbarInfrastructure.Instance.Init(config);

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
