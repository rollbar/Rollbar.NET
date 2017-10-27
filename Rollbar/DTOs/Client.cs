namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Serialization.Json;

    [JsonConverter(typeof(DictionaryConverter))]
    public class Client 
        : ExtendableDtoBase
    {
        public static class ReservedProperties
        {
            public const string Javascript = "javascript";
        }

        public JavascriptClient Javascript
        {
            get { return this._keyedValues[ReservedProperties.Javascript] as JavascriptClient; }
            set { this._keyedValues[ReservedProperties.Javascript] = value; }
        }
    }
}
