using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Contexts
{
    public class SparqlXmlParserContext : BaseResultsParserContext
    {
        private ISparqlResultsHandler _handler;
        private XmlReader _reader;

        public SparqlXmlParserContext(XmlReader reader, ISparqlResultsHandler handler)
            : base(handler)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            this._reader = reader;
        }

        public SparqlXmlParserContext(XmlReader reader, SparqlResultSet results)
            : this(reader, new ResultSetHandler(results)) { }

        public XmlReader Input
        {
            get
            {
                return this._reader;
            }
        }
    }
}
