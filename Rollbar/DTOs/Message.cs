namespace Rollbar.DTOs
{
    using System.Collections.Generic;
    using Rollbar.Diagnostics;

    public class Message 
        : ExtendableDtoBase
    {
        public static class ReservedProperties
        {
            public const string Body = "body";
        }

        public Message(string body, IDictionary<string, object> arbitraryKeyValuePairs = null)
            : base(arbitraryKeyValuePairs)
        {
            Body = body;

            Validate();
        }

        public string Body
        {
            get { return this._keyedValues[ReservedProperties.Body] as string; }
            private set { this._keyedValues[ReservedProperties.Body] = value; }
        }

        public override void Validate()
        {
            Assumption.AssertNotNullOrWhiteSpace(this.Body, nameof(this.Body));
        }

    }
}
