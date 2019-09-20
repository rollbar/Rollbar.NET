#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarQueueControllerFixture))]
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
            Assert.IsNotNull(RollbarQueueController.Instance);
            Assert.IsInstanceOfType(RollbarQueueController.Instance, typeof(RollbarQueueController));
            Assert.AreSame(RollbarQueueController.Instance, RollbarQueueController.Instance);
        }

        private const int maxQueueRegisterationTestDurationInMillisec = 10 * 1000;
        [TestMethod]
        [Timeout(maxQueueRegisterationTestDurationInMillisec)]
        public void QueueRegisterationTest()
        {
            // we need to make sure we are starting clean:
            while (RollbarQueueController.Instance.GetQueuesCount() > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
            }

            RollbarConfig loggerConfig1 =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
            RollbarConfig loggerConfig2 =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken.Replace('0','1')) { Environment = RollbarUnitTestSettings.Environment, };
            Assert.AreNotEqual(loggerConfig1.AccessToken, loggerConfig2.AccessToken);

            int initialCount = RollbarQueueController.Instance.GetQueuesCount();
            int initialCount1 = RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken);
            int initialCount2 = RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken);

            Assert.AreEqual(initialCount  + 0, RollbarQueueController.Instance.GetQueuesCount());
            Assert.AreEqual(initialCount1 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
            Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));

            using (var logger1 = RollbarFactory.CreateNew().Configure(loggerConfig1))
            {
                Assert.AreEqual(initialCount  + 1, RollbarQueueController.Instance.GetQueuesCount());
                Assert.AreEqual(initialCount1 + 1, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
                Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));

                using (var logger2 = RollbarFactory.CreateNew().Configure(loggerConfig1))
                {
                    Assert.AreEqual(initialCount  + 2, RollbarQueueController.Instance.GetQueuesCount());
                    Assert.AreEqual(initialCount1 + 2, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
                    Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));

                    using (var logger3 = RollbarFactory.CreateNew().Configure(loggerConfig2))
                    {
                        Assert.AreEqual(initialCount  + 3, RollbarQueueController.Instance.GetQueuesCount());
                        Assert.AreEqual(initialCount1 + 2, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
                        Assert.AreEqual(initialCount2 + 1, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));
                    }

                    // an unused queue does not get removed immediately (but eventually) - so let's wait for it for a few processing cycles: 
                    while ((initialCount + 2) != RollbarQueueController.Instance.GetQueuesCount())
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(250));
                    }

                    // if everything is good, we should get here way before this test method times out:
                    Assert.AreEqual(initialCount  + 2, RollbarQueueController.Instance.GetQueuesCount());
                    Assert.AreEqual(initialCount1 + 2, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
                    Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));
                }

                // an unused queue does not get removed immediately (but eventually) - so let's wait for it for a few processing cycles: 
                while ((initialCount + 1) != RollbarQueueController.Instance.GetQueuesCount())
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(250));
                }

                // if everything is good, we should get here way before this test method times out:
                Assert.AreEqual(initialCount  + 1, RollbarQueueController.Instance.GetQueuesCount());
                Assert.AreEqual(initialCount1 + 1, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
                Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));
            }

            // an unused queue does not get removed immediately (but eventually) - so let's wait for it for a few processing cycles: 
            while ((initialCount + 0) != RollbarQueueController.Instance.GetQueuesCount())
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
            }

            // if everything is good, we should get here way before this test method times out:
            Assert.AreEqual(initialCount  + 0, RollbarQueueController.Instance.GetQueuesCount());
            Assert.AreEqual(initialCount1 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
            Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));
        }

        [TestMethod]
        public void ReconfigureRollbarTest()
        {
            RollbarConfig loggerConfig1 =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
            RollbarConfig loggerConfig2 =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken.Replace('0', '1')) { Environment = RollbarUnitTestSettings.Environment, };
            Assert.AreNotEqual(loggerConfig1.AccessToken, loggerConfig2.AccessToken);

            int initialCount = RollbarQueueController.Instance.GetQueuesCount();
            int initialCount1 = RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken);
            int initialCount2 = RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken);

            Assert.AreEqual(initialCount  + 0, RollbarQueueController.Instance.GetQueuesCount());
            Assert.AreEqual(initialCount1 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
            Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));

            using (var logger1 = RollbarFactory.CreateNew().Configure(loggerConfig1))
            {
                Assert.AreEqual(initialCount  + 1, RollbarQueueController.Instance.GetQueuesCount());
                Assert.AreEqual(initialCount1 + 1, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
                Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));

                logger1.Configure(loggerConfig2);

                Assert.AreEqual(initialCount  + 1, RollbarQueueController.Instance.GetQueuesCount());
                Assert.AreEqual(initialCount1 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
                Assert.AreEqual(initialCount2 + 1, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));
            }
        }

    }
}
