using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rollbar;
using Rollbar.NetCore.AspNet;

namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// Easy setup extensions for ASP.NET Core applications.
    /// Provides simple 2-3 line Rollbar integration following Sentry-style patterns.
    /// </summary>
    public static class RollbarWebHostBuilderExtensions
    {
        /// <summary>
        /// Add Rollbar error logging to ASP.NET Core with just an access token.
        /// Automatically configures middleware, logging, and sets up for the production environment.
        /// </summary>
        /// <param name="builder">The web host builder</param>
        /// <param name="accessToken">Your Rollbar access token</param>
        /// <returns>The web host builder for chaining</returns>
        /// <example>
        /// var builder = WebApplication.CreateBuilder(args);
        /// builder.WebHost.UseRollbar("your-access-token");
        /// var app = builder.Build();
        /// </example>
        public static IWebHostBuilder UseRollbar(this IWebHostBuilder builder, string accessToken)
        {
            return builder.UseRollbar(accessToken, "production");
        }

        /// <summary>
        /// Add Rollbar error logging to ASP.NET Core with access token and environment.
        /// </summary>
        /// <param name="builder">The web host builder</param>
        /// <param name="accessToken">Your Rollbar access token</param>
        /// <param name="environment">Environment name (e.g., "production", "staging", "development")</param>
        /// <returns>The web host builder for chaining</returns>
        /// <example>
        /// builder.WebHost.UseRollbar("your-access-token", "development");
        /// </example>
        public static IWebHostBuilder UseRollbar(this IWebHostBuilder builder, string accessToken, string environment)
        {
            return builder.UseRollbar(options =>
            {
                options.AccessToken = accessToken;
                options.Environment = environment;
            });
        }

        /// <summary>
        /// Add Rollbar error logging to ASP.NET Core with advanced configuration.
        /// </summary>
        /// <param name="builder">The web host builder</param>
        /// <param name="configure">Configuration action for Rollbar options</param>
        /// <returns>The web host builder for chaining</returns>
        /// <example>
        /// builder.WebHost.UseRollbar(options => {
        ///     options.AccessToken = "your-token";
        ///     options.Environment = "development";
        ///     options.WithScrubFields("password", "secret")
        ///            .WithPerson("user123", "john.doe");
        /// });
        /// </example>
        public static IWebHostBuilder UseRollbar(this IWebHostBuilder builder, Action<SimpleRollbarOptions> configure)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddRollbar(configure);
            });
        }
    }
}

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Easy setup extensions for ASP.NET Core application builder.
    /// </summary>
    public static class RollbarApplicationBuilderExtensions
    {
        /// <summary>
        /// Add Rollbar error logging middleware to the ASP.NET Core pipeline.
        /// Call this if you used the UseRollbar() extension method on WebHostBuilder.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder for chaining</returns>
        /// <example>
        /// var app = builder.Build();
        /// app.UseRollbar(); // Add this line
        /// app.MapGet("/", () => "Hello World!");
        /// </example>
        public static IApplicationBuilder UseRollbar(this IApplicationBuilder app)
        {
            return app.UseRollbarMiddleware();
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Easy setup extensions for dependency injection.
    /// </summary>
    public static class RollbarServiceCollectionExtensions
    {
        /// <summary>
        /// Add Rollbar services to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="accessToken">Your Rollbar access token</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddRollbar(this IServiceCollection services, string accessToken)
        {
            return services.AddRollbar(accessToken, "production");
        }

        /// <summary>
        /// Add Rollbar services to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="accessToken">Your Rollbar access token</param>
        /// <param name="environment">Environment name</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddRollbar(this IServiceCollection services, string accessToken, string environment)
        {
            return services.AddRollbar(options =>
            {
                options.AccessToken = accessToken;
                options.Environment = environment;
            });
        }

        /// <summary>
        /// Add Rollbar services to the dependency injection container with configuration.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configure">Configuration action for Rollbar options</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddRollbar(this IServiceCollection services, Action<SimpleRollbarOptions> configure)
        {
            // Initialize Rollbar infrastructure
            RollbarEasySetup.Init(configure);

            // Add Rollbar logging services using existing extension
            services.AddRollbarLoggerProvider();

            return services;
        }
    }
}

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Easy setup extensions for Microsoft.Extensions.Logging.
    /// </summary>
    public static class RollbarLoggingBuilderExtensions
    {
        /// <summary>
        /// Add Rollbar to the logging builder with simple configuration.
        /// </summary>
        /// <param name="builder">The logging builder</param>
        /// <param name="accessToken">Your Rollbar access token</param>
        /// <returns>The logging builder for chaining</returns>
        /// <example>
        /// builder.Logging.AddRollbar("your-access-token");
        /// </example>
        public static ILoggingBuilder AddRollbar(this ILoggingBuilder builder, string accessToken)
        {
            return builder.AddRollbar(accessToken, "production");
        }

        /// <summary>
        /// Add Rollbar to the logging builder with simple configuration.
        /// </summary>
        /// <param name="builder">The logging builder</param>
        /// <param name="accessToken">Your Rollbar access token</param>
        /// <param name="environment">Environment name</param>
        /// <returns>The logging builder for chaining</returns>
        public static ILoggingBuilder AddRollbar(this ILoggingBuilder builder, string accessToken, string environment)
        {
            return builder.AddRollbar(options =>
            {
                options.AccessToken = accessToken;
                options.Environment = environment;
            });
        }

        /// <summary>
        /// Add Rollbar to the logging builder with advanced configuration.
        /// </summary>
        /// <param name="builder">The logging builder</param>
        /// <param name="configure">Configuration action for Rollbar options</param>
        /// <returns>The logging builder for chaining</returns>
        /// <example>
        /// builder.Logging.AddRollbar(options => {
        ///     options.AccessToken = "your-token";
        ///     options.Environment = "development";
        ///     options.MinLevel = ErrorLevel.Warning;
        /// });
        /// </example>
        public static ILoggingBuilder AddRollbar(this ILoggingBuilder builder, Action<SimpleRollbarOptions> configure)
        {
            // Initialize Rollbar infrastructure
            RollbarEasySetup.Init(configure);

            // Add to logging using existing extension method
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, RollbarLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerFactory, RollbarLoggerFactory>());

            return builder;
        }
    }
}

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Easy setup extensions for generic host applications.
    /// </summary>
    public static class RollbarHostBuilderExtensions
    {
        /// <summary>
        /// Add Rollbar to a generic host application (console apps, worker services, etc.).
        /// </summary>
        /// <param name="builder">The host builder</param>
        /// <param name="accessToken">Your Rollbar access token</param>
        /// <returns>The host builder for chaining</returns>
        /// <example>
        /// var builder = Host.CreateDefaultBuilder(args);
        /// builder.UseRollbar("your-access-token");
        /// var host = builder.Build();
        /// </example>
        public static IHostBuilder UseRollbar(this IHostBuilder builder, string accessToken)
        {
            return builder.UseRollbar(accessToken, "production");
        }

        /// <summary>
        /// Add Rollbar to a generic host application.
        /// </summary>
        /// <param name="builder">The host builder</param>
        /// <param name="accessToken">Your Rollbar access token</param>
        /// <param name="environment">Environment name</param>
        /// <returns>The host builder for chaining</returns>
        public static IHostBuilder UseRollbar(this IHostBuilder builder, string accessToken, string environment)
        {
            return builder.UseRollbar(options =>
            {
                options.AccessToken = accessToken;
                options.Environment = environment;
            });
        }

        /// <summary>
        /// Add Rollbar to a generic host application with advanced configuration.
        /// </summary>
        /// <param name="builder">The host builder</param>
        /// <param name="configure">Configuration action for Rollbar options</param>
        /// <returns>The host builder for chaining</returns>
        public static IHostBuilder UseRollbar(this IHostBuilder builder, Action<SimpleRollbarOptions> configure)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddRollbar(configure);
            }).ConfigureLogging(logging =>
            {
                logging.AddRollbar(configure);
            });
        }
    }
}