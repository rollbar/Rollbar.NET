namespace Rollbar.Deploys
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Rollbar.Diagnostics;

    /// <summary>
    /// Implements IRollbarDeploysManager.
    /// </summary>
    /// <seealso cref="Rollbar.Deploys.IRollbarDeploysManager" />
    internal class RollbarDeploysManager
            : IRollbarDeploysManager
    {
        private readonly string _writeAccessToken;
        private readonly string _readAccessToken;

        private RollbarDeploysManager()
        {
            this._readAccessToken = string.Empty;
            this._writeAccessToken = string.Empty;
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

            RollbarDestinationOptions destinationOptions = new(this._writeAccessToken, deployment.Environment);
            var config = new RollbarLoggerConfig();
            config.RollbarDestinationOptions.Reconfigure(destinationOptions);

            using HttpClient httpClient = new();
            RollbarDeployClient rollbarClient = new(config, httpClient);
            await rollbarClient.PostAsync(deployment);
        }

        /// <summary>
        /// Gets the deployment asynchronously.
        /// </summary>
        /// <param name="deploymentID">The deployment identifier.</param>
        /// <returns></returns>
        public async Task<IDeploymentDetails?> GetDeploymentAsync(string deploymentID)
        {
            Assumption.AssertNotNullOrWhiteSpace(deploymentID, nameof(deploymentID));
            Assumption.AssertNotNullOrWhiteSpace(this._readAccessToken, nameof(this._readAccessToken));

            var config = new RollbarLoggerConfig(this._readAccessToken);

            using HttpClient httpClient = new();
            RollbarDeployClient rollbarClient = new(config,httpClient);
            var result = await rollbarClient.GetDeploymentAsync(this._readAccessToken, deploymentID);
            return result?.Deploy;
        }

        /// <summary>
        /// Gets the deployments page asynchronously.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <returns></returns>
        public async Task<IDeploymentDetails[]?> GetDeploymentsPageAsync(string environment, int pageNumber)
        {
            Assumption.AssertNotNullOrWhiteSpace(this._readAccessToken, nameof(this._readAccessToken));

            var config = new RollbarLoggerConfig(this._readAccessToken);

            using HttpClient httpClient = new();
            RollbarDeployClient rollbarClient = new(config, httpClient);
            var result = await rollbarClient.GetDeploymentsAsync(this._readAccessToken, pageNumber).ConfigureAwait(false);
            return result?.DeploysPage?.Deploys;
        }
    }
}
