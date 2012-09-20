/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing Graphs which can be directly queried using SPARQL
    /// </summary>
    public class QueryableGraph
        : Graph
    {
        private ISparqlQueryProcessor _processor;
        private SparqlQueryParser _parser;

        /// <summary>
        /// Creates a new Queryable Graph
        /// </summary>
        public QueryableGraph()
            : base() { }

        /// <summary>
        /// Executes a SPARQL Query on the Graph
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public Object ExecuteQuery(String sparqlQuery)
        {
            SparqlQuery q = this._parser.ParseFromString(sparqlQuery);
            return this.ExecuteQuery(q);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Graph handling the results with the given handlers
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            SparqlQuery q = this._parser.ParseFromString(sparqlQuery);
            this.ExecuteQuery(rdfHandler, resultsHandler, q);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Graph
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public Object ExecuteQuery(SparqlQuery query)
        {
            if (this._processor == null)
            {
                InMemoryDataset ds = new InMemoryDataset(this);
                this._processor = new LeviathanQueryProcessor(ds);
            }
            return this._processor.ProcessQuery(query);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Graph handling the results with the given handlers
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        public void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
        {
            if (this._processor == null)
            {
                InMemoryDataset ds = new InMemoryDataset(this);
                this._processor = new LeviathanQueryProcessor(ds);
            }
            this._processor.ProcessQuery(rdfHandler, resultsHandler, query);
        }
    }
}
