#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.Instrumentation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;

    using global::Rollbar;
    using global::Rollbar.Instrumentation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.Http;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(InstrumentationFixture))]
    public class InstrumentationFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
            //SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        [TestMethod]
        public void ConstructionTest()
        {
            using (var timer = InstrumentationHelper.TimeIt(Operation.QueueProcessingLoop, PayloadSize.Medium))
            {
                for(int i = 0; i<10; i++)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(10));
                }
            }
        }

        #region Data mock

        //public enum PerformanceClassifier
        //{
        //    Operation,
        //    PayloadSize,
        //}

        public enum Operation
        {
            AsyncLog,
            SyncLog,
            QueueProcessingLoop,
        }

        public enum PayloadSize
        {
            Small,
            Medium,
            Large,
        }

        public static class InstrumentationHelper
        {
            public static PerformanceTimer TimeIt(Operation operation, PayloadSize payloadSize)
            {
                Dictionary<Type, object> classifiers = new Dictionary<Type, object>() {
                    { typeof(Operation), operation},
                    { typeof(PayloadSize), payloadSize},
                };

                return PerformanceTimer.StartNew(PerformanceMonitor.Instance, classifiers);
            }
        }

        public class PerformanceMonitor
            : IPerformanceMonitor
        {
            private static readonly IPerformanceMonitor instance = new PerformanceMonitor();

            public static IPerformanceMonitor Instance { get { return instance; } }

            public void Capture(TimeSpan measuredTime, IDictionary<string, object> measurementClassifiers = null)
            {
                StringBuilder sb = new StringBuilder("*** T = " + measuredTime.TotalMilliseconds + " [msec]");

                if (measurementClassifiers == null || measurementClassifiers.Count == 0)
                {
                    Trace.WriteLine(sb);
                }

                foreach (var key in measurementClassifiers.Keys)
                {
                    sb.Append(", " + key + " : " + measurementClassifiers[key]);
                }
                Trace.WriteLine(sb);
            }
        }

        #endregion Data mock
    }
}
