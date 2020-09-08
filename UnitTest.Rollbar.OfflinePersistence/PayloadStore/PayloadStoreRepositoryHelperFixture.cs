#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.PayloadStore
{
    using global::Rollbar.OfflinePersistence;
    using global::Rollbar.PayloadStore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    [TestCategory(nameof(PayloadStoreRepositoryHelperFixture))]
    public class PayloadStoreRepositoryHelperFixture
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
        public void GetRespositoryWorksFine()
        {
            var repos = PayloadStoreRepositoryHelper.CreatePayloadStoreRepository();
            Assert.IsInstanceOfType(repos, typeof(PayloadStoreRepository));
        }
    }
}
