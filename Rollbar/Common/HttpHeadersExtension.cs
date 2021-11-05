namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;

    /// <summary>
    /// Class HttpHeadersExtension.
    /// </summary>
    public static class HttpHeadersExtension
    {
        /// <summary>
        /// The shared empty result
        /// </summary>
        private static readonly string[] emptyResult = new string[0];

        /// <summary>
        /// Gets the header values safely.
        /// </summary>
        /// <param name="httpHeaders">The HTTP headers.</param>
        /// <param name="headerName">Name of the header.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        public static IEnumerable<string> GetHeaderValuesSafely(this HttpHeaders httpHeaders, string headerName)
        {
            IEnumerable<string>? headerValues;

            if (httpHeaders.TryGetValues(headerName, out headerValues) 
                && headerValues != null
                )
            {
                return headerValues;
            }

            return emptyResult;
        }

        /// <summary>
        /// Gets the last header value safely.
        /// </summary>
        /// <param name="httpHeaders">The HTTP headers.</param>
        /// <param name="headerName">Name of the header.</param>
        /// <returns>System.String.</returns>
        public static string? GetLastHeaderValueSafely(this HttpHeaders httpHeaders, string headerName)
        {
            string? headerValue = httpHeaders.GetHeaderValuesSafely(headerName).LastOrDefault();
            return headerValue;
        }

        /// <summary>
        /// Parses the header value safely if any.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpHeaders">The HTTP headers.</param>
        /// <param name="headerName">Name of the header.</param>
        /// <param name="tryParseHandler">The try parse handler.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        public static T? ParseHeaderValueSafelyIfAny<T>(this HttpHeaders httpHeaders, string headerName, StringUtility.TryParseHandler<T> tryParseHandler) 
            where T : struct
        {
            string? stringValue = httpHeaders.GetLastHeaderValueSafely(headerName);
            return StringUtility.Parse<T>(stringValue, tryParseHandler);
        }

        /// <summary>
        /// Parses the header value safely or default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpHeaders">The HTTP headers.</param>
        /// <param name="headerName">Name of the header.</param>
        /// <param name="tryParseHandler">The try parse handler.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>T.</returns>
        public static T ParseHeaderValueSafelyOrDefault<T>(this HttpHeaders httpHeaders, string headerName, StringUtility.TryParseHandler<T> tryParseHandler, T defaultValue)
            where T : struct
        {
            T? parsedValue = httpHeaders.ParseHeaderValueSafelyIfAny<T>(headerName, tryParseHandler);
            if (parsedValue.HasValue)
            {
                return parsedValue.Value;
            }

            return defaultValue;
        }
    }
}
