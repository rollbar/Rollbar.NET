namespace Rollbar
{
    using Newtonsoft.Json;
    using Rollbar.Serialization.Json;

    [JsonConverter(typeof(ErrorLevelConverter))]
    public enum ErrorLevel
    {
        Critical,
        Error,
        Warning,
        Info,
        Debug,
    }
}
