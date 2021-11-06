#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Infrastructure;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;

    using UnitTest.RollbarTestCommon;

    [TestClass]
    [TestCategory(nameof(RollbarQueueControllerFixture))]
    public class RollbarQueueControllerFixture
    {

        [TestInitialize]
        public void SetupFixture()
        {
            RollbarUnitTestEnvironmentUtil.SetupLiveTestRollbarInfrastructure();
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

        private const int maxQueueRegisterationTestDurationInMillisec = 60 * 1000;
        [TestMethod]
        [Timeout(maxQueueRegisterationTestDurationInMillisec)]
        public void QueueRegisterationTest()
        {
            Console.WriteLine($"Unrealeased queues count: {RollbarQueueController.Instance.GetUnReleasedQueuesCount()}");

            // we need to make sure we are starting clean:
            RollbarQueueController.Instance.FlushQueues();
            RollbarQueueController.Instance.Stop(true);
            RollbarQueueController.Instance.Start();
            //while (RollbarQueueController.Instance.GetQueuesCount() > 0)
            //{
            //    Thread.Sleep(TimeSpan.FromMilliseconds(250));
            //}
            Console.WriteLine($"Unreleased queues count: {RollbarQueueController.Instance.GetUnReleasedQueuesCount()}");

            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(
                    RollbarUnitTestSettings.AccessToken,
                    RollbarUnitTestSettings.Environment
                    );
            RollbarLoggerConfig loggerConfig1 =
                new RollbarLoggerConfig();
            loggerConfig1.RollbarDestinationOptions.Reconfigure(destinationOptions);

            destinationOptions.AccessToken =
                RollbarUnitTestSettings.AccessToken.Replace('0', '1');
            RollbarLoggerConfig loggerConfig2 =
                new RollbarLoggerConfig();
            loggerConfig2.RollbarDestinationOptions.Reconfigure(destinationOptions);

            Console.WriteLine($"Unreleased queues count: {RollbarQueueController.Instance.GetUnReleasedQueuesCount()}");

            Assert.AreNotEqual(loggerConfig1.RollbarDestinationOptions.AccessToken, loggerConfig2.RollbarDestinationOptions.AccessToken);

            int initialCount = RollbarQueueController.Instance.GetQueuesCount();
            int initialCount1 = RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken);
            int initialCount2 = RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken);

            Assert.AreEqual(initialCount + 0, RollbarQueueController.Instance.GetQueuesCount());
            Assert.AreEqual(initialCount1 + 0,
                RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken));
            Assert.AreEqual(initialCount2 + 0,
                RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken));

            using (var logger1 = RollbarFactory.CreateNew().Configure(loggerConfig1))
            {
                Assert.AreEqual(initialCount + 1, RollbarQueueController.Instance.GetQueuesCount());
                Assert.AreEqual(initialCount1 + 1,
                    RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken));
                Assert.AreEqual(initialCount2 + 0,
                    RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken));

                using (var logger2 = RollbarFactory.CreateNew().Configure(loggerConfig1))
                {
                    Assert.AreEqual(initialCount + 2, RollbarQueueController.Instance.GetQueuesCount());
                    Assert.AreEqual(initialCount1 + 2,
                        RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken));
                    Assert.AreEqual(initialCount2 + 0,
                        RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken));

                    using (var logger3 = RollbarFactory.CreateNew().Configure(loggerConfig2))
                    {
                        Assert.AreEqual(initialCount + 3, RollbarQueueController.Instance.GetQueuesCount());
                        Assert.AreEqual(initialCount1 + 2,
                            RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken));
                        Assert.AreEqual(initialCount2 + 1,
                            RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken));
                    }

                    // an unused queue does not get removed immediately (but eventually) - so let's wait for it for a few processing cycles: 
                    while ((initialCount + 2) != RollbarQueueController.Instance.GetQueuesCount())
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(250));
                    }

                    // if everything is good, we should get here way before this test method times out:
                    Assert.AreEqual(initialCount + 2, RollbarQueueController.Instance.GetQueuesCount());
                    Assert.AreEqual(initialCount1 + 2,
                        RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken));
                    Assert.AreEqual(initialCount2 + 0,
                        RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken));
                }

                // an unused queue does not get removed immediately (but eventually) - so let's wait for it for a few processing cycles: 
                while ((initialCount + 1) != RollbarQueueController.Instance.GetQueuesCount())
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(250));
                }

                // if everything is good, we should get here way before this test method times out:
                Assert.AreEqual(initialCount + 1, RollbarQueueController.Instance.GetQueuesCount());
                Assert.AreEqual(initialCount1 + 1,
                    RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken));
                Assert.AreEqual(initialCount2 + 0,
                    RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken));
            }

            // an unused queue does not get removed immediately (but eventually) - so let's wait for it for a few processing cycles: 
            while ((initialCount + 0) != RollbarQueueController.Instance.GetQueuesCount())
            {
                Console.WriteLine($"Unreleased queues count: {RollbarQueueController.Instance.GetUnReleasedQueuesCount()}");
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
            }


            // if everything is good, we should get here way before this test method times out:
            Assert.AreEqual(initialCount + 0, RollbarQueueController.Instance.GetQueuesCount());
            Assert.AreEqual(initialCount1 + 0,
                RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken));
            Assert.AreEqual(initialCount2 + 0,
                RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken));
        }

        [TestMethod]
        public void ReconfigureRollbarTest()
        {
            RollbarDestinationOptions destinationOptions = 
                new RollbarDestinationOptions(
                    RollbarUnitTestSettings.AccessToken, 
                    RollbarUnitTestSettings.Environment
                    );
            RollbarLoggerConfig loggerConfig1 =
                new RollbarLoggerConfig();
            loggerConfig1.RollbarDestinationOptions.Reconfigure(destinationOptions);

            destinationOptions.AccessToken = 
                RollbarUnitTestSettings.AccessToken.Replace('0', '1');
            RollbarLoggerConfig loggerConfig2 =
                new RollbarLoggerConfig();
            loggerConfig2.RollbarDestinationOptions.Reconfigure(destinationOptions);

            Assert.AreNotEqual(loggerConfig1.RollbarDestinationOptions.AccessToken, loggerConfig2.RollbarDestinationOptions.AccessToken);

            int initialCount = RollbarQueueController.Instance.GetQueuesCount();
            int initialCount1 = RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken);
            int initialCount2 = RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken);

            Assert.AreEqual(initialCount  + 0, RollbarQueueController.Instance.GetQueuesCount());
            Assert.AreEqual(initialCount1 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken));
            Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken));

            using (var logger1 = RollbarFactory.CreateNew().Configure(loggerConfig1))
            {
                Assert.AreEqual(initialCount  + 1, RollbarQueueController.Instance.GetQueuesCount());
                Assert.AreEqual(initialCount1 + 1, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken));
                Assert.AreEqual(initialCount2 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken));

                logger1.Configure(loggerConfig2);

                Assert.AreEqual(initialCount  + 1, RollbarQueueController.Instance.GetQueuesCount());
                Assert.AreEqual(initialCount1 + 0, RollbarQueueController.Instance.GetQueuesCount(loggerConfig1.RollbarDestinationOptions.AccessToken));
                Assert.AreEqual(initialCount2 + 1, RollbarQueueController.Instance.GetQueuesCount(loggerConfig2.RollbarDestinationOptions.AccessToken));
            }
        }

    }
}
