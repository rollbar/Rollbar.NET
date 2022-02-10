#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar {

    using global::Rollbar.Infrastructure;

    using Microsoft.VisualStudio.TestTools.UnitTesting;


    [TestClass]
    [TestCategory(nameof(RollbarConnectivityMonitorFixture))]
    public class RollbarConnectivityMonitorFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void TestApiServerTest()
        {
            bool isConnected = RollbarConnectivityMonitor.TestApiServer();
            Assert.IsTrue(isConnected, "Access to api.rollbar.com:443");
        }


        //[TestMethod]
        //public void ManualTest() {
        //bool lastState = false;
        //int stateSwitchCount = 0;
        //var stopWatch = Stopwatch.StartNew();
        //while(stopWatch.Elapsed < TimeSpan.FromSeconds(60)) {
        //var sw = Stopwatch.StartNew();
        //bool isConnected = ConnectivityMonitor.TestApiServer();
        ////bool isConnected = ConnectivityMonitor.Instance.IsConnectivityOn;
        //sw.Stop();
        //Console.WriteLine("State check took " + sw.Elapsed);
        //if(isConnected != lastState) {
        //lastState = isConnected;
        //stateSwitchCount++;
        //}
        //Console.WriteLine($"Network ON: {isConnected}");
        //Thread.Sleep(TimeSpan.FromMilliseconds(500));
        //}
        //stopWatch.Stop();
        //Console.WriteLine("stateSwitchCount = " + stateSwitchCount);
        //Assert.IsTrue(stateSwitchCount > 1);
        //}

    }
}
