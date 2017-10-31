namespace Rollbar.DTOs
{
    using System;
    using Newtonsoft.Json;
    using Rollbar.Serialization.Json;

    //[JsonConverter(typeof(DictionaryConverter))]
    public class Message 
        : ExtendableDtoBase
    {
        public static class ReservedProperties
        {
            public const string Body = "body";
        }

        public Message(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentNullException(nameof(body));
            }

            Body = body;
        }

        //[JsonIgnore]
        public string Body
        {
            get { return this._keyedValues[ReservedProperties.Body] as string; }
            private set { this._keyedValues[ReservedProperties.Body] = value; }
        }

    }
}
