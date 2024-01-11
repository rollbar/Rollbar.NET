#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.Deploys
{
    using global::Rollbar;
    using global::Rollbar.Deploys;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarDeploysManagerFixture))]
    public class RollbarDeploysManagerFixture
    {
        private readonly IRollbarDeploysManager _deploysManager =
            RollbarDeploysManagerFactory.CreateRollbarDeploysManager(
                RollbarUnitTestSettings.AccessToken,
                RollbarUnitTestSettings.DeploymentsReadAccessToken
                );

        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        private ICollection<IDeploymentDetails> GetAllDeployments()
        {
            List<IDeploymentDetails> deployments = new List<IDeploymentDetails>();

            int pageCount = 0;
            int pageItems;
            do
            {
                var task = this._deploysManager.GetDeploymentsPageAsync(RollbarUnitTestSettings.Environment, ++pageCount);
                task.Wait(TimeSpan.FromSeconds(3));
                pageItems = task.Result.Length;
                if (pageItems > 0)
                {
                    deployments.AddRange(task.Result);
                }
                Assert.IsNull(task.Exception);
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
            }
            while (pageItems > 0);

            return deployments;
        }

        [Ignore]
        [TestMethod]
        public void TestRollbarDeploysManager()
        {
            var initialDeployments = this.GetAllDeployments();

            var deployment = DeploymentFactory.CreateDeployment(
                    environment: RollbarUnitTestSettings.Environment,
                    revision: "99909a3a5a3dd4363f414161f340b582bb2e4161",
                    comment: "Some new unit test deployment @ " + DateTimeOffset.Now,
                    localUserName: "UnitTestRunner",
                    rollbarUserName: "rollbar"
                    );

            var task = this._deploysManager.RegisterAsync(deployment);
            task.Wait(TimeSpan.FromSeconds(3));
            Assert.IsNull(task.Exception);
            Thread.Sleep(TimeSpan.FromMilliseconds(250));

            var deployments = this.GetAllDeployments();

            Trace.WriteLine($"Initial deployments count : {initialDeployments.Count}");
            Trace.WriteLine($"Deployments count : {deployments.Count}");
            Assert.IsTrue(initialDeployments.Count < deployments.Count);

            var latestDeployment = deployments.FirstOrDefault();
            Assert.IsNotNull(latestDeployment);

            var getDeploymentTask = this._deploysManager.GetDeploymentAsync(latestDeployment.DeployID);
            getDeploymentTask.Wait(TimeSpan.FromSeconds(3));
            Assert.IsNull(getDeploymentTask.Exception);
            var deploymentDetails = getDeploymentTask.Result;
            Assert.IsNotNull(getDeploymentTask);
            Assert.AreEqual(deploymentDetails.Comment, latestDeployment.Comment);
            Assert.AreEqual(deploymentDetails.DeployID, latestDeployment.DeployID);
            Assert.AreEqual(deploymentDetails.EndTime, latestDeployment.EndTime);
            Assert.AreEqual(deploymentDetails.Environment, latestDeployment.Environment);
            Assert.AreEqual(deploymentDetails.LocalUsername, latestDeployment.LocalUsername);
            Assert.AreEqual(deploymentDetails.ProjectID, latestDeployment.ProjectID);
            Assert.AreEqual(deploymentDetails.Revision, latestDeployment.Revision);
            Assert.AreEqual(deploymentDetails.RollbarUsername, latestDeployment.RollbarUsername);
            Assert.AreEqual(deploymentDetails.StartTime, latestDeployment.StartTime);
        }
    }
}
