#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarFactoryFixture))]
    public class RollbarFactoryFixture
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
        public void ProducesValidInstances()
        {
            var rollbar1 = RollbarFactory.CreateNew();
            Assert.IsNotNull(rollbar1);

            var rollbarLogger = rollbar1 as RollbarLogger;
            Assert.IsNotNull(rollbarLogger);
            Assert.IsFalse(rollbarLogger.IsSingleton);

        }

        [TestMethod]
        public void ProducesUniqueInstances()
        {
            var rollbar1 = RollbarFactory.CreateNew();
            Assert.IsNotNull(rollbar1);

            var rollbar2 = RollbarFactory.CreateNew();
            Assert.IsNotNull(rollbar2);

            Assert.AreNotSame(rollbar2, rollbar1);
        }
    }
}
