namespace Rollbar.Deploys
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Class RollbarDeploysManagerFactory.
    /// </summary>
    public static class RollbarDeploysManagerFactory
    {
        /// <summary>
        /// Creates the rollbar deploys manager.
        /// </summary>
        /// <param name="writeAccessToken">The write access token.</param>
        /// <param name="readAccessToken">The read access token.</param>
        /// <returns>IRollbarDeploysManager.</returns>
        public static IRollbarDeploysManager CreateRollbarDeploysManager(
            string writeAccessToken, 
            string readAccessToken
            )
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return new RollbarDeploysManager(
                writeAccessToken: writeAccessToken, 
                readAccessToken: readAccessToken
                );
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
