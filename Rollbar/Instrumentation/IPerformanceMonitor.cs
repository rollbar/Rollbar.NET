namespace Rollbar.Instrumentation
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface IPerformanceMonitor
    /// </summary>
    public interface IPerformanceMonitor
    {
        /// <summary>
        /// Captures the specified measured time.
        /// </summary>
        /// <param name="measuredTime">The measured time.</param>
        /// <param name="measurementClassifiers">The measurement classifiers.</param>
        void Capture(TimeSpan measuredTime, IDictionary<string, object> measurementClassifiers = null);
    }
}
