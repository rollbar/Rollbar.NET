namespace Rollbar.PayloadScrubbing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    /// <summary>
    /// Class FormDataStringScrubber.
    /// Implements the <see cref="Rollbar.PayloadScrubbing.StringScrubber" />
    /// </summary>
    /// <seealso cref="Rollbar.PayloadScrubbing.StringScrubber" />
    /// <remarks>
    /// Performs scrubbing of form-data values as described here: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Type
    /// </remarks>
    internal class FormDataStringScrubber
        : StringScrubber
    {
        /// <summary>
        /// The form data boundary
        /// </summary>
        private readonly string? _formDataBoundary;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataStringScrubber"/> class.
        /// </summary>
        /// <param name="contentTypeHeaderValue">The content type header value.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        public FormDataStringScrubber(string contentTypeHeaderValue, string scrubMask, IEnumerable<string> scrubFields) 
            : base(scrubMask, scrubFields)
        {
            this._formDataBoundary = ExtractBaundaryValue(contentTypeHeaderValue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataStringScrubber"/> class.
        /// </summary>
        /// <param name="contentTypeHeaderValue">The content type header value.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubPaths">The scrub paths.</param>
        public FormDataStringScrubber(string contentTypeHeaderValue, string scrubMask, IEnumerable<string> scrubFields, IEnumerable<string> scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
            this._formDataBoundary = ExtractBaundaryValue(contentTypeHeaderValue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataStringScrubber"/> class.
        /// </summary>
        /// <param name="contentTypeHeaderValue">The content type header value.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubPaths">The scrub paths.</param>
        public FormDataStringScrubber(string contentTypeHeaderValue, string scrubMask, string[] scrubFields, string[] scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
            this._formDataBoundary = ExtractBaundaryValue(contentTypeHeaderValue);
        }


        /// <summary>
        /// Extracts the baundary value.
        /// </summary>
        /// <param name="contentTypeHeaderValue">The content type header value.</param>
        /// <returns>System.String.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "Loop works cleaner here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Not available in all .NET framework targets.")]
        private static string? ExtractBaundaryValue(string contentTypeHeaderValue)
        {
            // Value sample:
            // multipart/form-data; boundary=---------------------------974767299852498929531610575

            var components = contentTypeHeaderValue.Split(';');
            foreach (var component in components)
            {
                if (component.Contains("boundary="))
                {
                    return component.Split('=').Last();
                }
            }

            return null;
        }

        /// <summary>
        /// Splits the into parts.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>List&lt;List&lt;System.String&gt;&gt;.</returns>
        private List<List<string>> SplitIntoParts(string inputString)
        {
            List<List<string>> parts = new();

            List<string> part = new List<string>();
            parts.Add(part);
            foreach (var line in inputString.Split(new string[] {Environment.NewLine,}, StringSplitOptions.None))
            {
                if (string.CompareOrdinal(line, this._formDataBoundary) == 0)
                {
                    part = new List<string>();
                    parts.Add(part);
                }
                part.Add(line);
            }

            parts = parts.Where(p => p.Count > 0).ToList();
            foreach (var p in from p in parts
                              where string.IsNullOrEmpty(p.Last())
                              select p)
            {
                p.RemoveAt(p.Count - 1);
            }

            return parts;
        }


        /// <summary>
        /// Processes the specified part.
        /// </summary>
        /// <param name="part">The part.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "Loop works better here.")]
        private void Process(List<string> part)
        {
            bool scrubContent = false;
            int lineIndex = 0;
            int contentStartIndex = 0;
            foreach (var line in part)
            {
                if (this._scrubFields != null 
                    && line.StartsWith("Content-Disposition: form-data; name=\"")
                    )
                {
                    foreach (var scrubField in this._scrubFields)
                    {
#pragma warning disable CA1307 // Specify StringComparison for clarity
                        if (line.Contains($"\"{scrubField}\""))
#pragma warning restore CA1307 // Specify StringComparison for clarity
                        {
                            scrubContent = true;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    contentStartIndex = lineIndex + 1;
                    break;
                }

                lineIndex++;
            }

            if (scrubContent && contentStartIndex > 1 && contentStartIndex < part.Count)
            {
                // replace the content with a scrub mask line:
                string? closingBoundary = null;
                if (this._formDataBoundary != null 
                    && part[part.Count - 1].StartsWith(this._formDataBoundary)
                    )
                {
                    closingBoundary = part[part.Count - 1];
                }
                part.RemoveRange(contentStartIndex, part.Count - contentStartIndex);
                part.Add(this._scrubMask);
                if (closingBoundary != null)
                {
                    part.Add(closingBoundary);
                }
            }
        }

        /// <summary>
        /// Does the scrub.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="ArgumentException"></exception>
        protected override string DoScrub(string inputString)
        {
#pragma warning disable CA1307 // Specify StringComparison for clarity
            if (!inputString.Contains("Content-Disposition: form-data;"))
#pragma warning restore CA1307 // Specify StringComparison for clarity
            {
                throw new ArgumentException($"{nameof(inputString)} does not appear to be a form-data like...");
            }

            var parts = SplitIntoParts(inputString);

            StringBuilder sb = new StringBuilder(inputString.Length);

            foreach (var part in parts)
            {
                Process(part);
                foreach (var line in part)
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }
    }
}
