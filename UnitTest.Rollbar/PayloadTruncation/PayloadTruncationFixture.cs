#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.PayloadTruncation
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using global::Rollbar.PayloadTruncation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory(nameof(PayloadTruncationFixture))]
    public class PayloadTruncationFixture
    {
        private readonly RollbarInfrastructureConfig _config;

        public PayloadTruncationFixture()
        {
            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment);
            RollbarDeveloperOptions developerOptions =
                new RollbarDeveloperOptions(ErrorLevel.Debug, true, true);

            this._config = new RollbarInfrastructureConfig();
            this._config.RollbarLoggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);
            this._config.RollbarLoggerConfig.RollbarDeveloperOptions.Reconfigure(developerOptions);
            if(!RollbarInfrastructure.Instance.IsInitialized)
            {
                RollbarInfrastructure.Instance.Init(this._config);
                RollbarInfrastructure.Instance.Config.RollbarLoggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);
                RollbarInfrastructure.Instance.Config.RollbarLoggerConfig.RollbarDeveloperOptions.Reconfigure(developerOptions);
            }

            Assert.IsTrue(this._config.RollbarLoggerConfig.RollbarDeveloperOptions.Transmit);
            Assert.IsTrue(RollbarInfrastructure.Instance.Config.RollbarLoggerConfig.RollbarDeveloperOptions.Transmit);

        }

        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void TestTruncation()
        {

            Payload[] testPayloads = new Payload[]
            {
                new Payload(this._config.RollbarLoggerConfig.RollbarDestinationOptions.AccessToken, new Data(
                    this._config.RollbarLoggerConfig,
                    new Body(new Message("A message I wish to send to the rollbar overlords", new Dictionary<string, object>() {{"longMessageString", "very-long-string-very-long-string-very-long-" }, {"theMessageNumber", 11 }, })),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
                new Payload(this._config.RollbarLoggerConfig.RollbarDestinationOptions.AccessToken, new Data(
                    this._config.RollbarLoggerConfig, 
                    new Body("A terrible crash!"),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
                new Payload(this._config.RollbarLoggerConfig.RollbarDestinationOptions.AccessToken, new Data(
                    this._config.RollbarLoggerConfig,
                    new Body(GetException()),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
                new Payload(this._config.RollbarLoggerConfig.RollbarDestinationOptions.AccessToken, new Data(
                    this._config.RollbarLoggerConfig,
                    new Body(GetAggregateException()),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
            };

            TimeSpan blockingTimeout = TimeSpan.FromSeconds(1);
            using (var logger = RollbarFactory.CreateNew().Configure(this._config.RollbarLoggerConfig))
            {
                Assert.IsTrue(logger.Config.RollbarDeveloperOptions.Transmit);

                foreach(var payload in testPayloads)
                {
                    logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
                }
            }

            IterativeTruncationStrategy truncationStrategy = null;

            const int payloadByteSizeLimit = 10; //we are intentionally exaggerating here to force all the iterations to happen...
            List<IPayloadTruncationStrategy> iterations = new List<IPayloadTruncationStrategy>();

            iterations.Add(new RawTruncationStrategy());
            truncationStrategy = new IterativeTruncationStrategy(400, iterations);
            AssertPayloadSizeReduction(false, testPayloads, truncationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, truncationStrategy);
            //using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            //{
            //    foreach (var payload in testPayloads)
            //    {
            //        logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
            //    }
            //}

            iterations.Add(new FramesTruncationStrategy(totalHeadFramesToPreserve: 1, totalTailFramesToPreserve: 1));
            truncationStrategy = new IterativeTruncationStrategy(payloadByteSizeLimit, iterations);
            AssertPayloadSizeReduction(false, testPayloads.Take(2).ToArray(), truncationStrategy);
            AssertPayloadSizeReduction(false, testPayloads.Take(2).ToArray(), truncationStrategy);
            Payload[] payloadsWithFrames = testPayloads.Reverse().Take(2).ToArray();
            foreach(var payload in payloadsWithFrames)
            {
                bool hasFramesToTrim = false;
                hasFramesToTrim |= (payload?.Data?.Body?.Trace?.Frames?.Length > 1);
                hasFramesToTrim |= ((payload?.Data?.Body?.TraceChain != null) && payload.Data.Body.TraceChain.Any(trace => trace?.Frames?.Length > 1));
                AssertPayloadSizeReduction(hasFramesToTrim, payload, truncationStrategy);
                AssertPayloadSizeReduction(false, payload, truncationStrategy);
            }
            //using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            //{
            //    foreach (var payload in testPayloads)
            //    {
            //        logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
            //    }
            //}

            iterations.Add(new StringsTruncationStrategy(stringBytesLimit: 10));
            truncationStrategy = new IterativeTruncationStrategy(payloadByteSizeLimit, iterations);
            AssertPayloadSizeReduction(true, testPayloads, truncationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, truncationStrategy);
            //using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            //{
            //    foreach (var payload in testPayloads)
            //    {
            //        logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
            //    }
            //}

            iterations.Add(new StringsTruncationStrategy(stringBytesLimit:  7));
            truncationStrategy = new IterativeTruncationStrategy(payloadByteSizeLimit, iterations);
            AssertPayloadSizeReduction(true, testPayloads, truncationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, truncationStrategy);
            //using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            //{
            //    foreach (var payload in testPayloads)
            //    {
            //        logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
            //    }
            //}

            iterations.Add(new StringsTruncationStrategy(stringBytesLimit:  3));
            truncationStrategy = new IterativeTruncationStrategy(payloadByteSizeLimit, iterations);
            AssertPayloadSizeReduction(true, testPayloads, truncationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, truncationStrategy);
            //using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            //{
            //    foreach (var payload in testPayloads)
            //    {
            //        logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
            //    }
            //}

            iterations.Add(new MinBodyTruncationStrategy());
            truncationStrategy = new IterativeTruncationStrategy(payloadByteSizeLimit, iterations);
            AssertPayloadSizeReduction(false, testPayloads.Take(2).ToArray(), truncationStrategy);
            AssertPayloadSizeReduction(false, testPayloads.Take(2).ToArray(), truncationStrategy);

            payloadsWithFrames = testPayloads.Reverse().Take(2).ToArray();
            foreach (var payload in payloadsWithFrames)
            {
                bool hasFramesToTrim = false;
                hasFramesToTrim |= (payload?.Data?.Body?.Trace?.Frames?.Length > 1);
                hasFramesToTrim |= ((payload?.Data?.Body?.TraceChain != null) && payload.Data.Body.TraceChain.Any(trace => trace?.Frames?.Length > 1));
                AssertPayloadSizeReduction(hasFramesToTrim, payload, truncationStrategy);
                AssertPayloadSizeReduction(false, payload, truncationStrategy);
            }

            //using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            //{
            //    foreach (var payload in testPayloads)
            //    {
            //        logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
            //    }
            //}
        }

        private void AssertPayloadSizeReduction(bool expectReduction, Payload testPayload, IterativeTruncationStrategy trancationStrategy)
        {
            AssertPayloadSizeReduction(expectReduction, new Payload[] { testPayload }, trancationStrategy);
        }

        private void AssertPayloadSizeReduction(bool expectReduction, Payload[] testPayloads, IterativeTruncationStrategy trancationStrategy)
        {
            foreach (var testPayload in testPayloads)
            {
                string original = JsonConvert.SerializeObject(testPayload);
                System.Diagnostics.Trace.WriteLine($"Original payload ({original.Length}): " + original);

                string truncated = null;
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                trancationStrategy.Truncate(testPayload);
                truncated = JsonConvert.SerializeObject(testPayload);
                System.Diagnostics.Trace.WriteLine($"Truncated payload ({truncated.Length}): " + truncated);

                string metadata = $"TruncationStrategy: {trancationStrategy} [ {trancationStrategy.OrderedTruncationStrategies.Select(strategy => strategy.ToString()).Aggregate((strategy, next) => next + ", " + strategy)} ], "
                    + Environment.NewLine + $"expectedReduction: {expectReduction}, "
                    + Environment.NewLine + $"original: {original} "
                    + Environment.NewLine + $"AND truncated: {truncated}";
                if (expectReduction)
                {
                    Assert.IsTrue(truncated.Length < original.Length, metadata);
                }
                else
                {
                    Assert.IsFalse(truncated.Length < original.Length, metadata);
                }
            }
        }

        private static AggregateException GetAggregateException()
        {
            try
            {
                SimulateAggregateException();
            }
            catch (AggregateException e)
            {
                return e;
            }

            throw new System.Exception("Unreachable");
        }

        private static void SimulateAggregateException()
        {
            Parallel.ForEach(new[] { 1, 2, 3, 4, 5 }, i => ThrowAnException());
        }

        private static void ThrowAnException()
        {
            SimulateException();
        }

        private static void SimulateException()
        {
            throw new System.Exception("Test");
        }

        private static System.Exception GetException()
        {
            try
            {
                ThrowAnException();
                throw new System.Exception("Unreachable");
            }
            catch (System.Exception e)
            {
                return e;
            }
        }

    }
}
