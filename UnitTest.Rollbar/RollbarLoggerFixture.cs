namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;

    [TestClass]
    [TestCategory("RollbarLoggerFixture")]
    public class RollbarLoggerFixture
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
        public void ImplementsIDisposable()
        {
            IDisposable disposable = _logger as IDisposable;
            Assert.IsNotNull(disposable);
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
            try
            {
                _logger.Log(ErrorLevel.Error, new System.Exception("test exception"));
            }
            catch
            {
                Assert.IsTrue(false); 
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
                try
                {
                    _logger.Error(new System.Exception("outer exception", ex));
                }
                catch
                {
                    Assert.IsTrue(false);
                }
            }
        }

        [TestMethod]
        public void ReportMessage()
        {
            try
            {
                _logger.Log(ErrorLevel.Error, "test message");
            }
            catch
            {
                Assert.IsTrue(false);
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
            IRollbar logger = RollbarFactory.CreateNew().Configure(loggerConfig);

            try
            {
                logger.Log(ErrorLevel.Error, "test message");
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void ExceptionWhileTransformingPayload()
        {
            this._transformException = false;

            RollbarConfig loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
            loggerConfig.Transform = delegate { throw new NullReferenceException(); };
            IRollbar logger = RollbarFactory.CreateNew().Configure(loggerConfig);
            logger.InternalEvent += Logger_InternalEvent;

            try
            {
                logger.Log(ErrorLevel.Error, "test message");
            }
            catch
            {
                Assert.IsTrue(false);
            }

            System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(3 * maxCallLengthInMillisec));
            Assert.IsTrue(this._transformException);
        }

        private bool _transformException = false;

        private void Logger_InternalEvent(object sender, RollbarEventArgs e)
        {
            this._transformException = true;
        }
    }
}
