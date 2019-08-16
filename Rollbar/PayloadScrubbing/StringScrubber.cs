namespace Rollbar.PayloadScrubbing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Class StringScrubber.
    /// </summary>
    /// <remarks>
    /// It fails safe in case (if the scrubbing strategy does not succeed)
    /// by returning a string value denoting the scrubbing failure instead of
    /// the unscrubbed input string.
    /// </remarks>
    internal class StringScrubber
    {
        private const string failedScrubbingMessage = "Data scrubbing failed!";
        protected readonly string   _scrubMask = "***";
        protected readonly string[] _scrubFields;
        protected readonly string[] _scrubPaths;

        private StringScrubber()
        {
        }

        public StringScrubber(string scrubMask, IEnumerable<string> scrubFields)
            : this(scrubMask, scrubFields.Where(n => !n.Contains('.')), scrubFields.Where(n => n.Contains('.')))
        {
            Debug.Assert(scrubFields.Count() ==
                         this._scrubFields.Length +
                         this._scrubPaths.Length
            );
        }
        public StringScrubber(string scrubMask, IEnumerable<string> scrubFields, IEnumerable<string> scrubPaths)
            : this(scrubMask, scrubFields.ToArray(), scrubPaths.ToArray())
        {
        }

        public StringScrubber(string scrubMask, string[] scrubFields, string[] scrubPaths)
        {
            this._scrubMask = scrubMask;
            this._scrubFields = scrubFields;
            this._scrubPaths = scrubPaths;
        }

        public string Scrub(string inputString)
        {
            if (((this._scrubFields == null || this._scrubFields.Length == 0)
                 && (this._scrubPaths == null || this._scrubPaths.Length == 0)
                )
                || (this._scrubFields.All(i => string.IsNullOrWhiteSpace(i))
                    && this._scrubPaths.All(i => string.IsNullOrWhiteSpace(i))
                )
            )
            {
                return inputString; //no data needs to be scrubbed...
            }

            try
            {
                return DoScrub(inputString);
            }
            catch (Exception e)
            {
                return this.GetType().Name + ": " + failedScrubbingMessage;
            }
        }

        protected virtual string DoScrub(string inputString)
        {
            // assuming non-structured string here: 
            return this.GetType().Name + ": " + failedScrubbingMessage;
        }
    }
}
