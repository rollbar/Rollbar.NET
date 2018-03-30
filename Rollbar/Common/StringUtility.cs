namespace Rollbar.Common
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A utility type aiding in string manipulations.
    /// </summary>
    public static class StringUtility
    {
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
        public static string Truncate(string input, Encoding encoding, int encodedBytesLimit)
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
