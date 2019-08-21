namespace Rollbar.PayloadScrubbing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;
    using System.Linq;
    using Rollbar.Serialization.Json;


    /// <summary>
    /// Class JsonStringScrubber.
    /// Implements the <see cref="Rollbar.PayloadScrubbing.StringScrubber" />
    /// </summary>
    /// <seealso cref="Rollbar.PayloadScrubbing.StringScrubber" />
    internal class JsonStringScrubber
        : StringScrubber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonStringScrubber"/> class.
        /// </summary>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        public JsonStringScrubber(string scrubMask, IEnumerable<string> scrubFields) 
            : base(scrubMask, scrubFields)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonStringScrubber"/> class.
        /// </summary>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubPaths">The scrub paths.</param>
        public JsonStringScrubber(string scrubMask, IEnumerable<string> scrubFields, IEnumerable<string> scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonStringScrubber"/> class.
        /// </summary>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubPaths">The scrub paths.</param>
        public JsonStringScrubber(string scrubMask, string[] scrubFields, string[] scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
        }

        /// <summary>
        /// Does the scrub.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>System.String.</returns>
        protected override string DoScrub(string inputString)
        {
            string scrubbedString = JsonScrubber.ScrubJsonFieldsByName(inputString, this._scrubFields, this._scrubMask);
            scrubbedString = JsonScrubber.ScrubJsonFieldsByPaths(scrubbedString, this._scrubPaths, this._scrubMask);
            return scrubbedString;
        }
    }
}
