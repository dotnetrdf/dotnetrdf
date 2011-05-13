/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for SPARQL RDF Parser
    /// </summary>
    public class SparqlRdfParserContext : BaseResultsParserContext
    {
        private IGraph _g;

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="g">Graph to parse from</param>
        /// <param name="handler">Results Handler</param>
        public SparqlRdfParserContext(IGraph g, ISparqlResultsHandler handler)
            : base(handler)
        {
            if (g == null) throw new ArgumentNullException("g");
            this._g = g;
        }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="g">Graph to parse from</param>
        /// <param name="results">Results Handler</param>
        public SparqlRdfParserContext(IGraph g, SparqlResultSet results)
            : this(g, new ResultSetHandler(results)) { }

        /// <summary>
        /// Gets the Graph being parsed from
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

    }
}
