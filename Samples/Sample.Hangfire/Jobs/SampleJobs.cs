using Rollbar;
using Rollbar.Hangfire;

namespace Sample.Hangfire.Jobs
{
    /// <summary>
    /// Sample Hangfire jobs demonstrating Rollbar integration features.
    /// </summary>
    public class SampleJobs
    {
        /// <summary>
        /// A simple job that completes successfully.
        /// </summary>
        [RollbarJob(ErrorLevel.Info, "sample,simple")]
        public void SimpleJob(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] Simple Job: {message}");
            Thread.Sleep(1000); // Simulate some work
            Console.WriteLine($"[{DateTime.Now}] Simple Job completed successfully");
        }

        /// <summary>
        /// A job that always fails to demonstrate error capture.
        /// </summary>
        [RollbarJob(ErrorLevel.Error, "sample,failure")]
        public void FailingJob(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] Failing Job: {message}");
            Thread.Sleep(500); // Simulate some work before failure
            
            throw new InvalidOperationException($"Simulated job failure: {message}");
        }

        /// <summary>
        /// A critical job that should be monitored closely.
        /// </summary>
        [RollbarJob(ErrorLevel.Critical, "sample,critical", 
                   IsCritical = true, 
                   Description = "Critical business operation that must be monitored")]
        public void CriticalJob(string operation)
        {
            Console.WriteLine($"[{DateTime.Now}] Critical Job: {operation}");
            
            // Simulate critical operation
            Thread.Sleep(2000);
            
            // 50% chance of failure for demonstration
            if (Random.Shared.Next(0, 2) == 0)
            {
                throw new Exception($"Critical operation failed: {operation}");
            }
            
            Console.WriteLine($"[{DateTime.Now}] Critical Job completed successfully");
        }

        /// <summary>
        /// A job with sensitive data that should be scrubbed.
        /// </summary>
        [RollbarJob(ErrorLevel.Warning, "sample,sensitive", 
                   CaptureArguments = true)] // Arguments will be captured but scrubbed
        public void JobWithSensitiveData(string userId, string password, string apiToken)
        {
            Console.WriteLine($"[{DateTime.Now}] Processing user: {userId}");
            
            // Simulate work with sensitive data
            Thread.Sleep(1500);
            
            // Sometimes fail to show how sensitive data is handled
            if (password.Contains("123"))
            {
                throw new UnauthorizedAccessException("Invalid credentials provided");
            }
            
            Console.WriteLine($"[{DateTime.Now}] User processing completed");
        }

        /// <summary>
        /// A long-running job to demonstrate performance monitoring.
        /// </summary>
        [RollbarJob(ErrorLevel.Info, "sample,performance", 
                   CapturePerformanceMetrics = true,
                   Description = "Long running job for performance testing")]
        public void LongRunningJob(int durationSeconds)
        {
            Console.WriteLine($"[{DateTime.Now}] Starting long running job ({durationSeconds}s)");
            
            var startTime = DateTime.Now;
            var endTime = startTime.AddSeconds(durationSeconds);
            
            while (DateTime.Now < endTime)
            {
                // Simulate CPU work
                var iterations = 1000000;
                var sum = 0;
                for (int i = 0; i < iterations; i++)
                {
                    sum += i;
                }
                
                Thread.Sleep(100);
                
                // Log progress
                var elapsed = DateTime.Now - startTime;
                if (elapsed.TotalSeconds % 5 == 0)
                {
                    Console.WriteLine($"[{DateTime.Now}] Long running job progress: {elapsed.TotalSeconds:F1}s / {durationSeconds}s");
                }
            }
            
            Console.WriteLine($"[{DateTime.Now}] Long running job completed in {(DateTime.Now - startTime).TotalSeconds:F1}s");
        }

        /// <summary>
        /// A recurring job that runs periodically.
        /// </summary>
        [RollbarJob(ErrorLevel.Info, "sample,recurring")]
        public void RecurringJob()
        {
            Console.WriteLine($"[{DateTime.Now}] Recurring job executed");
            
            // Simulate periodic task
            Thread.Sleep(500);
            
            // Occasionally fail to demonstrate retry handling
            if (DateTime.Now.Second % 7 == 0)
            {
                throw new TimeoutException("Simulated timeout in recurring job");
            }
            
            Console.WriteLine($"[{DateTime.Now}] Recurring job completed");
        }

        /// <summary>
        /// A startup job that runs when the application starts.
        /// </summary>
        [RollbarJob(ErrorLevel.Info, "sample,startup", 
                   CaptureArguments = false)] // Don't capture startup arguments
        public void StartupJob(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] Startup Job: {message}");
            
            // Simulate initialization work
            Thread.Sleep(800);
            
            Console.WriteLine($"[{DateTime.Now}] Startup Job completed");
        }

        /// <summary>
        /// A job that demonstrates retry behavior.
        /// </summary>
        [RollbarJob(ErrorLevel.Warning, "sample,retry",
                   RetryErrorLevel = ErrorLevel.Info)]
        public void RetryableJob(string operation, int maxAttempts = 3)
        {
            Console.WriteLine($"[{DateTime.Now}] Retryable Job: {operation}");
            
            // Get the current attempt number (this would typically come from Hangfire context)
            var currentAttempt = Random.Shared.Next(1, maxAttempts + 2);
            
            if (currentAttempt <= maxAttempts)
            {
                Console.WriteLine($"[{DateTime.Now}] Attempt {currentAttempt} failed, will retry");
                throw new Exception($"Temporary failure on attempt {currentAttempt}");
            }
            
            Console.WriteLine($"[{DateTime.Now}] Retryable Job succeeded on attempt {currentAttempt}");
        }

        /// <summary>
        /// A job that doesn't want to be tracked by Rollbar.
        /// </summary>
        [RollbarJob(DisableTracking = true)]
        public void UnmonitoredJob()
        {
            Console.WriteLine($"[{DateTime.Now}] Unmonitored job - this won't appear in Rollbar");
            
            // Even if this job fails, it won't be reported to Rollbar
            if (Random.Shared.Next(0, 2) == 0)
            {
                throw new Exception("This exception won't be reported to Rollbar");
            }
            
            Console.WriteLine($"[{DateTime.Now}] Unmonitored job completed");
        }

        /// <summary>
        /// A data processing job that works with large datasets.
        /// </summary>
        [RollbarJob(ErrorLevel.Error, "sample,data-processing",
                   CapturePerformanceMetrics = true,
                   MaxArgumentsCount = 10,
                   Description = "Processes large datasets")]
        public void DataProcessingJob(string[] dataFiles, int batchSize, string outputPath)
        {
            Console.WriteLine($"[{DateTime.Now}] Processing {dataFiles.Length} files in batches of {batchSize}");
            
            foreach (var file in dataFiles)
            {
                Console.WriteLine($"[{DateTime.Now}] Processing file: {file}");
                Thread.Sleep(200); // Simulate file processing
                
                // Simulate occasional file processing errors
                if (file.Contains("corrupt"))
                {
                    throw new FileLoadException($"Corrupted file detected: {file}");
                }
            }
            
            Console.WriteLine($"[{DateTime.Now}] Data processing completed. Output: {outputPath}");
        }
    }
}