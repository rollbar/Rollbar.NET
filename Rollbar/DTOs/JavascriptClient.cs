namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Serialization.Json;

    //[JsonConverter(typeof(DictionaryConverter))]
    public class JavascriptClient 
        : ExtendableDtoBase
    {
        public static class ReservedProperties
        {
            public const string Browser = "browser";
            public const string CodeVersion = "code_version";
            public const string SourceMapEnabled = "source_map_enabled";
            public const string GuessUncaughtFrames = "guess_uncaught_frames";
        }

        //[JsonIgnore]
        public string Browser
        {
            get { return this._keyedValues[ReservedProperties.Browser] as string; }
            set { this._keyedValues[ReservedProperties.Browser] = value; }
        }

        //[JsonIgnore]
        public string CodeVersion
        {
            get { return this._keyedValues[ReservedProperties.CodeVersion] as string; }
            set { this._keyedValues[ReservedProperties.CodeVersion] = value; }
        }

        //[JsonIgnore]
        public bool? SourceMapEnabled
        {
            get { return this._keyedValues[ReservedProperties.SourceMapEnabled] as bool?; }
            set { this._keyedValues[ReservedProperties.SourceMapEnabled] = value; }
        }

        //[JsonIgnore]
        public bool? GuessUncaughtFrames
        {
            get { return this._keyedValues[ReservedProperties.GuessUncaughtFrames] as bool?; }
            set { this._keyedValues[ReservedProperties.GuessUncaughtFrames] = value; }
        }
    }
}
