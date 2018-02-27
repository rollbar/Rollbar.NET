#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Rollbar.Diagnostics;
    using Rollbar.NetCore;

    public static class RollbarConfigurationUtil
    {
        public static IRollbarConfig DeduceRollbarConfig(IConfiguration configuration)
        {
            if (RollbarLocator.RollbarInstance.Config.AccessToken != null)
            {
                return RollbarLocator.RollbarInstance.Config;
            }

            // Here we assume that the Rollbar singleton was not explicitly preconfigured 
            // anywhere in the code (Program.cs or Startup.cs), 
            // so we are trying to configure it from IConfiguration:

            Assumption.AssertNotNull(configuration, nameof(configuration));

            const string defaultAccessToken = "none";
            RollbarConfig rollbarConfig = new RollbarConfig(defaultAccessToken);
            AppSettingsUtil.LoadAppSettings(ref rollbarConfig, configuration);

            if (rollbarConfig.AccessToken == defaultAccessToken)
            {
                const string error = "Rollbar.NET notifier is not configured properly.";
                throw new Exception(error);
            }

            RollbarLocator.RollbarInstance
                .Configure(rollbarConfig);

            return rollbarConfig;
        }

    }
}

#endif
