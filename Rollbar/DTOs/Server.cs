namespace Rollbar.DTOs
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Rollbar.Serialization.Json;

    //[JsonConverter(typeof(DictionaryConverter))]
    public class Server 
        : ExtendableDtoBase
    {
        protected Server(IDictionary<string, object> arbitraryKeyValuePairs) 
            : base(arbitraryKeyValuePairs)
        {
        }

        public static class ReservedProperties
        {
            public const string Host = "host";
            public const string Root = "root";
            public const string Branch = "branch";
            public const string CodeVersion = "code_version";
        }

        //[JsonIgnore]
        public string Host
        {
            get { return this._keyedValues[ReservedProperties.Host] as string; }
            set { this._keyedValues[ReservedProperties.Host] = value; }
        }

        //[JsonIgnore]
        public string Root
        {
            get { return this._keyedValues[ReservedProperties.Root] as string; }
            set { this._keyedValues[ReservedProperties.Root] = value; }
        }

        //[JsonIgnore]
        public string Branch
        {
            get { return this._keyedValues[ReservedProperties.Branch] as string; }
            set { this._keyedValues[ReservedProperties.Branch] = value; }
        }

        //[JsonIgnore]
        public string CodeVersion
        {
            get { return this._keyedValues[ReservedProperties.CodeVersion] as string; }
            set { this._keyedValues[ReservedProperties.CodeVersion] = value; }
        }
    }
}
