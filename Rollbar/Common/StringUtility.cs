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

        public static byte[] GetEncodingBytes(string input, Encoding encoding)
        {
            if (input == null)
            {
                return new byte[0];
            }

            return encoding.GetBytes(input);
        }

        public static int CalculateMaxEncodingBytes(string input, Encoding encoding)
        {
            if (input == null)
            {
                return 0;
            }

            return encoding.GetMaxByteCount(input.Length);
        }

        public static int CalculateExactEncodingBytes(string input, Encoding encoding)
        {
            if (input == null)
            {
                return 0;
            }

            return encoding.GetByteCount(input);
        }

        public static string Truncate(string input, Encoding encoding, int encodedBytesLimit)
        {
            int maxEncodedBytes = encoding.GetMaxByteCount(input.Length);
            if (maxEncodedBytes <= encodedBytesLimit)
            {
                return input; //nothing to truncate...
            }

            int truncaionIndicatorTotalByte = encoding.GetByteCount(truncaionIndicator);
            byte[] inputEncodedBytes = encoding.GetBytes(input);
            int totalBytes = encodedBytesLimit - truncaionIndicatorTotalByte;
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

            truncatedInput += truncaionIndicator;

            Assumption.AssertTrue(encoding.GetByteCount(
                truncatedInput) <= encodedBytesLimit || encodedBytesLimit < truncaionIndicatorTotalByte, 
                nameof(truncatedInput)
                );

            return truncatedInput;
        }

        private const string truncaionIndicator = "...";
    }
}
