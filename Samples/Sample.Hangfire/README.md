# Rollbar.Hangfire Sample Application

This sample demonstrates how to integrate Rollbar error tracking with Hangfire background job processing in an ASP.NET Core application.

## Features Demonstrated

- **Automatic Error Capture**: Background job exceptions are automatically reported to Rollbar
- **Job Context Information**: Rich job metadata including arguments, duration, and worker info
- **Performance Monitoring**: Job execution time and resource usage tracking
- **Security**: Automatic scrubbing of sensitive data in job arguments
- **Telemetry**: Job lifecycle events as breadcrumbs
- **Per-Job Configuration**: Custom error levels and monitoring via attributes

## Setup Instructions

### 1. Prerequisites

- .NET 8.0 or later
- SQL Server or LocalDB for Hangfire storage
- Rollbar account and access token

### 2. Configuration

1. **Update appsettings.json** with your Rollbar access token:
   ```json
   {
     "Rollbar": {
       "AccessToken": "your-actual-rollbar-access-token",
       "Environment": "development",
       "Enabled": true,
       "Transmit": true
     }
   }
   ```

2. **Update connection string** if needed:
   ```json
   {
     "ConnectionStrings": {
       "HangfireConnection": "your-sql-server-connection-string"
     }
   }
   ```

### 3. Running the Application

1. **Start the application**:
   ```bash
   dotnet run
   ```

2. **Access the Hangfire Dashboard** at: `http://localhost:5000/hangfire`

3. **Trigger sample jobs** using the endpoints:
   - `POST /jobs/simple` - Simple successful job
   - `POST /jobs/failing` - Job that always fails
   - `POST /jobs/critical` - Critical job with 50% failure rate
   - `POST /jobs/with-sensitive-data` - Job with data scrubbing
   - `POST /jobs/long-running` - Performance monitoring demo
   - `POST /jobs/recurring` - Sets up recurring job

## Sample Jobs Overview

### SimpleJob
- **Purpose**: Demonstrates basic job execution tracking
- **Features**: Success telemetry, basic job context
- **Configuration**: `[RollbarJob(ErrorLevel.Info, "sample,simple")]`

### FailingJob
- **Purpose**: Shows automatic exception capture
- **Features**: Error reporting, stack trace capture, job context
- **Configuration**: `[RollbarJob(ErrorLevel.Error, "sample,failure")]`

### CriticalJob
- **Purpose**: Demonstrates critical job monitoring
- **Features**: High-priority error reporting, custom fingerprinting
- **Configuration**: `[RollbarJob(ErrorLevel.Critical, IsCritical = true)]`

### JobWithSensitiveData
- **Purpose**: Shows sensitive data scrubbing
- **Features**: Argument capture with automatic scrubbing of passwords/tokens
- **Configuration**: `[RollbarJob(CaptureArguments = true)]`

### LongRunningJob
- **Purpose**: Performance monitoring demonstration
- **Features**: Execution time tracking, resource usage metrics
- **Configuration**: `[RollbarJob(CapturePerformanceMetrics = true)]`

### RecurringJob
- **Purpose**: Periodic job monitoring
- **Features**: Recurring failure pattern tracking, retry detection
- **Configuration**: `[RollbarJob(ErrorLevel.Info, "sample,recurring")]`

## Rollbar Integration Configuration

The application configures Rollbar Hangfire integration with comprehensive options:

```csharp
.UseRollbar(options => {
    options.CaptureJobArguments = true;           // Capture job parameters
    options.CapturePerformanceMetrics = true;     // Track execution time/resources
    options.CaptureWorkerInformation = true;      // Include worker/machine info
    options.ScrubFields = new[] { "password", "token", "secret", "apiKey" };
    options.MinimumJobFailureLevel = ErrorLevel.Error;
    options.EnableTelemetry = true;               // Job lifecycle breadcrumbs
    options.CaptureSuccessfulJobs = false;        // Only capture failures
    options.CaptureRetryInformation = true;       // Track retry attempts
    options.RetriedJobErrorLevel = ErrorLevel.Warning;
})
```

## What Gets Reported to Rollbar

### For Failed Jobs:
- **Exception Details**: Full stack trace and error message
- **Job Context**: Job ID, type, method, queue, creation time
- **Job Arguments**: Parameter values (with sensitive data scrubbed)
- **Performance Metrics**: Execution duration, memory usage, GC stats
- **Worker Information**: Machine name, process ID, thread info
- **Retry Information**: Attempt count, retry status
- **Custom Tags**: From `[RollbarJob]` attributes

### For Successful Jobs (if enabled):
- **Job Context**: Same context information as failures
- **Performance Metrics**: Execution statistics
- **Success Confirmation**: Completion status and duration

### Telemetry Events:
- Job creation events
- Job start events  
- Job completion events (success/failure)
- Performance milestones for long-running jobs

## Testing the Integration

1. **Trigger a failing job** and check Rollbar for the error report
2. **Monitor the Hangfire Dashboard** to see job execution
3. **Check Rollbar breadcrumbs** for job lifecycle events
4. **Verify sensitive data scrubbing** by triggering jobs with passwords
5. **Test performance monitoring** with long-running jobs

## Production Considerations

- Set `Transmit = true` in production
- Use appropriate error levels for your business criticality
- Configure scrub fields for your specific sensitive data
- Consider disabling `CaptureSuccessfulJobs` to reduce noise
- Use SQL Server (not LocalDB) for production Hangfire storage
- Implement proper authentication for Hangfire Dashboard

## Advanced Usage

### Custom Job Configuration
```csharp
[RollbarJob(
    ErrorLevel = ErrorLevel.Critical,
    Tags = "payment,financial", 
    CaptureArguments = false,  // Skip sensitive payment data
    IsCritical = true,
    Description = "Payment processing job"
)]
public void ProcessPayment(PaymentRequest request) { }
```

### Multiple Rollbar Instances
```csharp
var criticalRollbar = RollbarFactory.CreateNew(criticalConfig);
builder.Services.AddHangfire(config => config
    .UseRollbar(criticalRollbar, options => {
        options.MinimumJobFailureLevel = ErrorLevel.Critical;
    }));
```

This sample provides a comprehensive demonstration of Rollbar's Hangfire integration capabilities, showing how to monitor background jobs effectively while maintaining security and performance.