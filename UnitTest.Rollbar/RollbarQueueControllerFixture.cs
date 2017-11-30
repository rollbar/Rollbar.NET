namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;

    [TestClass]
    [TestCategory("RollbarQueueControllerFixture")]
    public class RollbarQueueControllerFixture
    {
        //private IRollbar _logger = null;

        [TestInitialize]
        public void SetupFixture()
        {
            //SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            //RollbarConfig loggerConfig =
            //    new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
            //_logger = RollbarFactory.CreateNew().Configure(loggerConfig);
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        [TestMethod]
        public void SingletonTest()
        {
            //TODO:
            Assert.Fail();
        }

        [TestMethod]
        public void RegisterTest()
        {
            //TODO:
            Assert.Fail();
        }

        [TestMethod]
        public void UnregisterTest()
        {
            //TODO:
            Assert.Fail();
        }

        [TestMethod]
        public void ReconfigureRollbarTest()
        {
            //TODO:
            Assert.Fail();
        }

    }
}
