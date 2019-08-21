[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar.PayloadScrubbing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    internal class FormDataStringScrubber
        : StringScrubber
    {
        private readonly string _formDataBoundary;

        public FormDataStringScrubber(string contentTypeHeaderValue, string scrubMask, IEnumerable<string> scrubFields) 
            : base(scrubMask, scrubFields)
        {
            this._formDataBoundary = ExtractBaundaryValue(contentTypeHeaderValue);
        }

        public FormDataStringScrubber(string contentTypeHeaderValue, string scrubMask, IEnumerable<string> scrubFields, IEnumerable<string> scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
            this._formDataBoundary = ExtractBaundaryValue(contentTypeHeaderValue);
        }

        public FormDataStringScrubber(string contentTypeHeaderValue, string scrubMask, string[] scrubFields, string[] scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
            this._formDataBoundary = ExtractBaundaryValue(contentTypeHeaderValue);
        }

        private string ExtractBaundaryValue(string contentTypeHeaderValue)
        {
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

        protected List<List<string>> SplitIntoParts(string inputString)
        {
            List<List<string>> parts = new List<List<string>>();

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
            foreach (var p in parts)
            {
                if (string.IsNullOrEmpty(p.Last()))
                {
                    p.RemoveAt(p.Count - 1);
                }
            }
            return parts;
        }

        private void Process(List<string> part)
        {
            bool scrubContent = false;
            int lineIndex = 0;
            int contentStartIndex = 0;
            foreach (var line in part)
            {
                if (line.StartsWith("Content-Disposition: form-data; name=\""))
                {
                    foreach (var scrubField in this._scrubFields)
                    {
                        if (line.Contains($"\"{scrubField}\""))
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
                string closingBoundary = null;
                if (part[part.Count - 1].StartsWith(this._formDataBoundary))
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

        protected override string DoScrub(string inputString)
        {
            if (!inputString.Contains("Content-Disposition: form-data;"))
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
