using System;
using System.Collections.Generic;
using System.Linq;
using Rollbar;

namespace Rollbar.Hangfire
{
    /// <summary>
    /// Attribute for decorating Hangfire jobs with specific Rollbar configuration.
    /// This allows per-job customization of error tracking behavior.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RollbarJobAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the error level for this job.
        /// If not specified, uses the global configuration.
        /// </summary>
        public ErrorLevel? ErrorLevel { get; set; }

        /// <summary>
        /// Gets or sets whether to capture job arguments for this specific job.
        /// If not specified, uses the global configuration.
        /// </summary>
        public bool? CaptureArguments { get; set; }

        /// <summary>
        /// Gets or sets whether to capture performance metrics for this specific job.
        /// If not specified, uses the global configuration.
        /// </summary>
        public bool? CapturePerformanceMetrics { get; set; }

        /// <summary>
        /// Gets or sets custom tags for this job.
        /// These will be added to the Rollbar payload for easier filtering and searching.
        /// </summary>
        public string? Tags { get; set; }

        /// <summary>
        /// Gets or sets a custom fingerprint for this job.
        /// Useful for grouping related job failures together.
        /// </summary>
        public string? Fingerprint { get; set; }

        /// <summary>
        /// Gets or sets whether this job is considered critical.
        /// Critical jobs may use different error levels or notification rules.
        /// </summary>
        public bool IsCritical { get; set; } = false;

        /// <summary>
        /// Gets or sets a custom description for this job.
        /// This will be included in error reports to provide context.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether to disable Rollbar tracking for this specific job.
        /// Useful for jobs that should not be monitored (e.g., test jobs).
        /// </summary>
        public bool DisableTracking { get; set; } = false;

        /// <summary>
        /// Gets or sets the retry error level for this job.
        /// Used when the job fails and is being retried.
        /// </summary>
        public ErrorLevel? RetryErrorLevel { get; set; }

        /// <summary>
        /// Initializes a new instance of the RollbarJobAttribute class.
        /// </summary>
        public RollbarJobAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RollbarJobAttribute class with a specific error level.
        /// </summary>
        /// <param name="errorLevel">The error level for this job</param>
        public RollbarJobAttribute(ErrorLevel errorLevel)
        {
            ErrorLevel = errorLevel;
        }

        /// <summary>
        /// Initializes a new instance of the RollbarJobAttribute class with tags.
        /// </summary>
        /// <param name="tags">Comma-separated tags for this job</param>
        public RollbarJobAttribute(string tags)
        {
            Tags = tags;
        }

        /// <summary>
        /// Initializes a new instance of the RollbarJobAttribute class with error level and tags.
        /// </summary>
        /// <param name="errorLevel">The error level for this job</param>
        /// <param name="tags">Comma-separated tags for this job</param>
        public RollbarJobAttribute(ErrorLevel errorLevel, string tags)
        {
            ErrorLevel = errorLevel;
            Tags = tags;
        }

        /// <summary>
        /// Gets the tags as an array.
        /// </summary>
        /// <returns>Array of tag strings, or empty array if no tags</returns>
        public string[] GetTagsArray()
        {
            if (string.IsNullOrWhiteSpace(Tags))
                return new string[0];

            return Tags!.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(tag => tag.Trim())
                      .Where(tag => !string.IsNullOrEmpty(tag))
                      .ToArray();
        }

        /// <summary>
        /// Creates a custom data dictionary with the attribute information.
        /// </summary>
        /// <returns>Dictionary containing attribute metadata</returns>
        public IDictionary<string, object> ToCustomData()
        {
            var customData = new Dictionary<string, object>();

            if (ErrorLevel.HasValue)
                customData["configuredErrorLevel"] = ErrorLevel.Value.ToString();

            if (CaptureArguments.HasValue)
                customData["captureArguments"] = CaptureArguments.Value;

            if (CapturePerformanceMetrics.HasValue)
                customData["capturePerformanceMetrics"] = CapturePerformanceMetrics.Value;

            if (!string.IsNullOrWhiteSpace(Tags))
                customData["tags"] = GetTagsArray();

            if (!string.IsNullOrWhiteSpace(Fingerprint))
                customData["customFingerprint"] = Fingerprint!;

            if (IsCritical)
                customData["isCritical"] = true;

            if (!string.IsNullOrWhiteSpace(Description))
                customData["jobDescription"] = Description!;

            if (DisableTracking)
                customData["trackingDisabled"] = true;

            if (RetryErrorLevel.HasValue)
                customData["retryErrorLevel"] = RetryErrorLevel.Value.ToString();

            return customData;
        }
    }

    /// <summary>
    /// Helper class for working with RollbarJobAttribute in filters.
    /// </summary>
    public static class RollbarJobAttributeHelper
    {
        /// <summary>
        /// Gets the RollbarJobAttribute from a job method, if present.
        /// </summary>
        /// <param name="job">The Hangfire job</param>
        /// <returns>The RollbarJobAttribute if found, null otherwise</returns>
        public static RollbarJobAttribute? GetJobAttribute(global::Hangfire.Common.Job job)
        {
            if (job?.Method == null)
                return null;

            // Check method first
            var methodAttribute = job.Method.GetCustomAttributes(typeof(RollbarJobAttribute), false)
                                           .OfType<RollbarJobAttribute>()
                                           .FirstOrDefault();

            if (methodAttribute != null)
                return methodAttribute;

            // Check declaring type
            var typeAttribute = job.Type?.GetCustomAttributes(typeof(RollbarJobAttribute), false)
                                        .OfType<RollbarJobAttribute>()
                                        .FirstOrDefault();

            return typeAttribute;
        }

        /// <summary>
        /// Merges attribute configuration with global options.
        /// Attribute values take precedence over global options.
        /// </summary>
        /// <param name="globalOptions">Global Rollbar Hangfire options</param>
        /// <param name="attribute">Job-specific attribute (can be null)</param>
        /// <returns>Effective configuration for the job</returns>
        public static EffectiveJobConfiguration MergeConfiguration(RollbarHangfireOptions globalOptions, RollbarJobAttribute? attribute)
        {
            return new EffectiveJobConfiguration
            {
                CaptureArguments = attribute?.CaptureArguments ?? globalOptions.CaptureJobArguments,
                CapturePerformanceMetrics = attribute?.CapturePerformanceMetrics ?? globalOptions.CapturePerformanceMetrics,
                ErrorLevel = attribute?.ErrorLevel ?? globalOptions.MinimumJobFailureLevel,
                RetryErrorLevel = attribute?.RetryErrorLevel ?? globalOptions.RetriedJobErrorLevel,
                Tags = attribute?.GetTagsArray() ?? new string[0],
                Fingerprint = attribute?.Fingerprint,
                IsCritical = attribute?.IsCritical ?? false,
                Description = attribute?.Description,
                DisableTracking = attribute?.DisableTracking ?? false,
                ScrubFields = globalOptions.ScrubFields,
                MaxArgumentsCount = globalOptions.MaxJobArgumentsCount,
                MaxArgumentValueLength = globalOptions.MaxArgumentValueLength
            };
        }
    }

    /// <summary>
    /// Represents the effective configuration for a job after merging global options with attribute settings.
    /// </summary>
    public class EffectiveJobConfiguration
    {
        public bool CaptureArguments { get; set; }
        public bool CapturePerformanceMetrics { get; set; }
        public ErrorLevel ErrorLevel { get; set; }
        public ErrorLevel RetryErrorLevel { get; set; }
        public string[] Tags { get; set; } = new string[0];
        public string? Fingerprint { get; set; }
        public bool IsCritical { get; set; }
        public string? Description { get; set; }
        public bool DisableTracking { get; set; }
        public string[] ScrubFields { get; set; } = new string[0];
        public int MaxArgumentsCount { get; set; }
        public int MaxArgumentValueLength { get; set; }
    }
}