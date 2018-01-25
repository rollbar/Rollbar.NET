#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarLoggerBlockingWrapperFixture))]
    public class RollbarLoggerBlockingWrapperFixture
    {
        private IRollbar _logger = null;

        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            RollbarConfig loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
            _logger = RollbarFactory.CreateNew().Configure(loggerConfig);
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        [TestMethod]
        public void HasBlockingBehavior()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            _logger.Info("Via no-blocking mechanism.");
            stopwatch.Stop();
            var noblockingCallDuration = stopwatch.Elapsed.TotalMilliseconds;

            stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.AsBlockingLogger(TimeSpan.FromMilliseconds(10000))
                    .Info("Via blocking mechanism.")
                    ;
            }
            catch (System.TimeoutException ex)
            {
                Assert.Fail("The timeout should be large enough for this test!");
            }
            stopwatch.Stop();
            var blockingCallDuration = stopwatch.Elapsed.TotalMilliseconds;

            Assert.IsTrue(blockingCallDuration > (10 * noblockingCallDuration));
        }

        [TestMethod]
        public void TimeoutWorks()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            _logger.Info("Via no-blocking mechanism.");
            stopwatch.Stop();
            var noblockingCallDuration = stopwatch.Elapsed.TotalMilliseconds;

            stopwatch = Stopwatch.StartNew();
            try
            {
                // the timeout has to be significantly larger than no-blocking call duration 
                // but smaller than time needed to succeed a blocking call:
                _logger.AsBlockingLogger(TimeSpan.FromMilliseconds(100))
                    .Info("Via blocking mechanism.")
                    ;
            }
            catch (System.TimeoutException ex)
            {
                stopwatch.Stop();
                var blockingCallDuration = stopwatch.Elapsed.TotalMilliseconds;
                Assert.IsTrue(blockingCallDuration > (3 * noblockingCallDuration)); 
                return;
            }

            Assert.Fail("The timeout should be small enough for this test to trigger the TimeoutException!");
        }

    }
}
