namespace Rollbar
{
    using Newtonsoft.Json;
    using Rollbar.Serialization.Json;

    /// <summary>
    /// Lists all the supported Rollbar error levels.
    /// </summary>
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
