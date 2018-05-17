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

    [TestClass]
    [TestCategory(nameof(RollbarLoggerFixture))]
    public class RollbarLoggerFixture
    {
        RollbarConfig _loggerConfig =
            new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };

        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            this._loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        [TestMethod]
        public void TestSnapExceptionDataAsCustomData()
        {
            IDictionary<string, object> customData = null;

            var exceptionData = new[] {
                new { Key = 1, Value = "one"  },
                new { Key = 2, Value = "two"  },
                new { Key = 3, Value = "three"  },
            };

            var mostInnerException = new ArgumentException("Most Inner exception.");
            for(int dataIndx = 0; dataIndx <= 1; dataIndx++)
            {
                mostInnerException.Data[exceptionData[dataIndx].Key] = exceptionData[dataIndx].Value;
            }
            Assert.IsNull(customData);
            //expected to allocate customData and add some entries:
            RollbarLogger.SnapExceptionDataAsCustomData(mostInnerException, ref customData);
            Assert.IsNotNull(customData);
            Assert.AreEqual(2, customData.Count);

            var innerException = new NullReferenceException("Inner exception.", mostInnerException);
            for (int dataIndx = 1; dataIndx <= 2; dataIndx++)
            {
                innerException.Data[exceptionData[dataIndx].Key] = exceptionData[dataIndx].Value;
            }
            //expected to append more entries:
            RollbarLogger.SnapExceptionDataAsCustomData(innerException, ref customData);
            Assert.IsNotNull(customData);
            Assert.AreEqual(4, customData.Count);
            //expected to not double-enter same entries:
            RollbarLogger.SnapExceptionDataAsCustomData(innerException, ref customData);
            Assert.IsNotNull(customData);
            Assert.AreEqual(4, customData.Count);

            var ex = new Exception("Exception", innerException);
            for (int dataIndx = 0; dataIndx <= 2; dataIndx++)
            {
                ex.Data[exceptionData[dataIndx].Key] = exceptionData[dataIndx].Value;
            }
            ex.Data["nullValueKey"] = null;
            //expected to append more entries:
            RollbarLogger.SnapExceptionDataAsCustomData(ex, ref customData);
            Assert.IsNotNull(customData);
            Assert.AreEqual(8, customData.Count);

            customData = null;
            var aggregateException = new AggregateException("Aggregate Exception", innerException, mostInnerException, ex);
            aggregateException.Data["aggregateKey"] = "Aggregate Value";
            //expected to allocate cuastomData and add entries:
            RollbarLogger.SnapExceptionDataAsCustomData(aggregateException, ref customData);
            Assert.IsNotNull(customData);
            Assert.AreEqual(9, customData.Count);
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


        [TestMethod]
        public void ScopedInstanceTest()
        {
            RollbarConfig loggerConfig = new RollbarConfig(RollbarUnitTestSettings.AccessToken)
            {
                Environment = RollbarUnitTestSettings.Environment,
            };

            int totalInitialQueues = RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken);
            using (var logger = RollbarFactory.CreateNew().Configure(loggerConfig))
            {
                Assert.AreEqual(totalInitialQueues + 1, RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken));
                logger.Log(ErrorLevel.Error, "test message");
            }
            Assert.AreEqual(totalInitialQueues, RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken));

        }

        [TestMethod]
        public void ReportException()
        {
            using (IRollbar logger = RollbarFactory.CreateNew().Configure(this._loggerConfig))
            {
                try
                {
                    logger.Error(new System.Exception("test exception"));
                }
                catch
                {
                    Assert.IsTrue(false);
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
                        logger.Error(new System.Exception("outer exception", ex));
                    }
                    catch
                    {
                        Assert.IsTrue(false);
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
                    logger.Log(ErrorLevel.Error, "test message");
                }
                catch
                {
                    Assert.IsTrue(false);
                }
            }
        }


        private const int maxCallLengthInMillisec = 500;

        [TestMethod]
        [Timeout(maxCallLengthInMillisec)]
        public void LongReportIsAsync()
        {
            RollbarConfig loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
            loggerConfig.Transform = delegate { System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(10 * maxCallLengthInMillisec)); };
            using (IRollbar logger = RollbarFactory.CreateNew().Configure(loggerConfig))
            {
                try
                {
                    logger.Log(ErrorLevel.Error, "test message");
                }
                catch
                {
                    Assert.IsTrue(false);
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
                    logger.Log(ErrorLevel.Error, "test message");
                }
                catch
                {
                    logger.InternalEvent -= Logger_InternalEvent;
                    Assert.IsTrue(false);
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

            TimeSpan rollbarBlockingTimeout = TimeSpan.FromMilliseconds(55000);

            List<IRollbar> rollbars =
                new List<IRollbar>(MultithreadedStressTestParams.TotalThreads);
            List<ILogger> loggers = new List<ILogger>(MultithreadedStressTestParams.TotalThreads);

            for (int i = 0; i < MultithreadedStressTestParams.TotalThreads; i++)
            {
                var rollbar = RollbarFactory.CreateNew().Configure(loggerConfig);
                rollbar.InternalEvent += RollbarStress_InternalEvent;
                loggers.Add(rollbar.AsBlockingLogger(rollbarBlockingTimeout));
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
                        var customFields = new Dictionary<string, object>(Fields.FieldsCount);
                        customFields[Fields.ThreadID] = taskIndex + 1;
                        customFields[Fields.ThreadLogID] = i + 1;
                        customFields[Fields.Timestamp] = DateTimeOffset.UtcNow;

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
