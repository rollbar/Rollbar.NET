#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar {
    using System;
    using System.Diagnostics;
    using System.Threading;

    using global::Rollbar;

    using Microsoft.VisualStudio.TestTools.UnitTesting;


    [TestClass]
    [TestCategory(nameof(ConnectivityMonitorFixture))]
    public class ConnectivityMonitorFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        //[TestMethod]
        //public void ManualTest()
        //{
        //    bool lastState = false;
        //    int stateSwitchCount = 0;
        //    var stopWatch = Stopwatch.StartNew();
        //    while (stopWatch.Elapsed < TimeSpan.FromSeconds(90))
        //    {
        //        bool isConnected = ConnectivityMonitor.ConnectivityAvailable;
        //        if (isConnected != lastState)
        //        {
        //            lastState = isConnected;
        //            stateSwitchCount++;
        //        }
        //        Trace.WriteLine($"Network ON: {isConnected}");
        //        Thread.Sleep(TimeSpan.FromMilliseconds(500));
        //    }
        //    stopWatch.Stop();
        //    Assert.IsTrue(stateSwitchCount > 1);
        //}
    }
}
