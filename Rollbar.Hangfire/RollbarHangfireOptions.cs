using System;
using Rollbar;

namespace Rollbar.Hangfire
{
    /// <summary>
    /// Configuration options for Rollbar Hangfire integration.
    /// </summary>
    public class RollbarHangfireOptions
    {
        /// <summary>
        /// Whether to capture job arguments in error reports.
        /// Default: true
        /// </summary>
        public bool CaptureJobArguments { get; set; } = true;

        /// <summary>
        /// Whether to capture performance metrics for jobs.
        /// Default: true
        /// </summary>
        public bool CapturePerformanceMetrics { get; set; } = true;

        /// <summary>
        /// Whether to capture worker information (machine name, process info).
        /// Default: true
        /// </summary>
        public bool CaptureWorkerInformation { get; set; } = true;

        /// <summary>
        /// Job argument field names to scrub for security.
        /// Default: password, token, key, secret, credential
        /// </summary>
        public string[] ScrubFields { get; set; } = { "password", "token", "key", "secret", "credential", "apikey", "api_key" };

        /// <summary>
        /// Minimum error level for job failures.
        /// Default: Error
        /// </summary>
        public ErrorLevel MinimumJobFailureLevel { get; set; } = ErrorLevel.Error;

        /// <summary>
        /// Whether to enable telemetry breadcrumbs for job lifecycle events.
        /// Default: true
        /// </summary>
        public bool EnableTelemetry { get; set; } = true;

        /// <summary>
        /// Whether to capture successful job completions (for monitoring).
        /// Default: false (only capture failures)
        /// </summary>
        public bool CaptureSuccessfulJobs { get; set; } = false;

        /// <summary>
        /// Maximum number of job arguments to capture (to prevent payload bloat).
        /// Default: 20
        /// </summary>
        public int MaxJobArgumentsCount { get; set; } = 20;

        /// <summary>
        /// Maximum length of individual job argument values.
        /// Default: 1000 characters
        /// </summary>
        public int MaxArgumentValueLength { get; set; } = 1000;

        /// <summary>
        /// Whether to capture queue wait time if available.
        /// Default: true
        /// </summary>
        public bool CaptureQueueWaitTime { get; set; } = true;

        /// <summary>
        /// Custom error level for retried jobs.
        /// Default: Warning
        /// </summary>
        public ErrorLevel RetriedJobErrorLevel { get; set; } = ErrorLevel.Warning;

        /// <summary>
        /// Whether to capture job retry information.
        /// Default: true
        /// </summary>
        public bool CaptureRetryInformation { get; set; } = true;
    }
}