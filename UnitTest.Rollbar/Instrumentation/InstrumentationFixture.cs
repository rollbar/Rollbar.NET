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

        /// <summary>
        /// Class PerformanceMonitor.
        /// Implements the <see cref="Rollbar.Instrumentation.IPerformanceMonitor" />
        /// </summary>
        /// <seealso cref="Rollbar.Instrumentation.IPerformanceMonitor" />
        public class PerformanceMonitor
            : IPerformanceMonitor
        {
            /// <summary>
            /// The instance
            /// </summary>
            private static readonly IPerformanceMonitor instance = new PerformanceMonitor();

            /// <summary>
            /// Gets the instance.
            /// </summary>
            /// <value>The instance.</value>
            public static IPerformanceMonitor Instance { get { return instance; } }

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
            /// <param name="classification">The classification.</param>
            public void Capture(TimeSpan measuredTime, IClassification classification)
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
