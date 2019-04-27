namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    /// <summary>
    /// Defines test class AccessTokenQueuesMetadataFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(AccessTokenQueuesMetadataFixture))]
    public class AccessTokenQueuesMetadataFixture
    {
        /// <summary>
        /// Setups the fixture.
        /// </summary>
        [TestInitialize]
        public void SetupFixture()
        {
        }

        /// <summary>
        /// Tears down fixture.
        /// </summary>
        [TestCleanup]
        public void TearDownFixture()
        {

        }

        /// <summary>
        /// Defines the test method ConstructionTest.
        /// </summary>
        [TestMethod]
        public void ConstructionTest()
        {
            var metadata = new AccessTokenQueuesMetadata(RollbarUnitTestSettings.AccessToken);

            Assert.AreEqual(RollbarUnitTestSettings.AccessToken, metadata.AccessToken);
            Assert.IsNotNull(metadata.Queues);
            Assert.IsTrue(metadata.NextTimeTokenUsage < DateTimeOffset.Now);
            Assert.AreEqual(TimeSpan.Zero, metadata.TokenUsageDelay);
        }

        /// <summary>
        /// Defines the test method DelayIncrementTest.
        /// </summary>
        [TestMethod]
        public void DelayIncrementTest()
        {
            var metadata = new AccessTokenQueuesMetadata(RollbarUnitTestSettings.AccessToken);

            Assert.IsTrue(metadata.NextTimeTokenUsage < DateTimeOffset.Now);
            Assert.AreEqual(TimeSpan.Zero, metadata.TokenUsageDelay);

            metadata.IncrementTokenUsageDelay();
            Assert.IsTrue(metadata.NextTimeTokenUsage > DateTimeOffset.Now);
            Assert.IsTrue(TimeSpan.Zero < metadata.TokenUsageDelay);

            var nextTimeUsage = metadata.NextTimeTokenUsage;
            var usageDelay = metadata.TokenUsageDelay;
            metadata.IncrementTokenUsageDelay();
            Assert.IsTrue(metadata.NextTimeTokenUsage > nextTimeUsage);
            Assert.IsTrue(usageDelay < metadata.TokenUsageDelay);
        }

        /// <summary>
        /// Defines the test method DelayResetTest.
        /// </summary>
        [TestMethod]
        public void DelayResetTest()
        {
            var metadata = new AccessTokenQueuesMetadata(RollbarUnitTestSettings.AccessToken);

            Assert.IsTrue(metadata.NextTimeTokenUsage < DateTimeOffset.Now);
            Assert.AreEqual(TimeSpan.Zero, metadata.TokenUsageDelay);

            metadata.IncrementTokenUsageDelay();
            Assert.IsTrue(metadata.NextTimeTokenUsage > DateTimeOffset.Now);
            Assert.IsTrue(TimeSpan.Zero < metadata.TokenUsageDelay);

            metadata.ResetTokenUsageDelay();
            //Assert.IsTrue(metadata.NextTimeTokenUsage <= DateTimeOffset.Now);
            Assert.AreEqual(TimeSpan.Zero, metadata.TokenUsageDelay);
        }
    }
}
