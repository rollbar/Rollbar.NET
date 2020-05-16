namespace Rollbar.Deploys
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the Rollbar Deploy Manager's interface.
    /// </summary>
    public interface IRollbarDeploysManager
    {
        /// <summary>
        /// Registers the deployment asynchronously.
        /// </summary>
        /// <param name="deployment">The deployment.</param>
        /// <returns></returns>
        Task RegisterAsync(IDeployment deployment);

        /// <summary>
        /// Gets the deployment asynchronously.
        /// </summary>
        /// <param name="deploymentID">The deployment identifier.</param>
        /// <returns></returns>
        Task<IDeploymentDetails> GetDeploymentAsync(string deploymentID);

        /// <summary>
        /// Gets the deployments page asynchronously.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <returns></returns>
        Task<IDeploymentDetails[]> GetDeploymentsPageAsync(string environment, int pageNumber);
    }
}
