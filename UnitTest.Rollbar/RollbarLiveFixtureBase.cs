#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarLiveFixtureBase))]
    public abstract class RollbarLiveFixtureBase
        : IDisposable
    {
        private RollbarConfig _loggerConfig;

        protected RollbarLiveFixtureBase()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            this._loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
        }

        protected IRollbarConfig ProvideLiveRollbarConfig()
        {
            return this.ProvideLiveRollbarConfig(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment);
        }

        protected IRollbarConfig ProvideLiveRollbarConfig(string rollbarAccessToken, string rollbarEnvironment)
        {
            if (this._loggerConfig == null)
            {
                this._loggerConfig =
                    new RollbarConfig(rollbarAccessToken) { Environment = rollbarEnvironment, };
            }
            return this._loggerConfig;
        }

        protected IRollbar ProvideDisposableRollbar()
        {
            return RollbarFactory.CreateNew(isSingleton: false, rollbarConfig: this.ProvideLiveRollbarConfig());
        }

        protected IRollbar ProvideSharedRollbar()
        {
            if (!RollbarLocator.RollbarInstance.Equals(ProvideLiveRollbarConfig()))
            {
                RollbarLocator.RollbarInstance.Configure(ProvideLiveRollbarConfig());
            }
            return RollbarLocator.RollbarInstance;
        }

        //[TestInitialize]
        //public void SetupFixture()
        //{
        //}

        //[TestCleanup]
        //public void TearDownFixture()
        //{
        //}

        //[TestMethod]
        //public void SomeTestMethod()
        //{
        //}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarLiveFixtureBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
