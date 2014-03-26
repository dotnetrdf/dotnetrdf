using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace VDS.RDF.Parsing.Contexts
{
    public class TriXParserContext
        : BaseParserContext
    {
        public TriXParserContext(IRdfHandler handler, IParserProfile profile) 
            : base(handler, false, profile) { }
    }

    public class TriXStreamingParserContext
        : TriXParserContext
    {
        public TriXStreamingParserContext(IRdfHandler handler, XmlReader reader, IParserProfile profile)
            : base(handler, profile)
        {
            this.XmlReader = reader;
        }

        /// <summary>
        /// Gets/Sets the XML reader
        /// </summary>
        public XmlReader XmlReader { get; private set; }
    }
}
