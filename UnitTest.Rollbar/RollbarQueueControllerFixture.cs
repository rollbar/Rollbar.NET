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
            Assert.IsNotNull(RollbarQueueController.Instance);
            Assert.IsInstanceOfType(RollbarQueueController.Instance, typeof(RollbarQueueController));
            Assert.AreSame(RollbarQueueController.Instance, RollbarQueueController.Instance);
        }

        [TestMethod]
        public void QueueRegisteration()
        {
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

                    Assert.AreEqual(initialCount  + 2, RollbarQueueController.Instance.GetQueuesCount());
                    Assert.AreEqual(initialCount1 + 2, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
                    Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));
                }

                Assert.AreEqual(initialCount  + 1, RollbarQueueController.Instance.GetQueuesCount());
                Assert.AreEqual(initialCount1 + 1, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.AccessToken));
                Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.AccessToken));
            }

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
