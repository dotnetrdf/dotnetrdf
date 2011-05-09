using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Base class for SPARQL Results Parser Contexts
    /// </summary>
    public class BaseResultsParserContext : IResultsParserContext
    {
        private ISparqlResultsHandler _handler;
        private List<String> _variables = new List<string>();

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="handler">Results Handler</param>
        public BaseResultsParserContext(ISparqlResultsHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            this._handler = handler;
        }

        /// <summary>
        /// Gets the Results Handler to be used
        /// </summary>
        public ISparqlResultsHandler Handler
        {
            get
            {
                return this._handler;
            }
        }

        /// <summary>
        /// Gets the Variables that have been seen
        /// </summary>
        public List<String> Variables
        {
            get
            {
                return this._variables;
            }
        }
    }
}
