namespace UnitTest.Rollbar.RollbarPerformance
{
    using global::Rollbar.Instrumentation;
    using global::Rollbar.Classification;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    public class RollbarPerformanceMonitor
        : IPerformanceMonitor
    {
        private static readonly string performanceLogFileName = null;
        private static volatile RollbarPerformanceMonitor instance = null;
        private static object classSyncRoot = new object();
        private object _instanceSyncRoot = new object();

        private RollbarPerformanceMonitor()
        {
        }

        static RollbarPerformanceMonitor()
        {
            RollbarPerformanceMonitor.performanceLogFileName = @"RollbarPerformance-" + DateTimeOffset.Now.ToString().Replace(':','-') + ".csv";
            RollbarPerformanceMonitor.performanceLogFileName = Path.Combine(Environment.CurrentDirectory, performanceLogFileName);
        }

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

        public void Capture(TimeSpan measuredTime, IClassification measurementClassification = null)
        {
            lock (classSyncRoot)
            {
                PerformCapture(measuredTime, measurementClassification);
            }
        }

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
