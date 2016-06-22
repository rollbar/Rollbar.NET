using System.Collections.Generic;
using Newtonsoft.Json;

namespace RollbarDotNet 
{
    [JsonConverter(typeof(ArbitraryKeyConverter))]
    public class JavascriptClient : HasArbitraryKeys 
    {
        public string Browser { get; set; }

        public string CodeVersion { get; set; }

        public bool? SourceMapEnabled { get; set; }

        public bool? GuessUncaughtFrames { get; set; }

        protected override void Normalize() 
        {
            Browser = (string)(AdditionalKeys.ContainsKey("browser") ? AdditionalKeys["browser"] : Browser);
            AdditionalKeys.Remove("browser");
            CodeVersion = (string)(AdditionalKeys.ContainsKey("code_version") ? AdditionalKeys["code_version"] : CodeVersion);
            AdditionalKeys.Remove("code_version");
            SourceMapEnabled = (bool?)(AdditionalKeys.ContainsKey("source_map_enabled") ? AdditionalKeys["source_map_enabled"] : SourceMapEnabled);
            AdditionalKeys.Remove("source_map_enabled");
            GuessUncaughtFrames = (bool?)(AdditionalKeys.ContainsKey("guess_uncaught_frames") ? AdditionalKeys["guess_uncaught_frames"] : GuessUncaughtFrames);
            AdditionalKeys.Remove("guess_uncaught_frames");
        }

        protected override Dictionary<string, object> Denormalize(Dictionary<string, object> dict) 
        {
            if (Browser != null) 
            {
                dict["browser"] = Browser;
            }

            if (CodeVersion != null) 
            {
                dict["code_version"] = CodeVersion;
            }

            if (SourceMapEnabled != null) 
            {
                dict["source_map_enabled"] = SourceMapEnabled;
            }

            if (GuessUncaughtFrames != null) 
            {
                dict["guess_uncaught_frames"] = GuessUncaughtFrames;
            }

            return dict;
        }
    }
}
