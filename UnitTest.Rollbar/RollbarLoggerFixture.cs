#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Rollbar.DTOs;
    using Exception = System.Exception;
    using System.Diagnostics;

    [TestClass]
    [TestCategory(nameof(RollbarLoggerFixture))]
    public class RollbarLoggerFixture
        : RollbarLiveFixtureBase
    {
        RollbarConfig _loggerConfig;

        [TestInitialize]
        public override void SetupFixture()
        {
            base.SetupFixture();

            this._loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
        }

        [TestCleanup]
        public override void TearDownFixture()
        {
            base.TearDownFixture();
        }

        [TestMethod]
        public void AllowsProxySettingsReconfiguration()
        {
            using (IRollbar logger = RollbarFactory.CreateNew().Configure(this._loggerConfig))
            {
                Assert.AreNotSame(this._loggerConfig, logger.Config);
                logger.Configure(this._loggerConfig);
                Assert.AreNotSame(this._loggerConfig, logger.Config);

                int errorCount = 0;
                logger.AsBlockingLogger(TimeSpan.FromSeconds(3)).Info("test 1");
                this.ExpectedCommunicationEventsTotal++;
                Assert.AreEqual(0, errorCount);

                RollbarConfig newConfig = new RollbarConfig("seed");
                newConfig.Reconfigure(this._loggerConfig);
                Assert.AreNotSame(this._loggerConfig, newConfig);
                logger.Configure(newConfig);
                logger.AsBlockingLogger(TimeSpan.FromSeconds(3)).Info("test 2");
                this.ExpectedCommunicationEventsTotal++;
                Assert.AreEqual(0, errorCount);

                newConfig.ProxyAddress = "www.fakeproxy.com";
                newConfig.ProxyUsername = "fakeusername";
                newConfig.ProxyPassword = "fakepassword";
                logger.Configure(newConfig);
                Assert.IsFalse(string.IsNullOrEmpty(logger.Config.ProxyAddress));
                Assert.IsFalse(string.IsNullOrEmpty(logger.Config.ProxyUsername));
                Assert.IsFalse(string.IsNullOrEmpty(logger.Config.ProxyPassword));
                try
                {
                    // the fake proxy settings will cause a timeout exception here:
                    this.ExpectedCommunicationErrorsTotal++;
                    logger.AsBlockingLogger(TimeSpan.FromSeconds(3)).Info("test 3 with fake proxy");
                }
                catch
                {
                    errorCount++;
                }
                Assert.AreEqual(1, errorCount);

                newConfig.ProxyAddress = null;
                newConfig.ProxyUsername = null;
                newConfig.ProxyPassword = null;
                logger.Configure(newConfig);
                Assert.IsTrue(string.IsNullOrEmpty(logger.Config.ProxyAddress));
                Assert.IsTrue(string.IsNullOrEmpty(logger.Config.ProxyUsername));
                Assert.IsTrue(string.IsNullOrEmpty(logger.Config.ProxyPassword));
                try
                {
                    // the fake proxy settings are gone, so, next call is expected to succeed:
                    this.ExpectedCommunicationEventsTotal++;
                    logger.AsBlockingLogger(TimeSpan.FromSeconds(15)).Info("test 4");
                }
                catch
                {
                    errorCount++;
                }
                Assert.AreEqual(1, errorCount);
            }
        }

        [TestMethod]
        public void ImplementsIDisposable()
        {
            using (IRollbar logger = RollbarFactory.CreateNew().Configure(this._loggerConfig))
            {
                IDisposable disposable = logger as IDisposable;
                Assert.IsNotNull(disposable);
            }
        }

        private const int maxScopedInstanceTestDurationInMillisec = 50 * 1000;
        [TestMethod]
        [Timeout(maxScopedInstanceTestDurationInMillisec)]
        public void ScopedInstanceTest()
        {
            // we need to make sure we are starting clean:
            while (RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken) > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
            }

            RollbarConfig loggerConfig = new RollbarConfig(RollbarUnitTestSettings.AccessToken)
            {
                Environment = RollbarUnitTestSettings.Environment,
            };

            int totalInitialQueues = RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken);
            using (var logger = RollbarFactory.CreateNew().Configure(loggerConfig))
            {
                Assert.AreEqual(totalInitialQueues + 1, RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken));
                this.ExpectedCommunicationEventsTotal++;
                logger.Log(ErrorLevel.Error, "test message");
            }
            // an unused queue does not get removed immediately (but eventually) - so let's wait for it for a few processing cycles: 
            while (totalInitialQueues != RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken))
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(250)); 
            }

            // if everything is good, we should get here way before this test method times out:
            Assert.AreEqual(totalInitialQueues, RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken));

        }

        [TestMethod]
        public void ReportException()
        {
            using (IRollbar logger = RollbarFactory.CreateNew().Configure(this._loggerConfig))
            {
                try
                {
                    this.ExpectedCommunicationEventsTotal++;
                    //TODO: implement and add SynchronousPackage around the payload object!!!
                    logger.Error(new ExceptionPackage(new System.Exception("test exception"), true));
                }
                catch
                {
                    Assert.Fail("the execution should not reach here!");
                }
            }
        }

        [TestMethod]
        public void ReportFromCatch()
        {
            try
            {
                var a = 10;
                var b = 0;
                var c = a / b;
            }
            catch (System.Exception ex)
            {
                using (IRollbar logger = RollbarFactory.CreateNew().Configure(this._loggerConfig))
                {
                    try
                    {
                        this.ExpectedCommunicationEventsTotal++;
                        //TODO: implement and add SynchronousPackage around the payload object!!!
                        logger.Error(new System.Exception("outer exception", ex));
                    }
                    catch
                    {
                        Assert.Fail();
                    }
                }
            }
        }

        [TestMethod]
        public void ReportMessage()
        {
            using (IRollbar logger = RollbarFactory.CreateNew().Configure(this._loggerConfig))
            {
                try
                {
                    this.ExpectedCommunicationEventsTotal++;
                    //TODO: implement and add SynchronousPackage around the payload object!!!
                    logger.Log(ErrorLevel.Error, "test message");
                }
                catch
                {
                    Assert.Fail("should never reach here!");
                }
            }
        }

        [DataTestMethod]
        [DataRow(ErrorLevel.Critical)]
        [DataRow(ErrorLevel.Error)]
        [DataRow(ErrorLevel.Warning)]
        [DataRow(ErrorLevel.Info)]
        [DataRow(ErrorLevel.Debug)]
        public void ConvenienceMethodsUseAppropriateErrorLevels(ErrorLevel expectedLogLevel)
        {
            var awaitAsyncSend = new ManualResetEventSlim(false);
            var acctualLogLevel = ErrorLevel.Info;
            void Transform(Payload payload)
            {
                acctualLogLevel = payload.Data.Level.Value;
                awaitAsyncSend.Set();
            }

            var loggerConfig = new RollbarConfig(RollbarUnitTestSettings.AccessToken)
            {
                Environment = RollbarUnitTestSettings.Environment,
                Transform = Transform,
            };
            this.ExpectedCommunicationEventsTotal++;
            using (var logger = RollbarFactory.CreateNew().Configure(loggerConfig))
            {
                try
                {
                    //TODO: implement and add SynchronousPackage around the payload object!!!
                    var ex = new Exception();
                    switch (expectedLogLevel)
                    {
                        case ErrorLevel.Critical:
                            logger.Critical(ex);
                            break;
                        case ErrorLevel.Error:
                            logger.Error(ex);
                            break;
                        case ErrorLevel.Warning:
                            logger.Warning(ex);
                            break;
                        case ErrorLevel.Info:
                            logger.Info(ex);
                            break;
                        case ErrorLevel.Debug:
                            logger.Debug(ex);
                            break;
                    }
                }
                catch
                {
                    Assert.Fail("should never reach here!");
                }
            }

            awaitAsyncSend.Wait();
            Assert.AreEqual(expectedLogLevel, acctualLogLevel);
        }


        [TestMethod]
        public void LongReportIsAsync()
        {
            const int maxCallLengthInMillisec = 50;
            TimeSpan payloadSubmissionDelay = TimeSpan.FromMilliseconds(3 * maxCallLengthInMillisec);
            RollbarConfig loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
            loggerConfig.Transform = delegate { Thread.Sleep(payloadSubmissionDelay); };
            using (IRollbar logger = RollbarFactory.CreateNew().Configure(loggerConfig))
            {
                try
                {
                    this.ExpectedCommunicationEventsTotal++;
                    Stopwatch sw = Stopwatch.StartNew();
                    logger.Log(ErrorLevel.Error, "test message");
                    sw.Stop();
                    Assert.IsTrue(sw.ElapsedMilliseconds < maxCallLengthInMillisec);
                    Thread.Sleep(payloadSubmissionDelay);
                }
                catch
                {
                    Assert.Fail("should never get here!");
                }
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void ExceptionWhileTransformingPayloadAsync()
        {
            this._transformException = false;

            RollbarConfig loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
            loggerConfig.Transform = delegate { throw new NullReferenceException(); };
            using (IRollbar logger = RollbarFactory.CreateNew().Configure(loggerConfig))
            {
                logger.InternalEvent += Logger_InternalEvent;

                try
                {
                    this.ExpectedCommunicationEventsTotal++;
                    logger.Log(ErrorLevel.Error, "test message");
                }
                catch
                {
                    logger.InternalEvent -= Logger_InternalEvent;
                    Assert.Fail("should never get here!");
                }

                this._signal.Wait();
                logger.InternalEvent -= Logger_InternalEvent;

                Assert.IsTrue(this._transformException);
            }
        }

        private bool _transformException = false;

        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0, 1);

        private void Logger_InternalEvent(object sender, RollbarEventArgs e)
        {
            this._transformException = true;
            this._signal.Release();
        }

        #region Stress test

        [TestMethod]
        [Timeout(60000)]
        public void MultithreadedStressTest_BlockingLogs()
        {
            RollbarLoggerFixture.stressLogsCount = 0;

            RollbarConfig loggerConfig = new RollbarConfig(RollbarUnitTestSettings.AccessToken)
            {
                Environment = RollbarUnitTestSettings.Environment,
                ReportingQueueDepth = 200,
            };

            this.ExpectedCommunicationEventsTotal = loggerConfig.ReportingQueueDepth;

            TimeSpan rollbarBlockingTimeout = TimeSpan.FromMilliseconds(55000);

            List<IRollbar> rollbars =
                new List<IRollbar>(MultithreadedStressTestParams.TotalThreads);
            List<ILogger> loggers = new List<ILogger>(MultithreadedStressTestParams.TotalThreads);

            for (int i = 0; i < MultithreadedStressTestParams.TotalThreads; i++)
            {
                var rollbar = RollbarFactory.CreateNew().Configure(loggerConfig);
                rollbar.InternalEvent += RollbarStress_InternalEvent;
                loggers.Add(rollbar.AsBlockingLogger(rollbarBlockingTimeout));
                rollbars.Add(rollbar);
            }

            PerformTheMultithreadedStressTest(loggers.ToArray());

            rollbars.ForEach(r => {
                r.InternalEvent -= RollbarStress_InternalEvent;
                r.Dispose();
            });
        }

        [TestMethod]
        [Timeout(60000)]
        public void MultithreadedStressTest()
        {
            RollbarLoggerFixture.stressLogsCount = 0;

            RollbarConfig loggerConfig = new RollbarConfig(RollbarUnitTestSettings.AccessToken)
            {
                Environment = RollbarUnitTestSettings.Environment,
                ReportingQueueDepth = 200,
            };

            this.ExpectedCommunicationEventsTotal = loggerConfig.ReportingQueueDepth;

            List<IRollbar> rollbars = 
                new List<IRollbar>(MultithreadedStressTestParams.TotalThreads);
            for (int i = 0; i < MultithreadedStressTestParams.TotalThreads; i++)
            {
                var rollbar = RollbarFactory.CreateNew().Configure(loggerConfig);
                rollbar.InternalEvent += RollbarStress_InternalEvent;
                rollbars.Add(rollbar);
            }

            PerformTheMultithreadedStressTest(rollbars.ToArray());

            rollbars.ForEach(r => {
                r.InternalEvent -= RollbarStress_InternalEvent;
                r.Dispose();
            });
        }

        private void PerformTheMultithreadedStressTest(IAsyncLogger[] loggers)
        {
            //first let's make sure the controller queues are not populated by previous tests:
            RollbarQueueController.Instance.FlushQueues();

            RollbarQueueController.Instance.InternalEvent += RollbarStress_InternalEvent;

            List<Task> tasks =
                new List<Task>(MultithreadedStressTestParams.TotalThreads);
            for (int t = 0; t < MultithreadedStressTestParams.TotalThreads; t++)
            {
                var task = new Task((state) =>
                {
                    int taskIndex = (int)state;
                    TimeSpan sleepIntervalDelta = 
                        TimeSpan.FromTicks(taskIndex * MultithreadedStressTestParams.LogIntervalDelta.Ticks);
                    var logger = loggers[taskIndex];
                    int i = 0;
                    while (i < MultithreadedStressTestParams.LogsPerThread)
                    {
                        var customFields = new Dictionary<string, object>(Fields.FieldsCount)
                        {
                            [Fields.ThreadID] = taskIndex + 1,
                            [Fields.ThreadLogID] = i + 1,
                            [Fields.Timestamp] = DateTimeOffset.UtcNow
                        };

                        logger.Info(
                            //$"{customFields[Fields.Timestamp]} Stress test: thread #{customFields[Fields.ThreadID]}, log #{customFields[Fields.ThreadLogID]}"
                            "Stress test"
                            , customFields
                            );

                        Thread.Sleep(MultithreadedStressTestParams.LogIntervalBase.Add(sleepIntervalDelta));
                        i++;
                    }
                }
                , t
                );

                tasks.Add(task);
            }

            tasks.ForEach(t => t.Start());

            Task.WaitAll(tasks.ToArray());

            int expectedCount = 2 * //we are subscribing to the internal events twice: on individual rollbar level and on the queue controller level
                MultithreadedStressTestParams.TotalThreads * MultithreadedStressTestParams.LogsPerThread;

            //we need this delay loop for async logs:
            while (RollbarQueueController.Instance.GetTotalPayloadCount() > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
            }

            RollbarQueueController.Instance.InternalEvent -= RollbarStress_InternalEvent;

            Assert.AreEqual(expectedCount, RollbarLoggerFixture.stressLogsCount);
        }

        private void PerformTheMultithreadedStressTest(ILogger[] loggers)
        {
            //first let's make sure the controller queues are not populated by previous tests:
            RollbarQueueController.Instance.FlushQueues();

            RollbarQueueController.Instance.InternalEvent += RollbarStress_InternalEvent;

            List<Task> tasks =
                new List<Task>(MultithreadedStressTestParams.TotalThreads);
            for (int t = 0; t < MultithreadedStressTestParams.TotalThreads; t++)
            {
                var task = new Task((state) =>
                {
                    int taskIndex = (int)state;
                    TimeSpan sleepIntervalDelta =
                        TimeSpan.FromTicks(taskIndex * MultithreadedStressTestParams.LogIntervalDelta.Ticks);
                    var logger = loggers[taskIndex];
                    int i = 0;
                    while (i < MultithreadedStressTestParams.LogsPerThread)
                    {
                        var customFields = new Dictionary<string, object>(Fields.FieldsCount)
                        {
                            [Fields.ThreadID] = taskIndex + 1,
                            [Fields.ThreadLogID] = i + 1,
                            [Fields.Timestamp] = DateTimeOffset.UtcNow
                        };

                        logger.Info(
                            //$"{customFields[Fields.Timestamp]} Stress test: thread #{customFields[Fields.ThreadID]}, log #{customFields[Fields.ThreadLogID]}"
                            "Stress test"
                            , customFields
                            );

                        Thread.Sleep(MultithreadedStressTestParams.LogIntervalBase.Add(sleepIntervalDelta));
                        i++;
                    }
                }
                , t
                );

                tasks.Add(task);
            }

            tasks.ForEach(t => t.Start());

            Task.WaitAll(tasks.ToArray());

            int expectedCount = 2 * //we are subscribing to the internal events twice: on individual rollbar level and on the queue controller level
                MultithreadedStressTestParams.TotalThreads * MultithreadedStressTestParams.LogsPerThread;

            //we need this delay loop for async logs:
            while (RollbarQueueController.Instance.GetTotalPayloadCount() > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
            }

            RollbarQueueController.Instance.InternalEvent -= RollbarStress_InternalEvent;

            Assert.AreEqual(expectedCount, RollbarLoggerFixture.stressLogsCount);
        }

        private static int stressLogsCount = 0;
        private static void RollbarStress_InternalEvent(object sender, RollbarEventArgs e)
        {
            Interlocked.Increment(ref RollbarLoggerFixture.stressLogsCount);
        }

        private static class MultithreadedStressTestParams
        {
            public const int TotalThreads = 20;
            public const int LogsPerThread = 10;
            public static readonly TimeSpan LogIntervalDelta =
                TimeSpan.FromMilliseconds(10);
            public static readonly TimeSpan LogIntervalBase =
                TimeSpan.FromMilliseconds(20);
        }

        private static class Fields
        {
            public const int FieldsCount = 3;
            public const string Timestamp = "stress.timestamp";
            public const string ThreadID = "stress.thread.id";
            public const string ThreadLogID = "stress.thread.log.id";
        }

#endregion Stress test

    }
}
