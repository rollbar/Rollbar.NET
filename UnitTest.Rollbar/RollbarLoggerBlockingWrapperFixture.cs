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
    using UnitTest.RollbarTestCommon;
    using global::Rollbar.Infrastructure;

    [TestClass]
    [TestCategory(nameof(RollbarLoggerBlockingWrapperFixture))]
    public class RollbarLoggerBlockingWrapperFixture
    {
        private const int TIME_OUT = 10000;
        private IRollbar _logger = null;

        [TestInitialize]
        public void SetupFixture()
        {
            RollbarUnitTestEnvironmentUtil.SetupLiveTestRollbarInfrastructure();

            RollbarQueueController.Instance.FlushQueues();
            //RollbarQueueController.Instance.InternalEvent += Instance_InternalEvent;

            _logger = RollbarFactory.CreateNew().Configure(RollbarInfrastructure.Instance.Config.RollbarLoggerConfig);
        }

        [TestCleanup]
        public void TearDownFixture()
        {
            _logger.Dispose();

            // let's make sure we clean up all at the end:
            RollbarQueueController.Instance.FlushQueues();
            RollbarQueueController.Instance.InternalEvent += Instance_InternalEvent;
        }

        private void Instance_InternalEvent(object sender, RollbarEventArgs e)
        {
            string eventTrace = $"##################{Environment.NewLine}{e.TraceAsString()}{Environment.NewLine}";
            Console.WriteLine(eventTrace);
            System.Diagnostics.Trace.WriteLine(eventTrace);

            //CommunicationEventArgs communicationEventArgs = e as CommunicationEventArgs;
            //if (communicationEventArgs != null)
            //{
            //    this._rollbarCommEvents.Add(communicationEventArgs);
            //}

            //CommunicationErrorEventArgs communicationErrorEventArgs = e as CommunicationErrorEventArgs;
            //if (communicationErrorEventArgs != null)
            //{
            //    this._rollbarCommErrorEvents.Add(communicationErrorEventArgs);
            //}

        }


        [Ignore]
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

        [TestMethod]
        public void TimeoutExceptionAggregatesMisconfigurationDetails()
        {
            RollbarQueueController.Instance.InternalEvent += Instance_InternalEvent;

            RollbarLoggerConfig badConfig = new RollbarLoggerConfig("MISCONFIGURED_TOKEN"); // this is clearly wrong token...
            using (IRollbar logger = RollbarFactory.CreateNew(badConfig))
            {
                try
                {
                    var badData = new Data(new Body("Misconfigured Person data"));
                    badData.Person = new Person();
                    logger.AsBlockingLogger(TimeSpan.FromSeconds(3)).Log(badData);
                    Assert.Fail("No TimeoutException!");
                }
                catch (TimeoutException ex)
                {
                    Assert.AreEqual("Posting a payload to the Rollbar API Service timed-out", ex.Message);
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsTrue(ex.InnerException is AggregateException);
                    Assert.IsTrue((ex.InnerException as AggregateException).InnerExceptions.Count > 0);
                }
            }
        }

        [Ignore]
        [DataTestMethod]
        [DataRow(ErrorLevel.Critical)]
        [DataRow(ErrorLevel.Error)]
        [DataRow(ErrorLevel.Warning)]
        [DataRow(ErrorLevel.Info)]
        [DataRow(ErrorLevel.Debug)]
        public void ConvenienceMethodsUseAppropriateErrorLevels(ErrorLevel expectedLogLevel)
        {
            var acctualLogLevel = ErrorLevel.Info;
            void Transform(Payload payload)
            {
                acctualLogLevel = payload.Data.Level.Value;
            }

            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(
                    RollbarUnitTestSettings.AccessToken,
                    RollbarUnitTestSettings.Environment
                    );
            RollbarPayloadManipulationOptions payloadManipulationOptions =
                new RollbarPayloadManipulationOptions(Transform);
            var loggerConfig = new RollbarLoggerConfig();
            loggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);
            loggerConfig.RollbarPayloadManipulationOptions.Reconfigure(payloadManipulationOptions);
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
                    Assert.Fail();
                }
            }

            Assert.AreEqual(expectedLogLevel, acctualLogLevel);
        }

    }
}
