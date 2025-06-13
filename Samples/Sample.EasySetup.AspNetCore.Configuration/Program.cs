namespace Sample.EasySetup.AspNetCore.Configuration
{
    /// <summary>
    /// Demonstrates Rollbar setup using configuration from appsettings.json.
    /// This is the cleanest approach for production applications.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================================================
            // CONFIGURATION-BASED SETUP (1 line) - Load from appsettings.json
            // ============================================================================
            builder.WebHost.UseRollbarFromConfiguration();

            builder.Services.AddControllers();

            var app = builder.Build();

            // Enable Rollbar middleware
            app.UseRollbar();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.MapControllers();

            app.MapGet("/", () => "Rollbar Configuration-Based Setup Sample");
            app.MapGet("/test-error", () => throw new Exception("Configuration-based test error"));

            app.Run();
        }
    }
}