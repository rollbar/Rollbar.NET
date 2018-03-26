#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Deploys;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarClientFixture))]
    public class RollbarClientFixture
    {

        private IRollbarConfig _loggerConfig;

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

        //There is nothing really to unit test regarding the RollbarClient type.
        //It does not have any processing logic at all and just makes a couple of 
        //very straightforward into .NET type and Newtonsoft.Json SDKs.

        [TestMethod]
        public void TestGetDeploysPage()
        {
            RollbarClient rollbarClient = new RollbarClient(this._loggerConfig);

            //var result = 
            rollbarClient.GetDeploymentsAsync(RollbarUnitTestSettings.DeploymentsReadAccessToken, 1).Wait();

            rollbarClient.GetDeploymentsAsync(RollbarUnitTestSettings.DeploymentsReadAccessToken, int.MaxValue).Wait();
        }

        [TestMethod]
        public void TestGetDeploy()
        {
            RollbarClient rollbarClient = new RollbarClient(this._loggerConfig);

            //var result = 
            rollbarClient.GetDeploymentAsync(RollbarUnitTestSettings.DeploymentsReadAccessToken, "8387647").Wait();
        }

        [TestMethod]
        public void TestPostDeployment()
        {
            RollbarClient rollbarClient = new RollbarClient(this._loggerConfig);

            var deployment = new Deployment(this._loggerConfig.AccessToken, this._loggerConfig.Environment, "99909a3a5a3dd4363f414161f340b582bb2e4161") {
                Comment = "Some new unit test deployment",
                LocalUsername = "UnitTestRunner",
                RollbarUsername = "rollbar",
            };

            //var result = 
            rollbarClient.PostAsync(deployment).Wait();
        }
    }
}
