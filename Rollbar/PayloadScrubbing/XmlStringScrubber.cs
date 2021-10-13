namespace Rollbar.PayloadScrubbing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Class XmlStringScrubber.
    /// Implements the <see cref="Rollbar.PayloadScrubbing.StringScrubber" />
    /// </summary>
    /// <seealso cref="Rollbar.PayloadScrubbing.StringScrubber" />
    internal class XmlStringScrubber
        : StringScrubber
    {
        /// <summary>
        /// The json string scrubber
        /// </summary>
        private readonly JsonStringScrubber _jsonStringScrubber;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlStringScrubber"/> class.
        /// </summary>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        public XmlStringScrubber(string scrubMask, IEnumerable<string> scrubFields) 
            : base(scrubMask, scrubFields)
        {
            this._jsonStringScrubber = new JsonStringScrubber(this._scrubMask, _scrubFields ?? new string[0], _scrubPaths ?? new string[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlStringScrubber"/> class.
        /// </summary>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubPaths">The scrub paths.</param>
        public XmlStringScrubber(string scrubMask, IEnumerable<string> scrubFields, IEnumerable<string> scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
            this._jsonStringScrubber = new JsonStringScrubber(this._scrubMask, _scrubFields ?? new string[0], _scrubPaths ?? new string[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlStringScrubber"/> class.
        /// </summary>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubPaths">The scrub paths.</param>
        public XmlStringScrubber(string scrubMask, string[] scrubFields, string[] scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
            this._jsonStringScrubber = new JsonStringScrubber(this._scrubMask, _scrubFields ?? new string[0], _scrubPaths ?? new string[0]);
        }

        /// <summary>
        /// Does the scrub.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>System.String.</returns>
        protected override string? DoScrub(string inputString)
        {
            XmlDocument doc = new XmlDocument();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            doc.XmlResolver = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            doc.LoadXml(inputString);
            string? json = JsonConvert.SerializeXmlNode(doc);
            json = this._jsonStringScrubber.Scrub(json);
            if (json != null)
            {
                XNode? xmlNode = JsonConvert.DeserializeXNode(json);
                string? xml = xmlNode?.ToString();
                return xml;
            }

            return null;
        }
    }
}
