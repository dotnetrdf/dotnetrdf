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
using VDS.RDF.Query.Inference.Pellet;
using VDS.RDF.Query.Inference.Pellet.Services;

namespace VDS.RDF.Query
{
    /// <summary>
    /// A SPARQL Query Processor which processes queries by parsing them to the SPARQL Query Service of a Knowledge Base on a Pellet Server
    /// </summary>
    public class PelletQueryProcessor 
        : ISparqlQueryProcessor 
    {
        private Type _svcType = typeof(QueryService);
        private QueryService _svc;

        /// <summary>
        /// Creates a new Pellet Query Processor
        /// </summary>
        /// <param name="server">Pellet Server</param>
        /// <param name="kbName">Knowledge Base Name</param>
        public PelletQueryProcessor(PelletServer server, String kbName)
        {
            if (server.HasKnowledgeBase(kbName))
            {
                KnowledgeBase kb = server.GetKnowledgeBase(kbName);

                // Check SPARQL Query is supported
                if (kb.SupportsService(_svcType))
                {
                    _svc = (QueryService)kb.GetService(_svcType);
                }
                else
                {
                    throw new NotSupportedException("Cannot create a Pellet Query Processor as the selected Knowledge Base does not support the Query Service");
                }
            }
            else
            {
                throw new NotSupportedException("Cannot create a Pellet Query Processor for the Knowledge Base named '" + kbName + "' as this Server does not have the named Knowledge Base");
            }
        }

        /// <summary>
        /// Creates a new Pellet Query Processor
        /// </summary>
        /// <param name="serverUri">Pellet Server URI</param>
        /// <param name="kbName">Knowledge Base Name</param>
        public PelletQueryProcessor(Uri serverUri, String kbName)
            : this(new PelletServer(serverUri), kbName) { }

        ///<summary>
        ///  Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public object ProcessQuery(SparqlQuery query)
        {
            query.QueryExecutionTime = null;
            DateTime start = DateTime.Now;
            try
            {
                Object temp = _svc.Query(query.ToString());
                return temp;
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = (DateTime.Now - start);
            }
        }

        /// <summary>
        /// Processes a SPARQL Query passing the results to the RDF or Results handler as appropriate
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
        {
            query.QueryExecutionTime = null;
            DateTime start = DateTime.Now;
            try
            {
                _svc.Query(rdfHandler, resultsHandler, query.ToString());
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = (DateTime.Now - start);
            }
        }

        /// <summary>
        /// Processes a SPARQL Query asynchronously invoking the relevant callback when the query completes
        /// </summary>
        /// <param name="query">SPARQL QUery</param>
        /// <param name="rdfCallback">Callback for queries that return a Graph</param>
        /// <param name="resultsCallback">Callback for queries that return a Result Set</param>
        /// <param name="state">State to pass to the callback</param>
        public void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback, Object state)
        {
            query.QueryExecutionTime = null;
            DateTime start = DateTime.Now;
            try
            {
                _svc.Query(query.ToString(), rdfCallback, resultsCallback, state);
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = (DateTime.Now - start);
            }
        }

        /// <summary>
        /// Processes a SPARQL Query asynchronously passing the results to the relevant handler and invoking the callback when the query completes
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query, QueryCallback callback, Object state)
        {
            query.QueryExecutionTime = null;
            DateTime start = DateTime.Now;
            try
            {
                _svc.Query(rdfHandler, resultsHandler, query.ToString(), callback, state);
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = (DateTime.Now - start);
            }
        }
    }
}
