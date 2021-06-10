namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using Rollbar.Common;

    public class RollbarInfrastructureConfig
        : ReconfigurableBase<RollbarInfrastructureConfig, IRollbarInfrastructureConfig>
        , IRollbarInfrastructureConfig

    {
        public override Validator GetValidator()
        {
            return null;
        }

        public IRollbarLoggerConfig Reconfigure(IRollbarLoggerConfig likeMe)
        {
            throw new NotImplementedException();
        }

        public IRollbarInfrastructureConfig Reconfigure(IRollbarInfrastructureConfig likeMe)
        {
            return base.Reconfigure(likeMe);
        }
    }
}
