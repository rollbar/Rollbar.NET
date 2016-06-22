using Newtonsoft.Json;

namespace RollbarDotNet 
{
    [JsonConverter(typeof(ErrorLevelConverter))]
    public enum ErrorLevel 
    {
        Critical,
        Error,
        Warning,
        Info,
        Debug
    }
}