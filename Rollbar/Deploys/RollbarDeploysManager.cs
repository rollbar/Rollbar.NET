namespace Rollbar.Deploys
{
    using Rollbar.Diagnostics;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rollbar.Deploys.IRollbarDeploysManager" />
    internal class RollbarDeploysManager
            : IRollbarDeploysManager
    {
        private readonly string _writeAccessToken;
        private readonly string _readAccessToken;

        private RollbarDeploysManager()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarDeploysManager"/> class.
        /// </summary>
        /// <param name="writeAccessToken">The write access token.</param>
        /// <param name="readAccessToken">The read access token.</param>
        public RollbarDeploysManager(string writeAccessToken, string readAccessToken)
        {
            Assumption.AssertFalse(
                string.IsNullOrWhiteSpace(writeAccessToken) && string.IsNullOrWhiteSpace(readAccessToken),
                nameof(writeAccessToken) + "and" + nameof(readAccessToken)
                );

            this._writeAccessToken = writeAccessToken;
            this._readAccessToken = readAccessToken;
        }

        /// <summary>
        /// Registers the deployment asynchronously.
        /// </summary>
        /// <param name="deployment">The deployment.</param>
        /// <returns></returns>
        public async Task RegisterAsync(IDeployment deployment)
        {
            Assumption.AssertNotNullOrWhiteSpace(this._writeAccessToken, nameof(this._writeAccessToken));

            var config =
               new RollbarConfig(this._writeAccessToken) { Environment = deployment.Environment, };

            RollbarClient rollbarClient = new RollbarClient(config);

            await rollbarClient.PostAsync(deployment);
        }

        /// <summary>
        /// Gets the deployment asynchronously.
        /// </summary>
        /// <param name="deploymentID">The deployment identifier.</param>
        /// <returns></returns>
        public async Task<IDeploymentDetails> GetDeploymentAsync(string deploymentID)
        {
            Assumption.AssertNotNullOrWhiteSpace(deploymentID, nameof(deploymentID));
            Assumption.AssertNotNullOrWhiteSpace(this._readAccessToken, nameof(this._readAccessToken));

            var config = new RollbarConfig(this._readAccessToken);
            RollbarClient rollbarClient = new RollbarClient(config);

            var result = await rollbarClient.GetDeploymentAsync(this._readAccessToken, deploymentID);

            return result.Deploy;
        }

        /// <summary>
        /// Gets the deployments page asynchronously.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <returns></returns>
        public async Task<IDeploymentDetails[]> GetDeploymentsPageAsync(string environment, int pageNumber)
        {
            Assumption.AssertNotNullOrWhiteSpace(this._readAccessToken, nameof(this._readAccessToken));

            var config = new RollbarConfig(this._readAccessToken);
            RollbarClient rollbarClient = new RollbarClient(config);

            var result = await rollbarClient.GetDeploymentsAsync(this._readAccessToken, pageNumber);

            return result.DeploysPage.Deploys;
        }
    }
}
