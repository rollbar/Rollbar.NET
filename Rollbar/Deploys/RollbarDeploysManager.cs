namespace Rollbar.Deploys
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class RollbarDeploysManager
        : IRollbarDeploysManager
    {
        private readonly string _writeAccessToken;

        private RollbarDeploysManager()
        {

        }

        public RollbarDeploysManager(string writeAccessToken)
        {
            this._writeAccessToken = writeAccessToken;
        }

        public void Register(Deployment deployment)
        {
            throw new NotImplementedException();
        }
    }
}
