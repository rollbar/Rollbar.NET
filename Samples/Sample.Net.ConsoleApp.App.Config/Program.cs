namespace Sample.Net.ConsoleApp.App.Config
{
    using System;

    using Rollbar;
    using Rollbar.App.Config;

    class Program
    {
        static void Main(string[] args)
        {
            RollbarInfrastructureConfig rollbarInfrastructureConfig = new RollbarInfrastructureConfig();
            AppConfigUtility.LoadAppSettings(rollbarInfrastructureConfig);
            RollbarInfrastructure.Instance.Init(rollbarInfrastructureConfig);

            RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(3))
                .Info($"{typeof(Program).Namespace} sample: Rollbar is alive based on the app.config settings!");
        }
    }
}
