# Rollbar .NET SDK - Easy Setup Guide

This guide shows you how to get started with Rollbar in your .NET applications using our simplified setup API. You can be up and running in just 2-3 lines of code!

## Quick Start

### Console Applications

```csharp
using Rollbar;

// 2-line setup
RollbarEasySetup.Init("your-access-token-here");
RollbarEasySetup.CaptureException(new Exception("Test error"));
```

### ASP.NET Core Applications

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1 line to add Rollbar
builder.WebHost.UseRollbar("your-access-token-here");

var app = builder.Build();

// 1 line to enable middleware
app.UseRollbar();

app.MapGet("/", () => "Hello World!");
app.Run();
```

### Worker Services / Generic Host

```csharp
var builder = Host.CreateDefaultBuilder(args);

// 1 line setup
builder.UseRollbar("your-access-token-here");

var host = builder.Build();
host.Run();
```

## Installation

Install the Rollbar NuGet package:

```
dotnet add package Rollbar
```

For ASP.NET Core applications, also install:

```
dotnet add package Rollbar.NetCore.AspNet
```

## Setup Options

### 1. Minimal Setup (Production Ready)

**Console:**
```csharp
RollbarEasySetup.Init("your-access-token");
```

**ASP.NET Core:**
```csharp
builder.WebHost.UseRollbar("your-access-token");
```

### 2. Environment-Specific Setup

```csharp
// Console
RollbarEasySetup.Init("your-access-token", "development");

// ASP.NET Core
builder.WebHost.UseRollbar("your-access-token", "staging");
```

### 3. Advanced Configuration

```csharp
RollbarEasySetup.Init(options => {
    options.AccessToken = "your-access-token";
    options.Environment = "development";
    options.MinLevel = ErrorLevel.Warning;
    options.WithScrubFields("password", "secret")
           .WithPerson("user123", "john.doe", "john@example.com");
});
```

### 4. Configuration File Setup (Recommended)

**appsettings.json:**
```json
{
  "Rollbar": {
    "AccessToken": "your-access-token-here",
    "Environment": "production",
    "ScrubFields": ["password", "secret", "creditCard"],
    "MinLevel": "Warning",
    "Enabled": true
  }
}
```

**Program.cs:**
```csharp
// ASP.NET Core
builder.WebHost.UseRollbarFromConfiguration();

// Generic Host / Worker Service
builder.UseRollbarFromConfiguration();
```

## Usage Examples

### Capturing Exceptions

```csharp
try 
{
    // Your code here
}
catch (Exception ex)
{
    RollbarEasySetup.CaptureException(ex);
    // or use the traditional API
    RollbarLocator.RollbarInstance.Error(ex);
}
```

### Logging Messages

```csharp
RollbarEasySetup.CaptureMessage("Something happened", ErrorLevel.Info);
RollbarEasySetup.CaptureMessage("Warning message", ErrorLevel.Warning);
RollbarEasySetup.CaptureMessage("Error occurred", ErrorLevel.Error);
```

### Using with Microsoft.Extensions.Logging

```csharp
// In your controller or service
public class MyController : ControllerBase
{
    private readonly ILogger<MyController> _logger;

    public MyController(ILogger<MyController> logger)
    {
        _logger = logger;
    }

    public IActionResult MyAction()
    {
        _logger.LogInformation("This will be sent to Rollbar");
        _logger.LogError("This error will also be sent to Rollbar");
        return Ok();
    }
}
```

### Flushing Before Shutdown (Console Apps)

```csharp
// At the end of your console application
RollbarEasySetup.Flush(); // Waits up to 5 seconds for pending messages
```

## Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `AccessToken` | string | *required* | Your Rollbar access token |
| `Environment` | string | "production" | Environment name (production, staging, development, etc.) |
| `MinLevel` | ErrorLevel | Info | Minimum log level to capture |
| `ScrubFields` | string[] | null | Field names to scrub from payloads |
| `PersonId` | string | null | User ID for associating errors |
| `PersonUsername` | string | null | Username for associating errors |
| `PersonEmail` | string | null | Email for associating errors |
| `Enabled` | bool | true | Whether Rollbar is enabled |
| `Transmit` | bool | true | Whether to actually send data (false for testing) |
| `EndPoint` | string | null | Custom Rollbar endpoint URL |

## Framework-Specific Setup

### ASP.NET Core with Dependency Injection

```csharp
builder.Services.AddRollbar(options => {
    options.AccessToken = "your-token";
    options.Environment = "development";
});
```

### Adding to Logging Pipeline

```csharp
builder.Logging.AddRollbar("your-access-token");
```

### .NET Framework Applications

```csharp
// Works with .NET Framework 4.6.2+
RollbarEasySetup.Init("your-access-token");
```

## Migration from Traditional Setup

If you're currently using the traditional Rollbar setup, you can gradually migrate:

**Before (Complex):**
```csharp
var config = new RollbarInfrastructureConfig("token", "env");
config.RollbarLoggerConfig.RollbarDataSecurityOptions.Reconfigure(opts => {
    opts.ScrubFields = new[] { "password" };
});
RollbarInfrastructure.Instance.Init(config);
```

**After (Simple):**
```csharp
RollbarEasySetup.Init(options => {
    options.AccessToken = "token";
    options.Environment = "env";
    options.WithScrubFields("password");
});
```

The traditional API (`RollbarLocator.RollbarInstance`) continues to work exactly as before.

## Advanced Scenarios

### Custom Configuration Section

```csharp
// Use a different configuration section name
builder.WebHost.UseRollbarFromConfiguration("MyRollbarSettings");
```

### Multiple Rollbar Instances

```csharp
// The easy setup uses the global instance, but you can still create custom instances
var customRollbar = RollbarFactory.CreateNew(customConfig);
```

### Integration with Existing Logging

The easy setup automatically integrates with:
- Microsoft.Extensions.Logging
- ASP.NET Core request logging
- Unhandled exception capturing

## Troubleshooting

### Access Token Not Set
```
InvalidOperationException: Rollbar AccessToken is required
```
Make sure you've set the `AccessToken` in your configuration or code.

### Configuration Section Not Found
```
InvalidOperationException: Configuration section 'Rollbar' not found
```
Add the Rollbar section to your `appsettings.json` file.

### Messages Not Appearing in Rollbar
1. Check that `Enabled` is `true`
2. Check that `Transmit` is `true`
3. Verify your access token is correct
4. Check the minimum log level setting

## Samples

See the `Samples` folder for complete working examples:
- `Sample.EasySetup.Console` - Basic console application
- `Sample.EasySetup.AspNetCore` - ASP.NET Core web application
- `Sample.EasySetup.AspNetCore.Configuration` - Configuration-based setup

## Comparison with Other SDKs

This easy setup API is inspired by popular error tracking libraries and provides:

✅ **2-3 line setup** (vs 15-30 lines traditionally)  
✅ **Sensible defaults** for production use  
✅ **Configuration file support** for clean separation  
✅ **Framework-native integration** (follows .NET conventions)  
✅ **Progressive complexity** (start simple, add features as needed)  
✅ **Backward compatibility** (existing code continues to work)  

The traditional Rollbar API remains fully functional and available for advanced scenarios.