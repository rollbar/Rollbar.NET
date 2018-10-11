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
    using global::Rollbar.Classification;

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
        public void BasicStructuralTest()
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
                IClassification classification = Classification.MatchClassification(operation, payloadSize);
                return PerformanceTimer.StartNew(PerformanceMonitor.Instance, classification);
            }
        }

        public class PerformanceMonitor
            : IPerformanceMonitor
        {
            private static readonly IPerformanceMonitor instance = new PerformanceMonitor();

            public static IPerformanceMonitor Instance { get { return instance; } }

            public void Capture(TimeSpan measuredTime, IClassification classification = null)
            {
                StringBuilder sb = new StringBuilder("*** T = " + measuredTime.TotalMilliseconds + " [msec]");

                if (classification == null || classification.ClassifiersCount == 0)
                {
                    Trace.WriteLine(sb);
                }

                foreach (var classifier in classification.Classifiers)
                {
                    sb.Append(", " + classifier.ClassifierType.Name + " : " + classifier.ClassifierObject);
                }
                Trace.WriteLine(sb);
            }
        }

        #endregion Data mock
    }
}
