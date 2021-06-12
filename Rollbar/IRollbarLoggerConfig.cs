namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    public interface IRollbarLoggerConfig
        : IReconfigurable<IRollbarLoggerConfig, IRollbarLoggerConfig>
        , IEquatable<IRollbarLoggerConfig>
        , ITraceable
    {
        IRollbarDestinationOptions RollbarDestinationOptions { get; }
        IHttpProxyOptions HttpProxyOptions { get; }
        IRollbarDeveloperOptions RollbarDeveloperOptions { get; }
        IRollbarDataSecurityOptions RollbarDataSecurityOptions { get; }
        IRollbarPayloadAdditionOptions RollbarPayloadAdditionOptions { get; }
        IRollbarPayloadManipulationOptions RollbarPayloadManipulationOptions { get; }
        IRollbarInfrastructureOptions RollbarInfrastructureOptions { get; }
    }
}
