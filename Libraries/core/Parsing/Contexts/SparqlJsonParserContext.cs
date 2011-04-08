using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Contexts
{
    public class SparqlJsonParserContext : BaseResultsParserContext
    {
        private JsonTextReader _reader;

        public SparqlJsonParserContext(JsonTextReader reader, ISparqlResultsHandler handler)
            : base(handler)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            this._reader = reader;
        }

        public SparqlJsonParserContext(JsonTextReader reader, SparqlResultSet results)
            : this(reader, new ResultSetHandler(results)) { }

        public JsonTextReader Input
        {
            get
            {
                return this._reader;
            }
        }
    }
}
