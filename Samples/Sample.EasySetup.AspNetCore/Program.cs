using Microsoft.AspNetCore.Mvc;

namespace Sample.EasySetup.AspNetCore
{
    /// <summary>
    /// Demonstrates the simplest possible Rollbar setup for ASP.NET Core applications.
    /// This shows how to get Rollbar working in just 2-3 lines of code.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================================================
            // MINIMAL SETUP (1 line) - Add Rollbar error logging
            // ============================================================================
            builder.WebHost.UseRollbar("your-access-token-here");

            // Add standard ASP.NET Core services
            builder.Services.AddControllers();

            var app = builder.Build();

            // ============================================================================
            // MINIMAL SETUP (1 line) - Enable Rollbar middleware
            // ============================================================================
            app.UseRollbar();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.MapControllers();

            // Simple test endpoints
            app.MapGet("/", () => "Rollbar Easy Setup ASP.NET Core Sample is running!");
            
            app.MapGet("/test-error", () => 
            {
                throw new InvalidOperationException("This is a test exception from ASP.NET Core");
            });

            app.MapGet("/test-log", (ILogger<Program> logger) => 
            {
                logger.LogInformation("Test info message");
                logger.LogWarning("Test warning message");
                logger.LogError("Test error message");
                return "Log messages sent!";
            });

            app.Run();
        }
    }

    /// <summary>
    /// Sample controller to demonstrate Rollbar integration
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("exception")]
        public IActionResult ThrowException()
        {
            throw new InvalidOperationException("Controller exception for testing");
        }

        [HttpGet("log")]
        public IActionResult LogMessages()
        {
            _logger.LogInformation("Controller info message");
            _logger.LogWarning("Controller warning message");
            _logger.LogError("Controller error message");
            return Ok("Messages logged!");
        }
    }
}