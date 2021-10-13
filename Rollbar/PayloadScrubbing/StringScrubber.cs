namespace Rollbar.PayloadScrubbing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Class StringScrubber.
    /// </summary>
    /// <remarks>It fails safe in case (if the scrubbing strategy does not succeed)
    /// by returning a string value denoting the scrubbing failure instead of
    /// the unscrubbed input string.</remarks>
    internal class StringScrubber
    {
        /// <summary>
        /// The failed scrubbing message
        /// </summary>
        private const string failedScrubbingMessage = "Data scrubbing failed!";
        /// <summary>
        /// The scrub mask
        /// </summary>
        protected readonly string   _scrubMask = "***";
        /// <summary>
        /// The scrub fields
        /// </summary>
        protected readonly string[]? _scrubFields;
        /// <summary>
        /// The scrub paths
        /// </summary>
        protected readonly string[]? _scrubPaths;

        /// <summary>
        /// Prevents a default instance of the <see cref="StringScrubber"/> class from being created.
        /// </summary>
        private StringScrubber()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringScrubber"/> class.
        /// </summary>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
#pragma warning disable CA1307 // Specify StringComparison for clarity
        public StringScrubber(string scrubMask, IEnumerable<string> scrubFields)
            : this(scrubMask, scrubFields.Where(n => !n.Contains('.')), scrubFields.Where(n => n.Contains('.')))
#pragma warning restore CA1307 // Specify StringComparison for clarity
        {
            Debug.Assert(
                scrubFields.Count() ==
                this._scrubFields!.Length + this._scrubPaths!.Length
            );
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="StringScrubber"/> class.
        /// </summary>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubPaths">The scrub paths.</param>
        public StringScrubber(string scrubMask, IEnumerable<string> scrubFields, IEnumerable<string> scrubPaths)
            : this(scrubMask, scrubFields.ToArray(), scrubPaths.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringScrubber"/> class.
        /// </summary>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubPaths">The scrub paths.</param>
        public StringScrubber(string scrubMask, string[] scrubFields, string[] scrubPaths)
        {
            this._scrubMask = scrubMask;
            this._scrubFields = scrubFields;
            this._scrubPaths = scrubPaths;
        }

        /// <summary>
        /// Scrubs the specified input string.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>System.String.</returns>
        public string? Scrub(string? inputString)
        {
            if (inputString == null || string.IsNullOrWhiteSpace(inputString)
                || ( 
                     (this._scrubFields == null || this._scrubFields.Length == 0 || this._scrubFields.All(i => string.IsNullOrWhiteSpace(i)))
                     && (this._scrubPaths == null || this._scrubPaths.Length == 0 || this._scrubPaths.All(i => string.IsNullOrWhiteSpace(i)))
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
                return this.GetType().Name + ": " + failedScrubbingMessage + $"EXCEPTION: {e}";
            }
        }

        /// <summary>
        /// Does the scrub.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>System.String.</returns>
        protected virtual string? DoScrub(string inputString)
        {
            // assuming non-structured string here: 
            return this.GetType().Name + ": " + failedScrubbingMessage;
        }
    }
}
