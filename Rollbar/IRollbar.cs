namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IRollbar
        : ILogger
    {
        IRollbar Configure(RollbarConfig settings);
        IRollbar Configure(string accessToken);

        RollbarConfig CloneConfiguration();

        ILogger Logger { get; }
    }
}
