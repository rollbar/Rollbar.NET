namespace Rollbar.DTOs
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Request : ExtendableDtoBase
    {
        protected Request(IDictionary<string, object> arbitraryKeyValuePairs) 
            : base(arbitraryKeyValuePairs)
        {
        }

        public static class ReservedProperties
        {
            public const string Url = "url";
            public const string Method = "method";
            public const string Headers = "headers";
            public const string Params = "params";
            public const string GetParams = "get_params";
            public const string QueryString = "query_string";
            public const string PostParams = "post_params";
            public const string PostBody = "post_body";
            public const string UserIp = "user_ip";
        }

        //[JsonIgnore]
        public string Url
        {
            get { return this._keyedValues[ReservedProperties.Url] as string; }
            set { this._keyedValues[ReservedProperties.Url] = value; }
        }

        //[JsonIgnore]
        public string Method
        {
            get { return this._keyedValues[ReservedProperties.Method] as string; }
            set { this._keyedValues[ReservedProperties.Method] = value; }
        }

        //[JsonIgnore]
        public IDictionary<string, string> Headers
        {
            get { return this._keyedValues[ReservedProperties.Headers] as IDictionary<string, string>; }
            set { this._keyedValues[ReservedProperties.Headers] = value; }
        }

        //[JsonIgnore]
        public IDictionary<string, object> Params
        {
            get { return this._keyedValues[ReservedProperties.Params] as IDictionary<string, object>; }
            set { this._keyedValues[ReservedProperties.Params] = value; }
        }

        //[JsonIgnore]
        public IDictionary<string, object> GetParams
        {
            get { return this._keyedValues[ReservedProperties.GetParams] as IDictionary<string, object>; }
            set { this._keyedValues[ReservedProperties.GetParams] = value; }
        }

        //[JsonIgnore]
        public string QueryString
        {
            get { return this._keyedValues[ReservedProperties.QueryString] as string; }
            set { this._keyedValues[ReservedProperties.QueryString] = value; }
        }

        //[JsonIgnore]
        public IDictionary<string, object> PostParams
        {
            get { return this._keyedValues[ReservedProperties.PostParams] as IDictionary<string, object>; }
            set { this._keyedValues[ReservedProperties.PostParams] = value; }
        }

        //[JsonIgnore]
        public string PostBody
        {
            get { return this._keyedValues[ReservedProperties.PostBody] as string; }
            set { this._keyedValues[ReservedProperties.PostBody] = value; }
        }

        //[JsonIgnore]
        public string UserIp
        {
            get { return this._keyedValues[ReservedProperties.UserIp] as string; }
            set { this._keyedValues[ReservedProperties.UserIp] = value; }
        }

    }
}
