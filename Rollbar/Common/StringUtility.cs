namespace Rollbar.Common
{
    using Rollbar.Diagnostics;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A utility type aiding in string manipulations.
    /// </summary>
    public static class StringUtility
    {
        private static readonly TraceSource traceSource = new TraceSource(typeof(StringUtility).FullName ?? "StringUtility");

        /// <summary>
        /// Delegate TryParseHandler
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if value string is valid, <c>false</c> otherwise.</returns>
        public delegate bool TryParseHandler<T>(string? value, out T? result);

        /// <summary>
        /// Either successfully parses the provided string value or returns null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <param name="tryParseHandler">The try parse handler.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        public static T? Parse<T>(string? value, TryParseHandler<T> tryParseHandler) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (tryParseHandler(value, out T result))
            {
                return result;
            }

            traceSource.TraceEvent(TraceEventType.Warning, 0, $"Invalid value '{value}'");
            return null;
        }

        /// <summary>
        /// Either successfully parses the provided string value or returns specified default value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <param name="tryParseHandler">The try parse handler.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>T.</returns>
        public static T ParseOrDefault<T>(string? value, TryParseHandler<T> tryParseHandler, T defaultValue) where T : struct
        {
            T? result = StringUtility.Parse<T>(value, tryParseHandler);
            if (result.HasValue)
            {
                return result.Value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Combines the specified substrings using the specified separator.
        /// </summary>
        /// <param name="substrings">The substrings.</param>
        /// <param name="substringSeparator">The substring separator.</param>
        /// <returns></returns>
        public static string Combine(IEnumerable<string> substrings, string substringSeparator)
        {
            if (substrings == null)
            {
                return string.Empty;
            }

            return string.Join(substringSeparator, substrings);
        }

        /// <summary>
        /// Gets the encoding bytes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public static byte[] GetEncodingBytes(string input, Encoding encoding)
        {
            if (input == null)
            {
                return new byte[0];
            }

            return encoding.GetBytes(input);
        }

        /// <summary>
        /// Calculates the maximum encoding bytes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public static int CalculateMaxEncodingBytes(string input, Encoding encoding)
        {
            if (input == null)
            {
                return 0;
            }

            return encoding.GetMaxByteCount(input.Length);
        }

        /// <summary>
        /// Calculates the exact encoding bytes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public static int CalculateExactEncodingBytes(string input, Encoding encoding)
        {
            if (input == null)
            {
                return 0;
            }

            return encoding.GetByteCount(input);
        }

        /// <summary>
        /// Truncates the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="encodedBytesLimit">The encoded bytes limit.</param>
        /// <returns></returns>
        public static string? Truncate(string? input, Encoding encoding, int encodedBytesLimit)
        {
            if (input == null)
            {
                return input; //nothing to truncate...
            }

            byte[] inputEncodedBytes = encoding.GetBytes(input);
            Assumption.AssertEqual(inputEncodedBytes.Length, encoding.GetByteCount(input), nameof(inputEncodedBytes.Length));
            if (inputEncodedBytes.Length <= encodedBytesLimit)
            {
                return input; //nothing to truncate...
            }

            int truncationIndicatorTotalBytes = encoding.GetByteCount(truncationIndicator);
            int totalBytes = encodedBytesLimit - truncationIndicatorTotalBytes;
            if (totalBytes < 0)
            {
                totalBytes = 0;
            }
            string truncatedInput = encoding.GetString(inputEncodedBytes, 0, totalBytes);
            if ((totalBytes > 0) 
                && (truncatedInput[truncatedInput.Length - 1] != input[truncatedInput.Length - 1])
                )
            {
                truncatedInput = truncatedInput.Substring(0, truncatedInput.Length - 1);
            }

            truncatedInput += truncationIndicator;

            Assumption.AssertTrue(encoding.GetByteCount(
                truncatedInput) <= encodedBytesLimit || encodedBytesLimit < truncationIndicatorTotalBytes, 
                nameof(truncatedInput)
                );

            return truncatedInput;
        }

        private const string truncationIndicator = "...";
    }
}
