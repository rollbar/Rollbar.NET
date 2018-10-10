namespace Rollbar.Instrumentation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Rollbar.Diagnostics;

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
        /// The measurement classifiers
        /// </summary>
        private readonly IDictionary<string, object> _measurementClassifiers = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTimer"/> class.
        /// </summary>
        /// <param name="performanceMonitor">The performance monitor.</param>
        /// <param name="measurementClassifiers">The measurement classifiers.</param>
        private PerformanceTimer(IPerformanceMonitor performanceMonitor, IDictionary<string, object> measurementClassifiers = null)
        {
            this._performanceMonitor = performanceMonitor;
            this._measurementClassifiers = measurementClassifiers;
            this._timer = new Stopwatch();
        }

        /// <summary>
        /// Creates new .
        /// </summary>
        /// <param name="performanceMonitor">The performance monitor.</param>
        /// <param name="measurementClassifiers">The measurement classifiers.</param>
        /// <returns>CodePerformanceTimer.</returns>
        private static PerformanceTimer StartNew(IPerformanceMonitor performanceMonitor, IDictionary<string, object> measurementClassifiers = null)
        {
            var timer = new PerformanceTimer(performanceMonitor, measurementClassifiers);
            timer._timer.Start();
            return timer;
        }

        /// <summary>
        /// Creates new .
        /// </summary>
        /// <typeparam name="TClassifier">The type of the t classifier.</typeparam>
        /// <param name="performanceMonitor">The performance monitor.</param>
        /// <param name="measurementClassifiers">The measurement classifiers.</param>
        /// <returns>CodePerformanceTimer.</returns>
        public static PerformanceTimer StartNew<TClassifier>(
            IPerformanceMonitor performanceMonitor,
            IDictionary<TClassifier, object> measurementClassifiers = null)
        {
            if (measurementClassifiers == null || measurementClassifiers.Count == 0)
            {
                return PerformanceTimer.StartNew(performanceMonitor, null);
            }

            IDictionary<string, object> stringKeyedClassifiers = new Dictionary<string,object>(measurementClassifiers.Count);

            Type classifierType = typeof(TClassifier);
            if (classifierType.IsEnum)
            {
                foreach(var key in measurementClassifiers.Keys)
                {
                    stringKeyedClassifiers[key.ToString()] = measurementClassifiers[key];
                }
            }
            else if (classifierType == typeof(string))
            {
                foreach (var key in measurementClassifiers.Keys)
                {
                    stringKeyedClassifiers[key as string] = measurementClassifiers[key];
                }
            }
            else if (classifierType.IsPrimitive)
            {
                foreach (var key in measurementClassifiers.Keys)
                {
                    stringKeyedClassifiers[key.ToString()] = measurementClassifiers[key];
                }
            }
            else if (classifierType == typeof(Type))
            {
                foreach (var key in measurementClassifiers.Keys)
                {
                    stringKeyedClassifiers[(key as Type).FullName] = measurementClassifiers[key];
                }
            }
            else
            {
                Assumption.FailValidation("Unexpected classifier/key type", nameof(TClassifier));
            }

            Assumption.AssertTrue(stringKeyedClassifiers.Count > 0, nameof(stringKeyedClassifiers.Count));

            return PerformanceTimer.StartNew(performanceMonitor, stringKeyedClassifiers);
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

            this._performanceMonitor.Capture(this._timer.Elapsed, this._measurementClassifiers);
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
