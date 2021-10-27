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
            Assert.IsNotNull(metadata.GetPayloadQueues);
            Assert.IsTrue(metadata.NextTimeTokenUsage <= DateTimeOffset.Now);
        }

    }
}
