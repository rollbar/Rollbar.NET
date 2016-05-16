using Newtonsoft.Json;

namespace Rollbar {
    [JsonConverter(typeof (ErrorLevelConverter))]
    public enum ErrorLevel {
        Critical,
        Error,
        Warning,
        Info,
        Debug
    }
}