using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Contexts
{
    public class BaseResultsParserContext : IResultsParserContext
    {
        private ISparqlResultsHandler _handler;
        private List<String> _variables = new List<string>();

        public BaseResultsParserContext(ISparqlResultsHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            this._handler = handler;
        }

        public ISparqlResultsHandler Handler
        {
            get
            {
                return this._handler;
            }
        }

        public List<String> Variables
        {
            get
            {
                return this._variables;
            }
        }
    }
}
