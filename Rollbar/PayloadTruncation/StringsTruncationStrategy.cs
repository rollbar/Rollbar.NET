namespace Rollbar.PayloadTruncation
{
    using System.Text;
    using Rollbar.DTOs;

    internal class StringsTruncationStrategy
        : PayloadTruncationStrategyBase
    {
        private readonly int _stringBytesLimit; //from higher to lower threshold value...

        private StringsTruncationStrategy()
        {

        }

        public StringsTruncationStrategy(int stringBytesLimit)
        {
            this._stringBytesLimit = stringBytesLimit;
        }

        public override int Truncate(Payload payload)
        {
            payload.TruncateStrings(Encoding.UTF8, _stringBytesLimit);

            return this.GetSizeInBytes(payload);
        }
    }
}
