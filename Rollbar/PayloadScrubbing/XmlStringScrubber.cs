using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Rollbar.PayloadScrubbing
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class XmlStringScrubber
        : StringScrubber
    {
        private readonly JsonStringScrubber _jsonStringScrubber;
        public XmlStringScrubber(string scrubMask, IEnumerable<string> scrubFields) 
            : base(scrubMask, scrubFields)
        {
            this._jsonStringScrubber = new JsonStringScrubber(this._scrubMask, _scrubFields, _scrubPaths);
        }

        public XmlStringScrubber(string scrubMask, IEnumerable<string> scrubFields, IEnumerable<string> scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
            this._jsonStringScrubber = new JsonStringScrubber(this._scrubMask, _scrubFields, _scrubPaths);
        }

        public XmlStringScrubber(string scrubMask, string[] scrubFields, string[] scrubPaths) 
            : base(scrubMask, scrubFields, scrubPaths)
        {
            this._jsonStringScrubber = new JsonStringScrubber(this._scrubMask, _scrubFields, _scrubPaths);
        }

        protected override string DoScrub(string inputString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(inputString);
            string json = JsonConvert.SerializeXmlNode(doc);
            json = this._jsonStringScrubber.Scrub(json);
            XNode xmlNode = JsonConvert.DeserializeXNode(json);
            string xml = xmlNode.ToString();
            return xml;
        }
    }
}
