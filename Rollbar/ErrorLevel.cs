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
        /// <summary>
        /// The critical error/log level.
        /// </summary>
        Critical,

        /// <summary>
        /// The error log level.
        /// </summary>
        Error,

        /// <summary>
        /// The warning log level.
        /// </summary>
        Warning,

        /// <summary>
        /// The informational log level.
        /// </summary>
        Info,

        /// <summary>
        /// The debug log level.
        /// </summary>
        Debug,
    }
}
