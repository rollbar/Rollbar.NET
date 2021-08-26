namespace Rollbar.Deploys
{

    /// <summary>
    /// Class DeploymentFactory.
    /// </summary>
    public static class DeploymentFactory
    {
        /// <summary>
        /// Creates the deployment.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="revision">The revision.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="localUserName">Name of the local user.</param>
        /// <param name="rollbarUserName">Name of the rollbar user.</param>
        /// <param name="writeAccessToken">The write access token.</param>
        /// <returns>IDeployment.</returns>
        public static IDeployment CreateDeployment(
            string environment, 
            string revision, 
            string? comment = null,
            string? localUserName = null, 
            string? rollbarUserName = null, 
            string? writeAccessToken = null
            )
        {
#pragma warning disable CS0618 // Type or member is obsolete
//#pragma warning disable CS0618 // Type or member is obsolete
            return new Deployment(
//#pragma warning restore CS0618 // Type or member is obsolete
                writeAccessToken: writeAccessToken, 
                environment: environment, 
                revision: revision
                ) 
                {
                    Comment = comment, 
                    LocalUsername = localUserName, 
                    RollbarUsername = rollbarUserName, 
                };
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
