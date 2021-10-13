namespace Rollbar.Common
{
    using System;

    /// <summary>
    /// Utility class for date/time related conversions.
    /// </summary>
    public static class DateTimeUtil
    {

        /// <summary>
        /// Converts to unix timestamp in milliseconds.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static long ConvertToUnixTimestampInMilliseconds(DateTime dateTime)
        {
            return Convert.ToInt64(dateTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
        }

        /// <summary>
        /// Converts to unix timestamp in seconds.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static long ConvertToUnixTimestampInSeconds(DateTime dateTime)
        {
            return Convert.ToInt64(dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        /// <summary>
        /// Converts from unix timestamp in seconds.
        /// </summary>
        /// <param name="unixTimestampInSeconds">The unix timestamp in seconds.</param>
        /// <returns>corresponding DateTimeOffset value</returns>
        public static DateTimeOffset ConvertFromUnixTimestampInSeconds(long unixTimestampInSeconds)
        {
            DateTimeOffset timestamp = new DateTimeOffset(new DateTime(1970, 1, 1), TimeSpan.Zero);
            timestamp = timestamp.AddSeconds(Convert.ToDouble(unixTimestampInSeconds));
            return timestamp;
        }

        /// <summary>
        /// Tries the parse from unix timestamp in seconds string.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="dateTimeOffset">The date time offset.</param>
        /// <returns><c>true</c> if was able to parse successfully, <c>false</c> otherwise.</returns>
        public static bool TryParseFromUnixTimestampInSecondsString(string? inputString, out DateTimeOffset dateTimeOffset)
        {
            if (long.TryParse(inputString, out long unixTimestamp))
            {
                dateTimeOffset = DateTimeUtil.ConvertFromUnixTimestampInSeconds(unixTimestamp);
                return true;
            }

            dateTimeOffset = default;
            return false;
        }
    }
}
