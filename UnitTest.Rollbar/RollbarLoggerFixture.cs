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
    using UnitTest.Rollbar.Mocks;
    using global::Rollbar.Infrastructure;

    /// <summary>
    /// Defines test class RollbarLoggerFixture.
    /// Implements the <see cref="UnitTest.Rollbar.RollbarLiveFixtureBase" />
    /// </summary>
    /// <seealso cref="UnitTest.Rollbar.RollbarLiveFixtureBase" />
    [TestClass]
    [TestCategory(nameof(RollbarLoggerFixture))]
    public class RollbarLoggerFixture
        : RollbarLiveFixtureBase
    {

        /// <summary>
        /// Sets the fixture up.
        /// </summary>
        [TestInitialize]
        public override void SetupFixture()
        {
            base.SetupFixture();
        }

        /// <summary>
        /// Tears down this fixture.
        /// </summary>
        [TestCleanup]
        public override void TearDownFixture()
        {
            base.TearDownFixture();
        }

        #region failure recovery tests

        /// <summary>
        /// Main purpose of these tests is to make sure that no Rollbar.NET usage scenario encountering an error
        /// brings down or halts the RollbarLogger operation.
        /// These tests are attempting to simulate various possible failure scenarios:
        /// - corrupt/invalid data for a payload,
        /// - an exception during payload transformation delegates execution,
        /// - exception within a packager or package decorator,
        /// - TBD...
        /// </summary>

        [TestMethod]
        public void InvalidPayloadDataTest()
        {
            //TODO:
        }

        [Ignore]
        [TestMethod]
        public void RethrowConfigOptionWorks()
        {
            this.Reset();

            RollbarLoggerConfig config = this.ProvideLiveRollbarConfig() as RollbarLoggerConfig;

            using IRollbar rollbar = this.ProvideDisposableRollbar();
            rollbar.Configure(config);
            this.IncrementCount<CommunicationEventArgs>();
            rollbar.Critical(ExceptionSimulator.GetExceptionWith(5,"Exception logged async!"));
            this.IncrementCount<CommunicationEventArgs>();
            rollbar.AsBlockingLogger(TimeSpan.FromSeconds(3))
                .Critical(ExceptionSimulator.GetExceptionWith(5,"Exception logged sync!"));

            int rethrowCount = 0;
            config.RollbarDeveloperOptions.RethrowExceptionsAfterReporting = true;
            rollbar.Configure(config);
            try
            {
                this.IncrementCount<CommunicationEventArgs>();
                rollbar.Critical(ExceptionSimulator.GetExceptionWith(5,"Exception logged async with rethrow!"));
            }
            catch
            {
                rethrowCount++;
            }

            try
            {
                this.IncrementCount<CommunicationEventArgs>();
                rollbar.AsBlockingLogger(TimeSpan.FromSeconds(3))
                    .Critical(ExceptionSimulator.GetExceptionWith(5,"Exception logged sync with rethrow!"));
            }
            catch
            {
                rethrowCount++;
            }

            config.RollbarDeveloperOptions.RethrowExceptionsAfterReporting = false;
            rollbar.Configure(config);
            this.IncrementCount<CommunicationEventArgs>();
            rollbar.Critical(ExceptionSimulator.GetExceptionWith(5,"Exception logged async!"));
            this.IncrementCount<CommunicationEventArgs>();
            rollbar.AsBlockingLogger(TimeSpan.FromSeconds(3))
                .Critical(ExceptionSimulator.GetExceptionWith(5,"Exception logged sync!"));

            Assert.AreEqual(2,rethrowCount,"matching total of rethrows...");
        }

        [Ignore]
        [TestMethod]
        public void TransmitConfigOptionWorks()
        {
            this.Reset();

            RollbarLoggerConfig config = this.ProvideLiveRollbarConfig() as RollbarLoggerConfig;

            using IRollbar rollbar = this.ProvideDisposableRollbar();
            rollbar.Configure(config);
            this.IncrementCount<CommunicationEventArgs>();
            rollbar.Critical("Transmission is expected to happen!");
            this.IncrementCount<CommunicationEventArgs>();
            rollbar.AsBlockingLogger(TimeSpan.FromSeconds(3)).Critical("Transmission is expected to happen!");

            config.RollbarDeveloperOptions.Transmit = false;
            rollbar.Configure(config);
            this.IncrementCount<TransmissionOmittedEventArgs>();
            rollbar.Critical("Transmission is expected to be omitted!");
            this.IncrementCount<TransmissionOmittedEventArgs>();
            rollbar.AsBlockingLogger(TimeSpan.FromSeconds(3)).Critical("Transmission is expected to be omitted!");

            config.RollbarDeveloperOptions.Transmit = true;
            rollbar.Configure(config);
            this.IncrementCount<CommunicationEventArgs>();
            rollbar.Critical("Transmission is expected to happen!");
            this.IncrementCount<CommunicationEventArgs>();
            rollbar.AsBlockingLogger(TimeSpan.FromSeconds(3)).Critical("Transmission is expected to happen!");
        }

        /// <summary>
        /// Defines the test method FaultyPayloadTransformationTest.
        /// </summary>
        [Ignore]
        [TestMethod]
        public void FaultyPayloadTransformationTest()
        {
            this.Reset();

            RollbarLoggerConfig config = this.ProvideLiveRollbarConfig() as RollbarLoggerConfig;
            RollbarPayloadManipulationOptions rollbarPayloadManipulationOptions = new RollbarPayloadManipulationOptions();
            rollbarPayloadManipulationOptions.Transform = delegate (Payload payload)
            {
                throw new Exception("Buggy transform delegate!");
            };
            config.RollbarPayloadManipulationOptions.Reconfigure(rollbarPayloadManipulationOptions);

            using (IRollbar rollbar = this.ProvideDisposableRollbar())
            {
                rollbar.Configure(config);

                rollbar.Critical("This message's Transform will fail!");

                this.VerifyInstanceOperational(rollbar);
                // one more extra sanity check:
                Assert.AreEqual(0, RollbarQueueController.Instance.GetTotalPayloadCount());
            }

            this.Reset();
        }

        /// <summary>
        /// Defines the test method FaultyCheckIgnoreTest.
        /// </summary>
        [Ignore]
        [TestMethod]
        public void FaultyCheckIgnoreTest()
        {
            this.Reset();

            RollbarLoggerConfig config = this.ProvideLiveRollbarConfig() as RollbarLoggerConfig;
            RollbarPayloadManipulationOptions payloadManipulationOptions = new RollbarPayloadManipulationOptions();
            payloadManipulationOptions.CheckIgnore = delegate (Payload payload)
            {
                throw new Exception("Buggy check-ignore delegate!");
            };
            config.RollbarPayloadManipulationOptions.Reconfigure(payloadManipulationOptions);

            using (IRollbar rollbar = this.ProvideDisposableRollbar())
            {
                rollbar.Configure(config);

                rollbar.Critical("This message's CheckIgnore will fail!");

                this.VerifyInstanceOperational(rollbar);
                // one more extra sanity check:
                Assert.AreEqual(0, RollbarQueueController.Instance.GetTotalPayloadCount());
            }

            this.Reset();
        }

        /// <summary>
        /// Defines the test method FaultyTruncateTest.
        /// </summary>
        [Ignore]
        [TestMethod]
        public void FaultyTruncateTest()
        {
            this.Reset();

            RollbarLoggerConfig config = this.ProvideLiveRollbarConfig() as RollbarLoggerConfig;
            RollbarPayloadManipulationOptions payloadManipulationOptions = new RollbarPayloadManipulationOptions();
            payloadManipulationOptions.Truncate = delegate (Payload payload)
            {
                throw new Exception("Buggy truncate delegate!");
            };
            config.RollbarPayloadManipulationOptions.Reconfigure(payloadManipulationOptions);

            using(IRollbar rollbar = this.ProvideDisposableRollbar())
            {
                rollbar.Configure(config);

                rollbar.Critical("This message's Truncate will fail!");

                this.VerifyInstanceOperational(rollbar);
                // one more extra sanity check:
                Assert.AreEqual(0, RollbarQueueController.Instance.GetTotalPayloadCount());
            }

            this.Reset();
        }

        /// <summary>
        /// Enum TrickyPackage
        /// </summary>
        public enum TrickyPackage
        {
            /// <summary>
            /// The asynchronous faulty package
            /// </summary>
            AsyncFaultyPackage,
            /// <summary>
            /// The synchronize faulty package
            /// </summary>
            SyncFaultyPackage,
            /// <summary>
            /// The asynchronous nothing package
            /// </summary>
            AsyncNothingPackage,
            /// <summary>
            /// The synchronize nothing package
            /// </summary>
            SyncNothingPackage,
        }

        /// <summary>
        /// Trickies the package test.
        /// </summary>
        /// <param name="trickyPackage">The tricky package.</param>
        [Ignore]
        [DataTestMethod]
        [DataRow(TrickyPackage.AsyncFaultyPackage)]
        [DataRow(TrickyPackage.SyncFaultyPackage)]
        [DataRow(TrickyPackage.AsyncNothingPackage)]
        [DataRow(TrickyPackage.SyncNothingPackage)]
        public void TrickyPackageTest(TrickyPackage trickyPackage)
        {
            this.Reset();

            IRollbarPackage package = null;
            switch (trickyPackage)
            {
                case TrickyPackage.AsyncFaultyPackage:
                    package = new FaultyPackage(false);
                    break;
                case TrickyPackage.SyncFaultyPackage:
                    package = new FaultyPackage(true);
                    break;
                case TrickyPackage.AsyncNothingPackage:
                    package = new NothingPackage(false);
                    break;
                case TrickyPackage.SyncNothingPackage:
                    package = new NothingPackage(true);
                    break;
                default:
                    Assert.Fail($"Unexpected {nameof(trickyPackage)}: {trickyPackage}!");
                    break;
            }

            using (IRollbar rollbar = this.ProvideDisposableRollbar())
            {
                rollbar.Critical(package);

                this.VerifyInstanceOperational(rollbar);
                // one more extra sanity check:
                Assert.AreEqual(0, RollbarQueueController.Instance.GetTotalPayloadCount());
            }

            this.Reset();
        }


        #endregion failure recovery tests

        #region rate limiting tests

        /// <summary>
        /// Defines the test method RateLimitConfigSettingOverridesServerHeadersBasedReportingRateTest.
        /// </summary>
        [Ignore]
        [TestMethod]
        public void RateLimitConfigSettingOverridesServerHeadersBasedReportingRateTest()
        {
            const int totalTestPayloads = 10;
            const int localReportingRate = 30;

            using (IRollbar rollbar = this.ProvideDisposableRollbar())
            {
                // using Rollbar API service enforced reporting rate:
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < totalTestPayloads; i++)
                {
                    rollbar.AsBlockingLogger(TimeSpan.FromSeconds(3)).Critical("RateLimitConfigSettingOverridesServerHeadersBasedReportingRateTest");
                }
                sw.Stop();
                TimeSpan serverRateDuration = sw.Elapsed;

                // reconfigure with locally defined reporting rate:
                //RollbarLoggerConfig rollbarConfig = new RollbarLoggerConfig();
                //rollbarConfig.Reconfigure(rollbar.Config);
                //Assert.IsFalse(rollbarConfig.RollbarInfrastructureOptions.MaxReportsPerMinute.HasValue);
                Assert.IsFalse(RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.MaxReportsPerMinute.HasValue);

                RollbarInfrastructureOptions rollbarInfrastructureOptions = new RollbarInfrastructureOptions();
                rollbarInfrastructureOptions.MaxReportsPerMinute = localReportingRate;
                RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.Reconfigure(rollbarInfrastructureOptions);
                Assert.IsTrue(RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.MaxReportsPerMinute.HasValue);
                Assert.AreEqual(localReportingRate, RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.MaxReportsPerMinute.Value);

                //rollbar.Config.Reconfigure(rollbarConfig);
                //Assert.IsTrue(rollbar.Config.RollbarInfrastructureOptions.MaxReportsPerMinute.HasValue);
                //Assert.AreEqual(localReportingRate, rollbar.Config.RollbarInfrastructureOptions.MaxReportsPerMinute.Value);

                // using local config defined reporting rate:
                sw.Restart();
                for (int i = 0; i < totalTestPayloads; i++)
                {
                    rollbar.AsBlockingLogger(TimeSpan.FromSeconds(3)).Critical("RateLimitConfigSettingOverridesServerHeadersBasedReportingRateTest");
                }
                sw.Stop();
                TimeSpan localRateDuration = sw.Elapsed;

                Assert.IsTrue(2 < (localRateDuration.TotalMilliseconds / serverRateDuration.TotalMilliseconds), "This is good enough confirmation of locally defined rate in action...");
            }

            this.IncrementCount<CommunicationEventArgs>(2 * totalTestPayloads);
        }

        #endregion rate limiting tests

        [TestMethod]
        public void ThrowsExceptionWhenInitializedWithInvalidConfigInstance()
        {
            RollbarLoggerConfig invalidConfig = new RollbarLoggerConfig(string.Empty);
            invalidConfig.RollbarPayloadAdditionOptions.Person = new Person();

            try
            {
                using var rollbar = RollbarFactory.CreateNew(invalidConfig);
            }
            catch (RollbarException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
                Assert.IsTrue(ex.Data.Count > 0, "Expected to contain failed validation rules!");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
                Assert.Fail("Should never reach here due to exception above!");
            }

            Assert.Fail("Should never reach here due to exception above!");
        }

        [TestMethod]
        public void ThrowsExceptionWhenConfiguredWithInvalidConfigInstance()
        {
            RollbarLoggerConfig invalidConfig = new RollbarLoggerConfig(string.Empty);
            invalidConfig.RollbarPayloadAdditionOptions.Person = new Person();

            var rollbar = RollbarFactory.CreateNew();

            try
            {
                rollbar.Configure(invalidConfig);
            }
            catch (RollbarException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
                Assert.IsTrue(ex.Data.Count > 0, "Expected to contain failed validation rules!");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
                Assert.Fail("Should never reach here due to exception above!");
            }

            Assert.Fail("Should never reach here due to exception above!");
        }

        //TODO: redo the test below to properly account for payload persistence in case of wrong proxy!!!

        /// <summary>
        /// Defines the test method AllowsProxySettingsReconfiguration.
        /// </summary>
        //[TestMethod]
        //public void AllowsProxySettingsReconfiguration()
        //{
        //    this.Reset();

        //    using (IRollbar logger = this.ProvideDisposableRollbar())
        //    {
        //        IRollbarConfig initialConfig = logger.Config;

        //        Assert.AreSame(initialConfig, logger.Config);
        //        logger.Configure(initialConfig);
        //        Assert.AreSame(initialConfig, logger.Config);

        //        int errorCount = 0;
        //        logger.AsBlockingLogger(TimeSpan.FromSeconds(3)).Info("test 1");
        //        this.IncrementCount<CommunicationEventArgs>();
        //        Assert.AreEqual(0, errorCount, "Checking errorCount 1.");

        //        RollbarConfig newConfig = new RollbarConfig("seed");
        //        newConfig.Reconfigure(initialConfig);
        //        Assert.AreNotSame(initialConfig, newConfig);
        //        logger.Configure(newConfig);
        //        logger.AsBlockingLogger(TimeSpan.FromSeconds(3)).Info("test 2");
        //        this.IncrementCount<CommunicationEventArgs>();
        //        Assert.AreEqual(0, errorCount, "Checking errorCount 2.");

        //        newConfig.ProxyAddress = "www.fakeproxy.com";
        //        newConfig.ProxyUsername = "fakeusername";
        //        newConfig.ProxyPassword = "fakepassword";
        //        logger.Configure(newConfig);
        //        Assert.IsFalse(string.IsNullOrEmpty(logger.Config.ProxyAddress));
        //        Assert.IsFalse(string.IsNullOrEmpty(logger.Config.ProxyUsername));
        //        Assert.IsFalse(string.IsNullOrEmpty(logger.Config.ProxyPassword));
        //        try
        //        {
        //            // the fake proxy settings will not cause a timeout exception here!
        //            // the payload will not be transmitted but persisted:
        //            int expectedFewcomMMerrors = 6; // this is a non-deterministic experimental value!
        //            while (expectedFewcomMMerrors-- > 0)
        //            {
        //                this.IncrementCount<CommunicationErrorEventArgs>();
        //            }
        //            logger.AsBlockingLogger(TimeSpan.FromSeconds(3)).Info("test 3 with fake proxy");
        //        }
        //        catch
        //        {
        //            errorCount++;
        //        }
        //        Assert.AreEqual(0, errorCount, "Checking errorCount 3.");
        //        //TODO: gain access to the payload store persisted records count.
        //        //      The count is expected to be 1.

        //        newConfig.ProxyAddress = null;
        //        newConfig.ProxyUsername = null;
        //        newConfig.ProxyPassword = null;
        //        logger.Configure(newConfig);
        //        Assert.IsTrue(string.IsNullOrEmpty(logger.Config.ProxyAddress));
        //        Assert.IsTrue(string.IsNullOrEmpty(logger.Config.ProxyUsername));
        //        Assert.IsTrue(string.IsNullOrEmpty(logger.Config.ProxyPassword));
        //        try
        //        {
        //            // the fake proxy settings are gone, so, next call is expected to succeed:
        //            this.IncrementCount<CommunicationEventArgs>();
        //            logger.AsBlockingLogger(TimeSpan.FromSeconds(15)).Info("test 4");
        //        }
        //        catch
        //        {
        //            errorCount++;
        //        }
        //        Assert.AreEqual(0, errorCount, "Checking errorCount 4.");
        //        //TODO: gain access to the payload store persisted records count.
        //        //      The count is expected to be 1 (one record stuck with wrfng proxy settings until stale in a few days).
        //    }
        //}

        /// <summary>
        /// Defines the test method ImplementsIDisposable.
        /// </summary>
        [TestMethod]
        public void ImplementsIDisposable()
        {
            using IRollbar logger = this.ProvideDisposableRollbar();
            IDisposable disposable = logger as IDisposable;
            Assert.IsNotNull(disposable);
        }

        /// <summary>
        /// The maximum scoped instance test duration in millisec
        /// </summary>
        private const int maxScopedInstanceTestDurationInMillisec = 60 * 1000;
        /// <summary>
        /// Defines the test method ScopedInstanceTest.
        /// </summary>
        [Ignore]
        [TestMethod]
        [Timeout(maxScopedInstanceTestDurationInMillisec)]
        public void ScopedInstanceTest()
        {
            // we need to make sure we are starting clean:
            RollbarQueueController.Instance.FlushQueues();
            RollbarQueueController.Instance.Start();
            var accessTokenQueues =
                RollbarQueueController.Instance.GetQueues(RollbarUnitTestSettings.AccessToken);
            while (accessTokenQueues.Any())
            {
                string msg = "Initial queues count: " + accessTokenQueues.Count();
                System.Diagnostics.Trace.WriteLine(msg);
                Console.WriteLine(msg);
                foreach(var queue in accessTokenQueues)
                {
                    msg = "---Payloads in a queue: " + queue.GetPayloadCount();
                    System.Diagnostics.Trace.WriteLine(msg);
                    Console.WriteLine(msg);

                    if(!queue.IsReleased)
                    {
                        queue.Release();
                    }
                    else
                    {
                        queue.Flush();
                    }
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
                accessTokenQueues =
                    RollbarQueueController.Instance.GetQueues(RollbarUnitTestSettings.AccessToken);
            }

            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(
                    RollbarUnitTestSettings.AccessToken,
                    RollbarUnitTestSettings.Environment
                    );
            RollbarLoggerConfig loggerConfig = new RollbarLoggerConfig();
            loggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);

            int totalInitialQueues = RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken);
            using (var logger = RollbarFactory.CreateNew().Configure(loggerConfig))
            {
                Assert.AreEqual(totalInitialQueues + 1, RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken));
                this.IncrementCount<CommunicationEventArgs>();
                logger.Log(ErrorLevel.Error, "test message");
            }
            // an unused queue does not get removed immediately (but eventually) - so let's wait for it for a few processing cycles:
            int currentQueuesCount =
                RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken);
            while (totalInitialQueues != currentQueuesCount)
            {
                string msg = "Current queues count: " + currentQueuesCount + " while initial count was: " + totalInitialQueues;
                System.Diagnostics.Trace.WriteLine(msg);
                Console.WriteLine(msg);
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
                currentQueuesCount =
                    RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken);
            }

            // if everything is good, we should get here way before this test method times out:
            Assert.AreEqual(totalInitialQueues, RollbarQueueController.Instance.GetQueuesCount(RollbarUnitTestSettings.AccessToken));

        }

        /// <summary>
        /// Defines the test method ReportException.
        /// </summary>
        [Ignore]
        [TestMethod]
        public void ReportException()
        {
            using IRollbar logger = this.ProvideDisposableRollbar();
            try
            {
                this.IncrementCount<CommunicationEventArgs>();
                logger.AsBlockingLogger(defaultRollbarTimeout).Error(new System.Exception("test exception"));
            }
            catch
            {
                Assert.Fail("the execution should not reach here!");
            }
        }

        /// <summary>
        /// Defines the test method ReportFromCatch.
        /// </summary>
        [Ignore]
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
                using IRollbar logger = this.ProvideDisposableRollbar();
                try
                {
                    this.IncrementCount<CommunicationEventArgs>();
                    logger.AsBlockingLogger(defaultRollbarTimeout).Error(new System.Exception("outer exception",ex));
                }
                catch
                {
                    Assert.Fail();
                }
            }
        }

        /// <summary>
        /// Defines the test method ReportMessage.
        /// </summary>
        [Ignore]
        [TestMethod]
        public void ReportMessage()
        {
            using IRollbar logger = this.ProvideDisposableRollbar();
            try
            {
                this.IncrementCount<CommunicationEventArgs>();
                logger.AsBlockingLogger(defaultRollbarTimeout).Log(ErrorLevel.Error,"test message");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Assert.Fail("should never reach here!");
            }
        }

        /// <summary>
        /// Conveniences the methods use appropriate error levels.
        /// </summary>
        /// <param name="expectedLogLevel">The expected log level.</param>
        [Ignore]
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

            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(
                    RollbarUnitTestSettings.AccessToken,
                    RollbarUnitTestSettings.Environment
                    );
            RollbarLoggerConfig loggerConfig = new RollbarLoggerConfig();
            loggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);

            RollbarPayloadManipulationOptions payloadManipulationOptions = new RollbarPayloadManipulationOptions();
            payloadManipulationOptions.Transform = Transform;
            loggerConfig.RollbarPayloadManipulationOptions.Reconfigure(payloadManipulationOptions);

            this.IncrementCount<CommunicationEventArgs>();
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

        /// <summary>
        /// Defines the test method LongReportIsAsync.
        /// </summary>
        [Ignore]
        [TestMethod]
        public void LongReportIsAsync()
        {
            const int maxCallLengthInMillisec = 50;
            TimeSpan payloadSubmissionDelay = TimeSpan.FromMilliseconds(3 * maxCallLengthInMillisec);

            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(
                    RollbarUnitTestSettings.AccessToken,
                    RollbarUnitTestSettings.Environment
                    );
            RollbarLoggerConfig loggerConfig = new RollbarLoggerConfig();
            loggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);

            RollbarPayloadManipulationOptions payloadManipulationOptions = new RollbarPayloadManipulationOptions();
            payloadManipulationOptions.Transform = delegate
            {
                Thread.Sleep(payloadSubmissionDelay);
            };
            loggerConfig.RollbarPayloadManipulationOptions.Reconfigure(payloadManipulationOptions);

            using IRollbar logger = RollbarFactory.CreateNew().Configure(loggerConfig);
            try
            {
                this.IncrementCount<CommunicationEventArgs>();
                Stopwatch sw = Stopwatch.StartNew();
                logger.Log(ErrorLevel.Error,"test message");
                sw.Stop();
                Assert.IsTrue(sw.ElapsedMilliseconds < maxCallLengthInMillisec);
                Thread.Sleep(payloadSubmissionDelay);
            }
            catch
            {
                Assert.Fail("should never get here!");
            }
        }

        /// <summary>
        /// Defines the test method ExceptionWhileTransformingPayloadAsync.
        /// </summary>
        [Ignore]
        [TestMethod]
        [Timeout(5000)]
        public void ExceptionWhileTransformingPayloadAsync()
        {
            this._transformException = false;

            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(
                    RollbarUnitTestSettings.AccessToken,
                    RollbarUnitTestSettings.Environment
                    );
            RollbarLoggerConfig loggerConfig = new RollbarLoggerConfig();
            loggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);

            RollbarPayloadManipulationOptions payloadManipulationOptions = new RollbarPayloadManipulationOptions();
            payloadManipulationOptions.Transform = delegate {
                throw new NullReferenceException();
            };
            loggerConfig.RollbarPayloadManipulationOptions.Reconfigure(payloadManipulationOptions);

            using IRollbar logger = RollbarFactory.CreateNew().Configure(loggerConfig);
            logger.InternalEvent += Logger_InternalEvent;

            try
            {
                this.IncrementCount<CommunicationEventArgs>();
                logger.Log(ErrorLevel.Error,"test message");
            }
            catch
            {
                logger.InternalEvent -= Logger_InternalEvent;
                Assert.Fail("should never get here!");
                throw;
            }

            this._signal.Wait();
            logger.InternalEvent -= Logger_InternalEvent;

            Assert.IsTrue(this._transformException);
        }

        /// <summary>
        /// The transform exception
        /// </summary>
        private bool _transformException = false;

        /// <summary>
        /// The signal
        /// </summary>
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0, 1);

        /// <summary>
        /// Handles the InternalEvent event of the Logger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        private void Logger_InternalEvent(object sender, RollbarEventArgs e)
        {
            this._transformException = true;
            this._signal.Release();
        }

        #region Stress test

        /// <summary>
        /// Defines the test method MultithreadedStressTest_BlockingLogs.
        /// </summary>
        [Ignore]
        [TestMethod]
        [Timeout(120000)]
        public void MultithreadedStressTest_BlockingLogs()
        {
            RollbarLoggerFixture.stressLogsCount = 0;

            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(
                    RollbarUnitTestSettings.AccessToken,
                    RollbarUnitTestSettings.Environment
                    );
            RollbarLoggerConfig loggerConfig = new RollbarLoggerConfig();
            loggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);

            RollbarInfrastructureOptions infrastructureOptions = new RollbarInfrastructureOptions();
            infrastructureOptions.ReportingQueueDepth = 200;
            RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.Reconfigure(infrastructureOptions);

            this.IncrementCount<CommunicationEventArgs>(RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.ReportingQueueDepth);

            TimeSpan rollbarBlockingTimeout = TimeSpan.FromMilliseconds(55000);

            List<IRollbar> rollbars =
                new List<IRollbar>(MultithreadedStressTestParams.TotalThreads);
            List<ILogger> loggers = new List<ILogger>(MultithreadedStressTestParams.TotalThreads);

            for (int i = 0; i < MultithreadedStressTestParams.TotalThreads; i++)
            {
                var rollbar = RollbarFactory.CreateNew().Configure(loggerConfig);
                loggers.Add(rollbar.AsBlockingLogger(rollbarBlockingTimeout));
                rollbars.Add(rollbar);
            }

            PerformTheMultithreadedStressTest(loggers.ToArray());

            rollbars.ForEach(r => {
                r.Dispose();
            });
        }

        /// <summary>
        /// Defines the test method MultithreadedStressTest_ConnectivityMonitorDisabled.
        /// </summary>
        //[TestMethod]
        //[Timeout(60000)]
        //public void MultithreadedStressTest_ConnectivityMonitorDisabled()
        //{
        //    ConnectivityMonitor.Instance.Disable();
        //    MultithreadedStressTest();
        //}

        /// <summary>
        /// Defines the test method MultithreadedStressTest.
        /// </summary>
        [Ignore]
        [TestMethod]
        [Timeout(100000)]
        public void MultithreadedStressTest()
        {
            RollbarLoggerFixture.stressLogsCount = 0;

            //ConnectivityMonitor.Instance.Disable();

            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(
                    RollbarUnitTestSettings.AccessToken,
                    RollbarUnitTestSettings.Environment
                    );
            RollbarLoggerConfig loggerConfig = new RollbarLoggerConfig();
            loggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);

            RollbarInfrastructureOptions infrastructureOptions = new RollbarInfrastructureOptions();
            infrastructureOptions.ReportingQueueDepth = 200;
            RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.Reconfigure(infrastructureOptions);

            this.IncrementCount<CommunicationEventArgs>(RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.ReportingQueueDepth);

            List<IRollbar> rollbars =
                new List<IRollbar>(MultithreadedStressTestParams.TotalThreads);
            for (int i = 0; i < MultithreadedStressTestParams.TotalThreads; i++)
            {
                var rollbar = RollbarFactory.CreateNew().Configure(loggerConfig);
                rollbars.Add(rollbar);
            }

            PerformTheMultithreadedStressTest(rollbars.ToArray());

            rollbars.ForEach(r => {
                r.Dispose();
            });
        }

        /// <summary>
        /// Performs the multithreaded stress test.
        /// </summary>
        /// <param name="loggers">The loggers.</param>
        private static void PerformTheMultithreadedStressTest(ILogger[] loggers)
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

            int expectedCount =
                MultithreadedStressTestParams.TotalThreads * MultithreadedStressTestParams.LogsPerThread;

            //we need this delay loop for async logs:
            while (RollbarQueueController.Instance.GetTotalPayloadCount() > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
            }

            Thread.Sleep(TimeSpan.FromSeconds(10));

            RollbarQueueController.Instance.InternalEvent -= RollbarStress_InternalEvent;

            Assert.AreEqual(expectedCount, RollbarLoggerFixture.stressLogsCount, "Matching stressLogsCount");
        }

        /// <summary>
        /// The stress logs count
        /// </summary>
        private static int stressLogsCount = 0;
        /// <summary>
        /// Handles the InternalEvent event of the RollbarStress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        private static void RollbarStress_InternalEvent(object sender, RollbarEventArgs e)
        {
            if (e is CommunicationEventArgs)
            {
                Interlocked.Increment(ref RollbarLoggerFixture.stressLogsCount);
            }
            else
            {
            }
        }

        /// <summary>
        /// Class MultithreadedStressTestParams.
        /// </summary>
        private static class MultithreadedStressTestParams
        {
            /// <summary>
            /// The total threads
            /// </summary>
            public const int TotalThreads = 20;
            /// <summary>
            /// The logs per thread
            /// </summary>
            public const int LogsPerThread = 10;
            /// <summary>
            /// The log interval delta
            /// </summary>
            public static readonly TimeSpan LogIntervalDelta =
                TimeSpan.FromMilliseconds(10);
            /// <summary>
            /// The log interval base
            /// </summary>
            public static readonly TimeSpan LogIntervalBase =
                TimeSpan.FromMilliseconds(20);
        }

        /// <summary>
        /// Class Fields.
        /// </summary>
        private static class Fields
        {
            /// <summary>
            /// The fields count
            /// </summary>
            public const int FieldsCount = 3;
            /// <summary>
            /// The timestamp
            /// </summary>
            public const string Timestamp = "stress.timestamp";
            /// <summary>
            /// The thread identifier
            /// </summary>
            public const string ThreadID = "stress.thread.id";
            /// <summary>
            /// The thread log identifier
            /// </summary>
            public const string ThreadLogID = "stress.thread.log.id";
        }

        #endregion Stress test

        //[TestMethod]
        //public void _RollbarRateLimitVerification()
        //{
        //    RollbarConfig config = this.ProvideLiveRollbarConfig() as RollbarConfig;

        //    int count = 0;

        //    using (IRollbar rollbar = this.ProvideDisposableRollbar())
        //    {
        //        rollbar.Configure(config);

        //        while (count++ < 300)
        //        {
        //            rollbar.Critical("RollbarRateLimitVerification test");
        //            Thread.Sleep(TimeSpan.FromSeconds(1));
        //        }

        //    }
        //}

    }
}
