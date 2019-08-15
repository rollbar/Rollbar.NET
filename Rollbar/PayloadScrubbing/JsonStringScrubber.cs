using Rollbar.Serialization.Json;

namespace Rollbar.PayloadScrubbing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;
    using System.Linq;

    internal class JsonStringScrubber
        : StringScrubber
    {
        public JsonStringScrubber(string scrubMask, IEnumerable<string> scrubFields) 
            : base(scrubMask, scrubFields)
        {
        }

        public JsonStringScrubber(string scrubMask, IEnumerable<string> scrubFields, IEnumerable<string> scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
        }

        public JsonStringScrubber(string scrubMask, string[] scrubFields, string[] scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
        }

        protected override string DoScrub(string inputString)
        {
            string scrubbedString = JsonScrubber.ScrubJsonFieldsByName(inputString, this._scrubFields, this._scrubMask);
            scrubbedString = JsonScrubber.ScrubJsonFieldsByPaths(scrubbedString, this._scrubFields, this._scrubMask);
            return scrubbedString;
        }
    }
}
