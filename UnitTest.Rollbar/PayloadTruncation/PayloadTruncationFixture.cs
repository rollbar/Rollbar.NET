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
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory(nameof(PayloadTruncationFixture))]
    public class PayloadTruncationFixture
    {
        private readonly RollbarConfig _config;

        public PayloadTruncationFixture()
        {
            this._config = new RollbarConfig(RollbarUnitTestSettings.AccessToken)
            {
                Environment = RollbarUnitTestSettings.Environment,
            };

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
                new Payload(this._config.AccessToken, new Data(
                    this._config, 
                    new Body(new Message("A message I wish to send to the rollbar overlords", new Dictionary<string, object>() {{"longMessageString", "very-long-string-very-long-string-very-long-" }, {"theMessageNumber", 11 }, })),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
                new Payload(this._config.AccessToken, new Data(
                    this._config, 
                    new Body("A terrible crash!"),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
                new Payload(this._config.AccessToken, new Data(
                    this._config, 
                    new Body(GetException()),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
                new Payload(this._config.AccessToken, new Data(
                    this._config, 
                    new Body(GetAggregateException()),
                    new Dictionary<string, object>() {{"longDataString", "long-string-very-long-string-very-long-" }, {"theDataNumber", 15 }, })
                    ),
            };

            TimeSpan blockingTimeout = TimeSpan.FromSeconds(5);
            using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            {
                foreach(var payload in testPayloads)
                {
                    logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
                }
            }

            IterativeTruncationStrategy trancationStrategy = null;

            const int payloadByteSizeLimit = 10; //we are intentionally exaggerating here to force all the iterations to happen...
            List<IPayloadTruncationStrategy> iterations = new List<IPayloadTruncationStrategy>();

            iterations.Add(new RawTruncationStrategy());
            trancationStrategy = new IterativeTruncationStrategy(400, iterations);
            AssertPayloadSizeReduction(false, testPayloads, trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, trancationStrategy);
            using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            {
                foreach (var payload in testPayloads)
                {
                    logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
                }
            }

            iterations.Add(new FramesTruncationStrategy(totalHeadFramesToPreserve: 1, totalTailFramesToPreserve: 1));
            trancationStrategy = new IterativeTruncationStrategy(payloadByteSizeLimit, iterations);
            AssertPayloadSizeReduction(false, testPayloads.Take(3).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads.Take(3).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(true, testPayloads.Reverse().Take(1).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads.Reverse().Take(1).ToArray(), trancationStrategy);
            using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            {
                foreach (var payload in testPayloads)
                {
                    logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
                }
            }

            iterations.Add(new StringsTruncationStrategy(stringBytesLimit: 10));
            trancationStrategy = new IterativeTruncationStrategy(payloadByteSizeLimit, iterations);
            AssertPayloadSizeReduction(true, testPayloads, trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, trancationStrategy);
            using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            {
                foreach (var payload in testPayloads)
                {
                    logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
                }
            }

            iterations.Add(new StringsTruncationStrategy(stringBytesLimit:  7));
            trancationStrategy = new IterativeTruncationStrategy(payloadByteSizeLimit, iterations);
            AssertPayloadSizeReduction(true, testPayloads, trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, trancationStrategy);
            using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            {
                foreach (var payload in testPayloads)
                {
                    logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
                }
            }

            iterations.Add(new StringsTruncationStrategy(stringBytesLimit:  3));
            trancationStrategy = new IterativeTruncationStrategy(payloadByteSizeLimit, iterations);
            AssertPayloadSizeReduction(true, testPayloads, trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, trancationStrategy);
            using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            {
                foreach (var payload in testPayloads)
                {
                    logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
                }
            }

            iterations.Add(new MinBodyTruncationStrategy());
            trancationStrategy = new IterativeTruncationStrategy(payloadByteSizeLimit, iterations);
            AssertPayloadSizeReduction(false, testPayloads.Take(2).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads.Take(2).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(true, testPayloads.Reverse().Take(2).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads.Reverse().Take(2).ToArray(), trancationStrategy);
            using (var logger = RollbarFactory.CreateNew().Configure(this._config))
            {
                foreach (var payload in testPayloads)
                {
                    logger.AsBlockingLogger(blockingTimeout).Log(payload.Data);
                }
            }
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

                if (expectReduction)
                {
                    Assert.IsTrue(truncated.Length < original.Length);
                }
                else
                {
                    Assert.IsTrue(truncated.Length == original.Length);
                }
            }
        }

        private static AggregateException GetAggregateException()
        {
            try
            {
                Parallel.ForEach(new[] { 1, 2, 3, 4, 5 }, i => ThrowAnException());
            }
            catch (AggregateException e)
            {
                return e;
            }

            throw new System.Exception("Unreachable");
        }

        private static void ThrowAnException()
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
