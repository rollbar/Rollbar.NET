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
    public class PerformanceTimer
        : Attribute
        , IDisposable
    {
        /// <summary>
        /// The timer
        /// </summary>
        private readonly Stopwatch _timer = null;
        /// <summary>
        /// The performance monitor
        /// </summary>
        private readonly IPerformanceMonitor _performanceMonitor = null;
        /// <summary>
        /// The measurement classification
        /// </summary>
        private readonly IClassification _measurementClassification = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTimer"/> class.
        /// </summary>
        /// <param name="performanceMonitor">The performance monitor.</param>
        /// <param name="measurementClassification">The measurement classification.</param>
        private PerformanceTimer(IPerformanceMonitor performanceMonitor, IClassification measurementClassification = null)
        {
            this._performanceMonitor = performanceMonitor;
            this._measurementClassification = measurementClassification;
            this._timer = new Stopwatch();
        }

        /// <summary>
        /// Creates new .
        /// </summary>
        /// <param name="performanceMonitor">The performance monitor.</param>
        /// <param name="measurementClassification">The measurement classification.</param>
        /// <returns>PerformanceTimer.</returns>
        public static PerformanceTimer StartNew(IPerformanceMonitor performanceMonitor, IClassification measurementClassification = null)
        {
            var timer = new PerformanceTimer(performanceMonitor, measurementClassification);
            timer._timer.Start();
            return timer;
        }

        #region IDisposable Support

        // For the sake of performance and simplicity we are not implementing 
        // the IDisposable the canonical way:

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this._timer.Stop();

            this._performanceMonitor.Capture(this._timer.Elapsed, this._measurementClassification);
        }

        //private bool disposedValue = false; // To detect redundant calls

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //            // TODO: dispose managed state (managed objects).
        //        }

        //        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        //        // TODO: set large fields to null.

        //        disposedValue = true;
        //    }
        //}

        //// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        //// ~CodePerformanceTimer() {
        ////   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        ////   Dispose(false);
        //// }

        //// This code added to correctly implement the disposable pattern.
        //public void Dispose()
        //{
        //    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //    Dispose(true);
        //    // TODO: uncomment the following line if the finalizer is overridden above.
        //    // GC.SuppressFinalize(this);
        //}

        #endregion
    }
}
