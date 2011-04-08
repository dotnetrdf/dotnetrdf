using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Contexts
{
    public class SparqlRdfParserContext : BaseResultsParserContext
    {
        private IGraph _g;

        public SparqlRdfParserContext(IGraph g, ISparqlResultsHandler handler)
            : base(handler)
        {
            if (g == null) throw new ArgumentNullException("g");
            this._g = g;
        }

        public SparqlRdfParserContext(IGraph g, SparqlResultSet results)
            : this(g, new ResultSetHandler(results)) { }

        public IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

    }
}
