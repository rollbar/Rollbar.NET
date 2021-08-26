namespace Rollbar.Instrumentation
{
    using Rollbar.Classification;
    using System;

    /// <summary>
    /// Interface IPerformanceMonitor
    /// </summary>
    public interface IPerformanceMonitor
    {
        /// <summary>
        /// Captures the specified measured time.
        /// </summary>
        /// <param name="measuredTime">The measured time.</param>
        void Capture(TimeSpan measuredTime);

        /// <summary>
        /// Captures the specified measured time.
        /// </summary>
        /// <param name="measuredTime">The measured time.</param>
        /// <param name="measurementClassification">The measurement classification.</param>
        void Capture(TimeSpan measuredTime, IClassification? measurementClassification);
    }
}
