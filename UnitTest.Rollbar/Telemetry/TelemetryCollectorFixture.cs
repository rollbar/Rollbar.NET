#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.Telemetry
{
    using global::Rollbar.Telemetry;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory(nameof(TelemetryCollectorFixture))]
    public class TelemetryCollectorFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        [TestMethod]
        [Timeout(150000)]
        public void BasicTest()
        {
            Assert.IsTrue(TelemetryCollector.Instance.IsAutocollecting);

            var config = TelemetryCollector.Instance.Config;

            Thread.Sleep(TimeSpan.FromMilliseconds(config.TelemetryCollectionInterval.TotalMilliseconds * config.TelemetryQueueDepth));
            Assert.AreEqual(config.TelemetryQueueDepth, TelemetryCollector.Instance.TelemetryQueue.GetItemsCount());
            foreach (var item in TelemetryCollector.Instance.TelemetryQueue.GetQueueContent())
            {
                Console.WriteLine(item);
            }

            TelemetryCollector.Instance.StopAutocollection(true);
            Assert.IsTrue(!TelemetryCollector.Instance.IsAutocollecting);
            config.TelemetryQueueDepth = 100;
            TelemetryCollector.Instance.StartAutocollection();
            Assert.IsTrue(TelemetryCollector.Instance.IsAutocollecting);

            Thread.Sleep(TimeSpan.FromMilliseconds(config.TelemetryCollectionInterval.TotalMilliseconds * config.TelemetryQueueDepth));
            Assert.AreEqual(config.TelemetryQueueDepth, TelemetryCollector.Instance.TelemetryQueue.GetItemsCount());
            foreach (var item in TelemetryCollector.Instance.TelemetryQueue.GetQueueContent())
            {
                Console.WriteLine(item);
            }


        }

    }
}
