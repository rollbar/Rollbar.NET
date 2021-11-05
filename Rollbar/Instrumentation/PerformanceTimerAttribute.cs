namespace Rollbar.Instrumentation
{
    using System;
    using System.Diagnostics;
    using Rollbar.Classification;

    /// <summary>
    /// Class CodePerformanceTimer.
    /// Implements the <see cref="System.Attribute" />
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    /// <seealso cref="System.IDisposable" />
    [Conditional(InstrumentationCondition.Instrument)]
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
    [CLSCompliant(false)]
    public class PerformanceTimerAttribute
        : Attribute
        , IDisposable
    {
        /// <summary>
        /// The timer
        /// </summary>
        private readonly Stopwatch? _timer;
        /// <summary>
        /// The performance monitor
        /// </summary>
        private readonly IPerformanceMonitor? _performanceMonitor;
        /// <summary>
        /// The measurement classification
        /// </summary>
        private readonly IClassification? _measurementClassification;

        /// <summary>
        /// Prevents a default instance of the <see cref="PerformanceTimerAttribute"/> class from being created.
        /// </summary>
        private PerformanceTimerAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTimerAttribute"/> class.
        /// </summary>
        /// <param name="performanceMonitor">The performance monitor.</param>
        /// <param name="measurementClassification">The measurement classification.</param>
        private PerformanceTimerAttribute(IPerformanceMonitor performanceMonitor, IClassification? measurementClassification = null)
        {
            this._performanceMonitor = performanceMonitor;
            this._measurementClassification = measurementClassification;
            this._timer = new Stopwatch();
        }

        /// <summary>
        /// Creates new .
        /// </summary>
        /// <param name="performanceMonitor">The performance monitor.</param>
        /// <returns>PerformanceTimerAttribute.</returns>
        public static PerformanceTimerAttribute StartNew(
            IPerformanceMonitor performanceMonitor
            )
        {
            return StartNew(performanceMonitor, null);
        }

        /// <summary>
        /// Creates new .
        /// </summary>
        /// <param name="performanceMonitor">The performance monitor.</param>
        /// <param name="measurementClassification">The measurement classification.</param>
        /// <returns>PerformanceTimerAttribute.</returns>
        public static PerformanceTimerAttribute StartNew(
            IPerformanceMonitor performanceMonitor, 
            IClassification? measurementClassification
            )
        {
            var timer = new PerformanceTimerAttribute(performanceMonitor, measurementClassification);
            timer._timer?.Start();
            return timer;
        }

        #region IDisposable Support

        private bool disposedValue;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1066:Collapsible \"if\" statements should be merged", Justification = "Cleaner alternative.")]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    if (this._timer != null)
                    {
                        this._timer.Stop();

                        this._performanceMonitor?.Capture(this._timer.Elapsed, this._measurementClassification);
                    }
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
