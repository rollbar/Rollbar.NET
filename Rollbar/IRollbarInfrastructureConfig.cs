namespace Rollbar
{
    using System;

    using Rollbar.Common;

    public interface IRollbarInfrastructureConfig
        : IReconfigurable<IRollbarInfrastructureConfig, IRollbarInfrastructureConfig>
        , IEquatable<IRollbarInfrastructureConfig>
        , ITraceable
    {
        IRollbarInfrastructureOptions RollbarInfrastructureOptions
        {
            get;
        }

        IRollbarOfflineStoreOptions RollbarOfflineStoreOptions
        {
            get;
        }

        IRollbarLoggerConfig RollbarLoggerConfig
        {
            get;
        }

    }
}
