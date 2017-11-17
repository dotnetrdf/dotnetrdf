/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
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
        private SparqlQueryParser _parser = new SparqlQueryParser();

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
            SparqlQuery q = _parser.ParseFromString(sparqlQuery);
            return ExecuteQuery(q);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Graph handling the results with the given handlers
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            SparqlQuery q = _parser.ParseFromString(sparqlQuery);
            ExecuteQuery(rdfHandler, resultsHandler, q);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Graph
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public Object ExecuteQuery(SparqlQuery query)
        {
            if (_processor == null)
            {
                InMemoryDataset ds = new InMemoryDataset(this);
                _processor = new LeviathanQueryProcessor(ds);
            }
            return _processor.ProcessQuery(query);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Graph handling the results with the given handlers
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        public void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
        {
            if (_processor == null)
            {
                InMemoryDataset ds = new InMemoryDataset(this);
                _processor = new LeviathanQueryProcessor(ds);
            }
            _processor.ProcessQuery(rdfHandler, resultsHandler, query);
        }
    }
}
