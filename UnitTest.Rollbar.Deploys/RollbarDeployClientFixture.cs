#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.Deploys
{
    using global::Rollbar;
    using global::Rollbar.Deploys;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net.Http;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarDeployClientFixture))]
    public class RollbarDeployClientFixture
    {
        private RollbarLoggerConfig _loggerConfig;

        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(
                    RollbarUnitTestSettings.AccessToken,
                    RollbarUnitTestSettings.Environment
                    );
            this._loggerConfig =
                new RollbarLoggerConfig();
            this._loggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [Ignore]
        [TestMethod]
        public void TestGetDeploysPage()
        {
            using var httpClient = new HttpClient();
            RollbarDeployClient rollbarClient = new RollbarDeployClient(this._loggerConfig,httpClient);

            var task = rollbarClient.GetDeploymentsAsync(RollbarUnitTestSettings.DeploymentsReadAccessToken,1);

            task.Wait(TimeSpan.FromSeconds(3));
            Assert.IsNotNull(task.Result);
            Assert.AreEqual(0, task.Result.ErrorCode);
            Assert.IsNotNull(task.Result.DeploysPage);
            Assert.IsTrue(task.Result.DeploysPage.PageNumber > 0);

        }

        [Ignore]
        [TestMethod]
        public void TestGetDeploy()
        {
            using var httpClient = new HttpClient();
            RollbarDeployClient rollbarClient = new RollbarDeployClient(this._loggerConfig,httpClient);

            var task = rollbarClient.GetDeploymentAsync(RollbarUnitTestSettings.DeploymentsReadAccessToken, "23838256");

            task.Wait(TimeSpan.FromSeconds(3));
            Assert.IsNotNull(task.Result);
            Assert.AreEqual(0, task.Result.ErrorCode);
            Assert.IsNotNull(task.Result.Deploy);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(task.Result.Deploy.DeployID));

        }

        [Ignore]
        [TestMethod]
        public void TestPostDeployment()
        {
            using var httpClient = new HttpClient();
            RollbarDeployClient rollbarClient = new RollbarDeployClient(this._loggerConfig,httpClient);

            var deployment = DeploymentFactory.CreateDeployment(
                environment: this._loggerConfig.RollbarDestinationOptions.Environment,
                revision: "99909a3a5a3dd4363f414161f340b582bb2e4161",
                comment: "Some new unit test deployment",
                localUserName: "UnitTestRunner",
                rollbarUserName: "rollbar",
                writeAccessToken: RollbarUnitTestSettings.DeploymentsWriteAccessToken
                );
            var task = rollbarClient.PostAsync(deployment);

            task.Wait(TimeSpan.FromSeconds(3));
            Assert.IsNull(task.Exception);
        }
    }
}
