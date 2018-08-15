namespace Rollbar
{
    using Newtonsoft.Json;
    using Rollbar.Serialization.Json;

    /// <summary>
    /// Lists all the supported Rollbar error levels.
    /// The members are ordered from least to most significant.
    /// </summary>
    [JsonConverter(typeof(ErrorLevelConverter))]
    public enum ErrorLevel
    {
        /// <summary>
        /// The debug log level.
        /// </summary>
        Debug,

        /// <summary>
        /// The informational log level.
        /// </summary>
        Info,

        /// <summary>
        /// The warning log level.
        /// </summary>
        Warning,

        /// <summary>
        /// The error log level.
        /// </summary>
        Error,

        /// <summary>
        /// The critical error/log level.
        /// </summary>
        Critical,

    }
}
