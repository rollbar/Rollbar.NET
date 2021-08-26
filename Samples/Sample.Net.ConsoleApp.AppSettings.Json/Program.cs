namespace Sample.Net.ConsoleApp.AppSettings.Json
{
    using System;

    using Rollbar;
    using Rollbar.AppSettings.Json;

    class Program
    {
        static void Main(string[] args)
        {
            RollbarInfrastructureConfig rollbarInfrastructureConfig = new RollbarInfrastructureConfig();
            AppSettingsUtility.LoadAppSettings(rollbarInfrastructureConfig);
            RollbarInfrastructure.Instance.Init(rollbarInfrastructureConfig);

            RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(3))
                .Info($"{typeof(Program).Namespace} sample: Rollbar Notifier is alive based on appsetting.json!");
        }
    }
}
