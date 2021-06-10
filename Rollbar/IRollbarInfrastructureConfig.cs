namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    public interface IRollbarInfrastructureConfig
        : IReconfigurable<IRollbarInfrastructureConfig, IRollbarInfrastructureConfig>
        , IEquatable<IRollbarInfrastructureConfig>
        , ITraceable
    {
        IRollbarLoggerConfig RollbarLoggerConfig
        {
            get;
        }

        IRollbarInfrastructureOptions RollbarInfrastructureOptions
        {
            get;
        }
    }
}
