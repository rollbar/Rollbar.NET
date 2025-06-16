using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Rollbar.DTOs;

namespace Rollbar.Hangfire
{
    /// <summary>
    /// Hangfire filter that integrates with Rollbar for comprehensive job monitoring and error tracking.
    /// </summary>
    public class RollbarHangfireFilter : IClientFilter, IServerFilter
    {
        private readonly RollbarHangfireOptions _options;
        private readonly IRollbar _rollbar;

        private const string StartTimeKey = "rollbar_start_time";

        /// <summary>
        /// Initializes a new instance of the RollbarHangfireFilter class.
        /// </summary>
        /// <param name="options">Configuration options for the filter</param>
        public RollbarHangfireFilter(RollbarHangfireOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _rollbar = RollbarLocator.RollbarInstance;
        }

        /// <summary>
        /// Initializes a new instance of the RollbarHangfireFilter class with a specific Rollbar instance.
        /// </summary>
        /// <param name="options">Configuration options for the filter</param>
        /// <param name="rollbar">Specific Rollbar instance to use</param>
        public RollbarHangfireFilter(RollbarHangfireOptions options, IRollbar rollbar)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _rollbar = rollbar ?? throw new ArgumentNullException(nameof(rollbar));
        }

        #region IClientFilter Implementation

        /// <summary>
        /// Called before a job is created.
        /// </summary>
        public void OnCreating(CreatingContext context)
        {
            // Job creation tracking can be added here if needed
        }

        /// <summary>
        /// Called after a job is created.
        /// </summary>
        public void OnCreated(CreatedContext context)
        {
            // Additional telemetry can be added here if needed
        }

        #endregion

        #region IServerFilter Implementation

        /// <summary>
        /// Called before a job starts executing.
        /// </summary>
        public void OnPerforming(PerformingContext context)
        {
            if (context?.BackgroundJob?.Job == null) return;

            // Store start time for performance tracking
            context.Items[StartTimeKey] = Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Called after a job finishes executing (success or failure).
        /// </summary>
        public void OnPerformed(PerformedContext context)
        {
            if (context?.BackgroundJob?.Job == null) return;

            var jobInfo = GetJobInfo(context.BackgroundJob.Job);
            var duration = CalculateDuration(context);
            var hasException = context.Exception != null;

            // Handle exceptions
            if (hasException)
            {
                HandleJobException(context, jobInfo, duration);
            }
            else if (_options.CaptureSuccessfulJobs)
            {
                HandleJobSuccess(context, jobInfo, duration);
            }
        }

        #endregion

        #region Private Methods

        private void HandleJobException(PerformedContext context, JobInfo jobInfo, double duration)
        {
            var exception = context.Exception!;
            var customData = CreateJobCustomData(context, jobInfo, duration, true);
            var errorLevel = DetermineErrorLevel(context, exception);

            try
            {
                var customDataDict = customData as IDictionary<string, object?> ?? 
                    customData.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);
                _rollbar.Log(errorLevel, exception, customDataDict);
            }
            catch (System.Exception rollbarException)
            {
                // Don't let Rollbar errors break job processing
                Debug.WriteLine($"Rollbar.Hangfire: Failed to log exception - {rollbarException.Message}");
            }
        }

        private void HandleJobSuccess(PerformedContext context, JobInfo jobInfo, double duration)
        {
            var customData = CreateJobCustomData(context, jobInfo, duration, false);

            try
            {
                var customDataDict = customData as IDictionary<string, object?> ?? 
                    customData.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);
                _rollbar.Info($"Hangfire job {jobInfo.FullName} completed successfully", customDataDict);
            }
            catch (System.Exception rollbarException)
            {
                // Don't let Rollbar errors break job processing
                Debug.WriteLine($"Rollbar.Hangfire: Failed to log success - {rollbarException.Message}");
            }
        }

        private ErrorLevel DetermineErrorLevel(PerformedContext context, System.Exception exception)
        {
            // Check if this is a retry
            if (_options.CaptureRetryInformation && IsRetryAttempt(context))
            {
                return _options.RetriedJobErrorLevel;
            }

            // Use configured minimum level
            return _options.MinimumJobFailureLevel;
        }

        private bool IsRetryAttempt(PerformedContext context)
        {
            // Check if this job has been retried
            try
            {
                var retryCount = context.GetJobParameter<int>("RetryCount");
                return retryCount > 0;
            }
            catch
            {
                return false;
            }
        }

        private Dictionary<string, object> CreateJobCustomData(PerformedContext context, JobInfo jobInfo, double duration, bool isFailure)
        {
            var customData = new Dictionary<string, object>
            {
                ["hangfire"] = CreateHangfireContext(context, jobInfo, duration, isFailure)
            };

            if (_options.CapturePerformanceMetrics)
            {
                customData["performance"] = CreatePerformanceMetrics(duration);
            }

            if (_options.CaptureWorkerInformation)
            {
                customData["worker"] = CreateWorkerContext();
            }

            return customData;
        }

        private object CreateHangfireContext(PerformedContext context, JobInfo jobInfo, double duration, bool isFailure)
        {
            var hangfireData = new Dictionary<string, object>
            {
                ["jobId"] = context.BackgroundJob.Id,
                ["jobType"] = jobInfo.TypeName,
                ["jobMethod"] = jobInfo.MethodName,
                ["jobFullName"] = jobInfo.FullName,
                ["queue"] = context.BackgroundJob.Job.Queue ?? "default",
                ["createdAt"] = context.BackgroundJob.CreatedAt,
                ["duration"] = Math.Round(duration, 2),
                ["isFailure"] = isFailure
            };

            // Add job arguments if enabled
            if (_options.CaptureJobArguments && context.BackgroundJob.Job.Args != null)
            {
                hangfireData["arguments"] = ScrubJobArguments(context.BackgroundJob.Job.Args);
            }

            // Add retry information if enabled
            if (_options.CaptureRetryInformation)
            {
                try
                {
                    var retryCount = context.GetJobParameter<int>("RetryCount");
                    hangfireData["retryCount"] = retryCount;
                    hangfireData["isRetry"] = retryCount > 0;
                }
                catch
                {
                    hangfireData["retryCount"] = 0;
                    hangfireData["isRetry"] = false;
                }
            }

            // Add state information - simplified for compatibility
            hangfireData["state"] = "Unknown";

            return hangfireData;
        }

        private object CreatePerformanceMetrics(double duration)
        {
            return new Dictionary<string, object>
            {
                ["executionTime"] = Math.Round(duration, 2),
                ["executionTimeUnit"] = "milliseconds"
            };
        }

        private object CreateWorkerContext()
        {
            var workerData = new Dictionary<string, object>
            {
                ["machineName"] = Environment.MachineName,
                ["threadId"] = Environment.CurrentManagedThreadId,
                ["workingSet"] = Environment.WorkingSet
            };

            // Add ProcessId only for newer .NET versions
#if NET5_0_OR_GREATER
            workerData["processId"] = Environment.ProcessId;
#else
            using (var process = System.Diagnostics.Process.GetCurrentProcess())
            {
                workerData["processId"] = process.Id;
            }
#endif

            return workerData;
        }

        private object[] ScrubJobArguments(IReadOnlyList<object> args)
        {
            if (args == null || args.Count == 0)
                return new object[0];

            var scrubbedArgs = new List<object>();
            var maxArgs = Math.Min(args.Count, _options.MaxJobArgumentsCount);

            for (int i = 0; i < maxArgs; i++)
            {
                var arg = args[i];
                if (arg == null)
                {
                    scrubbedArgs.Add("null");
                    continue;
                }

                var argString = arg.ToString();
                if (argString != null && argString.Length > _options.MaxArgumentValueLength)
                {
                    argString = argString.Substring(0, _options.MaxArgumentValueLength) + "...";
                }

                // Simple scrubbing for sensitive field names
                if (!string.IsNullOrEmpty(argString) && ContainsSensitiveData(argString))
                {
                    scrubbedArgs.Add("[SCRUBBED]");
                }
                else
                {
                    scrubbedArgs.Add(argString ?? "null");
                }
            }

            return scrubbedArgs.ToArray();
        }

        private bool ContainsSensitiveData(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            var lowerValue = value.ToLowerInvariant();
            return _options.ScrubFields.Any(field => lowerValue.Contains(field.ToLowerInvariant()));
        }

        private double CalculateDuration(PerformedContext context)
        {
            if (context.Items.TryGetValue(StartTimeKey, out var startTimeObj) && startTimeObj is long startTime)
            {
                var endTime = Stopwatch.GetTimestamp();
                var elapsedTicks = endTime - startTime;
                return (double)elapsedTicks / Stopwatch.Frequency * 1000; // Convert to milliseconds
            }

            return 0;
        }

        private JobInfo GetJobInfo(Job job)
        {
            var typeName = job.Type?.Name ?? "Unknown";
            var methodName = job.Method?.Name ?? "Unknown";
            var fullName = $"{typeName}.{methodName}";

            return new JobInfo(typeName, methodName, fullName);
        }

        #endregion

        #region Helper Classes

        private class JobInfo
        {
            public string TypeName { get; }
            public string MethodName { get; }
            public string FullName { get; }
            
            public JobInfo(string typeName, string methodName, string fullName)
            {
                TypeName = typeName;
                MethodName = methodName;
                FullName = fullName;
            }
        }

        #endregion
    }
}