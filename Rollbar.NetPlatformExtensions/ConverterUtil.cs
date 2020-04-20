namespace Rollbar.NetPlatformExtensions
{
    using mslogging = Microsoft.Extensions.Logging;

    /// <summary>
    /// Class ConverterUtil.
    /// </summary>
    public static class ConverterUtil
    {
        /// <summary>
        /// Converts to Rollbar's ErrorLevel.
        /// </summary>
        /// <param name="logLevel">The NetPlatformExtensions' log level.</param>
        /// <returns>ErrorLevel.</returns>
        public static ErrorLevel ToRollbarErrorLevel(mslogging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case mslogging.LogLevel.None:
                case mslogging.LogLevel.Critical:
                    return ErrorLevel.Critical;
                case mslogging.LogLevel.Error:
                    return ErrorLevel.Error;
                case mslogging.LogLevel.Warning:
                    return ErrorLevel.Warning;
                case mslogging.LogLevel.Information:
                    return ErrorLevel.Info;
                case mslogging.LogLevel.Trace:
                case mslogging.LogLevel.Debug:
                default:
                    return ErrorLevel.Debug;
            }
        }

        /// <summary>
        /// Converts to NetPlatformExtensions' LogLevel.
        /// </summary>
        /// <param name="errorLevel">The Rollbar's error level.</param>
        /// <returns>mslogging.LogLevel.</returns>
        public static mslogging.LogLevel ToNetPlatformExtensionsLogLevel(ErrorLevel errorLevel)
        {
            switch (errorLevel)
            {
                case ErrorLevel.Critical:
                    return mslogging.LogLevel.Critical;
                case ErrorLevel.Error:
                    return mslogging.LogLevel.Error;
                case ErrorLevel.Warning:
                    return mslogging.LogLevel.Warning;
                case ErrorLevel.Info:
                    return mslogging.LogLevel.Information;
                case ErrorLevel.Debug:
                    return mslogging.LogLevel.Debug;
                default:
                    return mslogging.LogLevel.Trace;
            }
        }
    }
}
