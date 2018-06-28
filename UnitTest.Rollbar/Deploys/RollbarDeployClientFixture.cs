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

        [TestMethod]
        public void TestGetDeploysPage()
        {
            using (var httpClient = new HttpClient())
            {
                RollbarDeployClient rollbarClient = new RollbarDeployClient(this._loggerConfig, httpClient);

                var task = rollbarClient.GetDeploymentsAsync(RollbarUnitTestSettings.DeploymentsReadAccessToken, 1);

                task.Wait(TimeSpan.FromSeconds(3));
                Assert.IsNotNull(task.Result);
                Assert.AreEqual(task.Result.ErrorCode, 0);
                Assert.IsNotNull(task.Result.DeploysPage);
                Assert.IsTrue(task.Result.DeploysPage.PageNumber > 0);
            }

        }

        [TestMethod]
        public void TestGetDeploy()
        {
            using (var httpClient = new HttpClient())
            {
                RollbarDeployClient rollbarClient = new RollbarDeployClient(this._loggerConfig, httpClient);

                var task = rollbarClient.GetDeploymentAsync(RollbarUnitTestSettings.DeploymentsReadAccessToken, "8387647");

                task.Wait(TimeSpan.FromSeconds(3));
                Assert.IsNotNull(task.Result);
                Assert.AreEqual(task.Result.ErrorCode, 0);
                Assert.IsNotNull(task.Result.Deploy);
                Assert.IsTrue(!string.IsNullOrWhiteSpace(task.Result.Deploy.DeployID));
            }

        }

        [TestMethod]
        public void TestPostDeployment()
        {
            using (var httpClient = new HttpClient())
            {
                RollbarDeployClient rollbarClient = new RollbarDeployClient(this._loggerConfig, httpClient);

                var deployment = new Deployment(this._loggerConfig.AccessToken, this._loggerConfig.Environment, "99909a3a5a3dd4363f414161f340b582bb2e4161")
                {
                    Comment = "Some new unit test deployment",
                    LocalUsername = "UnitTestRunner",
                    RollbarUsername = "rollbar",
                };

                var task = rollbarClient.PostAsync(deployment);

                task.Wait(TimeSpan.FromSeconds(3));
                Assert.IsNull(task.Exception);
            }
        }
    }
}
