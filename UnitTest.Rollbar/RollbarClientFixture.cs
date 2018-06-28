#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Deploys;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net.Http;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarClientFixture))]
    public class RollbarClientFixture
    {

        private RollbarConfig _loggerConfig;

        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            this._loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

    }
}
