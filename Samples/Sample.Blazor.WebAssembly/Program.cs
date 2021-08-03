namespace Sample.Blazor.WebAssembly
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Rollbar;
    using Rollbar.NetPlatformExtensions;

    using Samples;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            RollbarLoggerConfig rollbarConfig = new RollbarLoggerConfig(
                RollbarSamplesSettings.AccessToken, 
                RollbarSamplesSettings.Environment
                );

            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddLogging(configure =>
                configure
                .AddFilter(levelFilter => levelFilter >= LogLevel.Information)
                .AddProvider(new RollbarLoggerProvider(rollbarConfig))
            );

            await builder.Build().RunAsync();
        }

    }
}
