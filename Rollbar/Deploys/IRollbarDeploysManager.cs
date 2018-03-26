using System;
using System.Collections.Generic;
using System.Text;

namespace Rollbar.Deploys
{
    public interface IRollbarDeploysManager
    {
        void Register(Deployment deployment);

    }
}
