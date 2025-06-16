using Hangfire;
using Hangfire.SqlServer;
using Rollbar;
using Rollbar.Hangfire;
using Sample.Hangfire.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Initialize Rollbar first
RollbarEasySetup.Init(options => {
    options.AccessToken = "your-rollbar-access-token-here";
    options.Environment = "development";
    options.Enabled = true;
    options.Transmit = false; // Set to true in production
});

// Add Hangfire services
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection") ?? 
        "Server=(localdb)\\mssqllocaldb;Database=HangfireTest;Trusted_Connection=true;MultipleActiveResultSets=true",
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        })
    // Configure Rollbar integration with comprehensive options
    .UseRollbar(options => {
        options.CaptureJobArguments = true;
        options.CapturePerformanceMetrics = true;
        options.CaptureWorkerInformation = true;
        options.ScrubFields = new[] { "password", "token", "secret", "apiKey" };
        options.MinimumJobFailureLevel = ErrorLevel.Error;
        options.EnableTelemetry = true;
        options.CaptureSuccessfulJobs = false; // Only capture failures by default
        options.CaptureRetryInformation = true;
        options.RetriedJobErrorLevel = ErrorLevel.Warning;
    }));

// Add Hangfire server
builder.Services.AddHangfireServer();

// Add controllers for the web interface
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

// Add Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
});

app.MapControllers();

// Schedule some sample jobs
app.MapGet("/", () => "Hangfire with Rollbar Sample - Visit /hangfire for dashboard");

app.MapPost("/jobs/simple", () =>
{
    var jobId = BackgroundJob.Enqueue<SampleJobs>(x => x.SimpleJob("Hello from simple job!"));
    return Results.Ok(new { JobId = jobId, Message = "Simple job enqueued" });
});

app.MapPost("/jobs/failing", () =>
{
    var jobId = BackgroundJob.Enqueue<SampleJobs>(x => x.FailingJob("This job will fail"));
    return Results.Ok(new { JobId = jobId, Message = "Failing job enqueued" });
});

app.MapPost("/jobs/critical", () =>
{
    var jobId = BackgroundJob.Enqueue<SampleJobs>(x => x.CriticalJob("Critical operation"));
    return Results.Ok(new { JobId = jobId, Message = "Critical job enqueued" });
});

app.MapPost("/jobs/with-sensitive-data", () =>
{
    var jobId = BackgroundJob.Enqueue<SampleJobs>(x => x.JobWithSensitiveData("user123", "password123", "secret-token"));
    return Results.Ok(new { JobId = jobId, Message = "Job with sensitive data enqueued" });
});

app.MapPost("/jobs/long-running", () =>
{
    var jobId = BackgroundJob.Enqueue<SampleJobs>(x => x.LongRunningJob(30));
    return Results.Ok(new { JobId = jobId, Message = "Long running job enqueued" });
});

app.MapPost("/jobs/recurring", () =>
{
    RecurringJob.AddOrUpdate<SampleJobs>("recurring-sample", x => x.RecurringJob(), Cron.Minutely);
    return Results.Ok(new { Message = "Recurring job scheduled (every minute)" });
});

// Schedule some initial jobs on startup
using (var scope = app.Services.CreateScope())
{
    BackgroundJob.Enqueue<SampleJobs>(x => x.StartupJob("Application started"));
}

app.Run();

// Allow all users to access Hangfire Dashboard in development
public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true; // Allow all in development
    }
}