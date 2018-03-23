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
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void TestTruncation()
        {

            Dictionary<string, object> customAttributes = new Dictionary<string, object>()
            {
                {"longString", "very-long-string-very-long-string-very-long-string-very-long-string-very-long-string-very-long-string-very-long-string-very-long-string-very-long-string-" },
                {"theNumber", 11 },
            };

            Payload[] testPayloads = new Payload[]
            {
                new Payload(this._config.AccessToken, new Data(this._config, new Body(new Message("A message I wish to send to the rollbar overlords", customAttributes)), customAttributes)),
                new Payload(this._config.AccessToken, new Data(this._config, new Body("A terrible crash!"), customAttributes)),
                new Payload(this._config.AccessToken, new Data(this._config, new Body(GetException()), customAttributes)),
                new Payload(this._config.AccessToken, new Data(this._config, new Body(GetAggregateException()), customAttributes)),
            };

            IterativeTruncationStrategy trancationStrategy = null;

            List<IPayloadTruncationStrategy> iterations = new List<IPayloadTruncationStrategy>();

            iterations.Add(new RawTruncationStrategy());
            trancationStrategy = new IterativeTruncationStrategy(400, iterations);
            AssertPayloadSizeReduction(false, testPayloads, trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, trancationStrategy);

            iterations.Add(new FramesTruncationStrategy(totalHeadFramesToPreserve: 1, totalTailFramesToPreserve: 1));
            trancationStrategy = new IterativeTruncationStrategy(400, iterations);
            AssertPayloadSizeReduction(false, testPayloads.Take(3).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads.Take(3).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(true, testPayloads.Reverse().Take(1).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads.Reverse().Take(1).ToArray(), trancationStrategy);

            iterations.Add(new StringsTruncationStrategy(stringBytesLimit: 10));
            trancationStrategy = new IterativeTruncationStrategy(400, iterations);
            AssertPayloadSizeReduction(true, testPayloads, trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, trancationStrategy);

            iterations.Add(new StringsTruncationStrategy(stringBytesLimit:  7));
            trancationStrategy = new IterativeTruncationStrategy(400, iterations);
            AssertPayloadSizeReduction(true, testPayloads, trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, trancationStrategy);

            iterations.Add(new StringsTruncationStrategy(stringBytesLimit:  3));
            trancationStrategy = new IterativeTruncationStrategy(400, iterations);
            AssertPayloadSizeReduction(true, testPayloads, trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads, trancationStrategy);

            iterations.Add(new MinBodyTruncationStrategy());
            trancationStrategy = new IterativeTruncationStrategy(400, iterations);
            AssertPayloadSizeReduction(false, testPayloads.Take(2).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads.Take(2).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(true, testPayloads.Reverse().Take(2).ToArray(), trancationStrategy);
            AssertPayloadSizeReduction(false, testPayloads.Reverse().Take(2).ToArray(), trancationStrategy);
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
