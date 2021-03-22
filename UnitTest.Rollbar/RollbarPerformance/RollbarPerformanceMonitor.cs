namespace UnitTest.Rollbar.RollbarPerformance
{
    using global::Rollbar.Instrumentation;
    using global::Rollbar.Classification;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    /// <summary>
    /// Class RollbarPerformanceMonitor.
    /// Implements the <see cref="Rollbar.Instrumentation.IPerformanceMonitor" />
    /// </summary>
    /// <seealso cref="Rollbar.Instrumentation.IPerformanceMonitor" />
    public class RollbarPerformanceMonitor
        : IPerformanceMonitor
    {
        /// <summary>
        /// The performance log file name
        /// </summary>
        private static readonly string performanceLogFileName = null;
        /// <summary>
        /// The instance
        /// </summary>
        private static volatile RollbarPerformanceMonitor instance = null;
        /// <summary>
        /// The class synchronize root
        /// </summary>
        private static object classSyncRoot = new object();
        /// <summary>
        /// The instance synchronize root
        /// </summary>
        private object _instanceSyncRoot = new object();

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarPerformanceMonitor"/> class from being created.
        /// </summary>
        private RollbarPerformanceMonitor()
        {
        }

        /// <summary>
        /// Initializes static members of the <see cref="RollbarPerformanceMonitor"/> class.
        /// </summary>
        static RollbarPerformanceMonitor()
        {
            RollbarPerformanceMonitor.performanceLogFileName = 
                @"RollbarPerformance-" 
                + DateTimeOffset.Now.ToString().Replace('/', '-').Replace('\\','-').Replace(':','-').Replace(' ', '_') 
                + ".csv"
                ;
            RollbarPerformanceMonitor.performanceLogFileName = 
                Path.Combine(Environment.CurrentDirectory, performanceLogFileName)
                ;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static IPerformanceMonitor Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (classSyncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new RollbarPerformanceMonitor();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Captures the specified measured time.
        /// </summary>
        /// <param name="measuredTime">The measured time.</param>
        public void Capture(TimeSpan measuredTime)
        {
            this.Capture(measuredTime, null);
        }

        /// <summary>
        /// Captures the specified measured time.
        /// </summary>
        /// <param name="measuredTime">The measured time.</param>
        /// <param name="measurementClassification">The measurement classification.</param>
        public void Capture(TimeSpan measuredTime, IClassification measurementClassification)
        {
            lock (classSyncRoot)
            {
                PerformCapture(measuredTime, measurementClassification);
            }
        }

        /// <summary>
        /// Performs the capture.
        /// </summary>
        /// <param name="measuredTime">The measured time.</param>
        /// <param name="measurementClassification">The measurement classification.</param>
        private void PerformCapture(TimeSpan measuredTime, IClassification measurementClassification = null)
        {
            StringBuilder sb = new StringBuilder(measuredTime.TotalMilliseconds.ToString());// + " [msec],");
            foreach (var classifier in measurementClassification.Classifiers)
            {
                sb.Append("," + classifier.ClassifierObject);
            }
            //sb.AppendLine();

            using (StreamWriter file = new StreamWriter(RollbarPerformanceMonitor.performanceLogFileName, true))
            {
                file.WriteLine(sb.ToString());
            }
        }
    }
}
