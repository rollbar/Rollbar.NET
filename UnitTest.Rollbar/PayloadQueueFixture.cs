namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading;

    [TestClass]
    [TestCategory("PayloadQueueFixture")]
    public class PayloadQueueFixture
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
        public void ConstructionTest()
        {
            using (var logger = new RollbarLogger(false))
            {
                PayloadQueue pq = new PayloadQueue(logger);
                Assert.IsNotNull(pq.Logger);
                Assert.AreSame(logger, pq.Logger);
            }
        }
    }
}
