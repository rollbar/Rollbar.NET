#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using global::Rollbar.DTOs;
    using Exception = System.Exception;

    [TestClass]
    [TestCategory(nameof(RollbarLoggerBlockingWrapperFixture))]
    public class RollbarLoggerBlockingWrapperFixture
    {
        private const int TIME_OUT = 10000;
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
            _logger.Dispose();
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
                _logger.AsBlockingLogger(TimeSpan.FromMilliseconds(TIME_OUT))
                    .Info("Via blocking mechanism.")
                    ;
            }
            catch (TimeoutException ex)
            {
                Assert.Fail("The timeout should be large enough for this test!", ex);
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
                Assert.IsTrue(blockingCallDuration > (3 * noblockingCallDuration), ex.ToString());
                return;
            }

            Assert.Fail("The timeout should be small enough for this test to trigger the TimeoutException!");
        }

        [DataTestMethod]
        [DataRow(ErrorLevel.Critical)]
        [DataRow(ErrorLevel.Error)]
        [DataRow(ErrorLevel.Warning)]
        [DataRow(ErrorLevel.Info)]
        [DataRow(ErrorLevel.Debug)]
        public void ConvenienceMethodsUsesAppropriateErrorLevels(ErrorLevel expectedLogLevel)
        {
            var acctualLogLevel = ErrorLevel.Info;
            void Transform(Payload payload)
            {
                acctualLogLevel = payload.Data.Level.Value;
            }

            var loggerConfig = new RollbarConfig(RollbarUnitTestSettings.AccessToken)
            {
                Environment = RollbarUnitTestSettings.Environment,
                Transform = Transform,

            };
            using (var logger = RollbarFactory.CreateNew().Configure(loggerConfig))
            {
                try
                {
                    var blockingLogger = logger.AsBlockingLogger(TimeSpan.FromMilliseconds(TIME_OUT));
                    var ex = new Exception();
                    switch (expectedLogLevel)
                    {
                        case ErrorLevel.Critical:
                            blockingLogger.Critical(ex);
                            break;
                        case ErrorLevel.Error:
                            blockingLogger.Error(ex);
                            break;
                        case ErrorLevel.Warning:
                            blockingLogger.Warning(ex);
                            break;
                        case ErrorLevel.Info:
                            blockingLogger.Info(ex);
                            break;
                        case ErrorLevel.Debug:
                            blockingLogger.Debug(ex);
                            break;
                    }
                }
                catch
                {
                    Assert.IsTrue(false);
                }
            }

            Assert.AreEqual(expectedLogLevel, acctualLogLevel);
        }

    }
}
