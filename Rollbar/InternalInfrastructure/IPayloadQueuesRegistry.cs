namespace Rollbar.InternalInfrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal interface IPayloadQueuesRegistry
    {
        void Register(PayloadQueue queue);
        void Unregister(PayloadQueue queue);
        IReadOnlyCollection<PayloadBundle> PayloadQueues { get; }
        int PayloadQueuesCount { get; }
    }
}
