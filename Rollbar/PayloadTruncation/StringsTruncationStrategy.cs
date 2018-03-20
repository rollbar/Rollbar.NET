namespace Rollbar.PayloadTruncation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rollbar.DTOs;

    internal class StringsTruncationStrategy
        : PayloadTruncationStrategyBase
    {
        public override int Truncate(Payload payload)
        {
            throw new NotImplementedException();
        }
    }
}
