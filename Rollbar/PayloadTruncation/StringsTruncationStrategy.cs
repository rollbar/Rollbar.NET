namespace Rollbar.PayloadTruncation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rollbar.DTOs;

    internal class StringsTruncationStrategy
        : PayloadTruncationStrategyBase
    {
        private readonly int[] _sortedThresholds; //from higher to lower threshold value...

        private StringsTruncationStrategy()
        {

        }

        public StringsTruncationStrategy(int[] descendingStringLengthThresholds)
        {
            this._sortedThresholds = descendingStringLengthThresholds;
        }

        public override int Truncate(Payload payload)
        {
            throw new NotImplementedException();
        }
    }
}
