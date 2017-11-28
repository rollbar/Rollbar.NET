namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    [TestCategory("AccessTokenQueuesMetadataFixture")]
    public class AccessTokenQueuesMetadataFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        [TestMethod]
        public void ConstructionTest()
        {
            var metadata = new AccessTokenQueuesMetadata(RollbarUnitTestSettings.AccessToken);

            Assert.AreEqual(RollbarUnitTestSettings.AccessToken, metadata.AccessToken);
            Assert.IsNotNull(metadata.Queues);
            Assert.IsFalse(metadata.NextTimeTokenUsage.HasValue);
            Assert.AreEqual(TimeSpan.Zero, metadata.TokenUsageDelay);
        }

        [TestMethod]
        public void DelayIncrementTest()
        {
            var metadata = new AccessTokenQueuesMetadata(RollbarUnitTestSettings.AccessToken);

            Assert.IsFalse(metadata.NextTimeTokenUsage.HasValue);
            Assert.AreEqual(TimeSpan.Zero, metadata.TokenUsageDelay);

            metadata.IncrementTokenUsageDelay();
            Assert.IsTrue(metadata.NextTimeTokenUsage.HasValue);
            Assert.IsTrue(TimeSpan.Zero < metadata.TokenUsageDelay);

            var nextTimeUsage = metadata.NextTimeTokenUsage.Value;
            var usageDelay = metadata.TokenUsageDelay;
            metadata.IncrementTokenUsageDelay();
            Assert.IsTrue(metadata.NextTimeTokenUsage.Value > nextTimeUsage);
            Assert.IsTrue(usageDelay < metadata.TokenUsageDelay);
        }

        [TestMethod]
        public void DelayResetTest()
        {
            var metadata = new AccessTokenQueuesMetadata(RollbarUnitTestSettings.AccessToken);

            Assert.IsFalse(metadata.NextTimeTokenUsage.HasValue);
            Assert.AreEqual(TimeSpan.Zero, metadata.TokenUsageDelay);

            metadata.IncrementTokenUsageDelay();
            Assert.IsTrue(metadata.NextTimeTokenUsage.HasValue);
            Assert.IsTrue(TimeSpan.Zero < metadata.TokenUsageDelay);

            metadata.ResetTokenUsageDelay();
            Assert.IsFalse(metadata.NextTimeTokenUsage.HasValue);
            Assert.AreEqual(TimeSpan.Zero, metadata.TokenUsageDelay);
        }
    }
}
