using System;

namespace Rollbar.PayloadStore
{
    public interface IPayloadRecord
    {
        string ConfigJson { get; set; }
        Guid ID { get; set; }
        string PayloadJson { get; set; }
        DateTime Timestamp { get; set; }
    }
}