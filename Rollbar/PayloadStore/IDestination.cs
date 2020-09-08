using System;
using System.Collections.Generic;

namespace Rollbar.PayloadStore
{
    public interface IDestination
    {
        string AccessToken { get; set; }
        string Endpoint { get; set; }
        Guid ID { get; set; }
    }
}