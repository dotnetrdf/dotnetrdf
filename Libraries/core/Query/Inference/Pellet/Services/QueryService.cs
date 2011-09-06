/*

Copyright Robert Vesse 2009-10
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    /// <summary>
    /// Represents the SPARQL Query Service provided by a Pellet Server knowledge base
    /// </summary>
    public class QueryService : PelletService
    {
        private String _sparqlUri;

        /// <summary>
        /// Creates a new SPARQL Query Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal QueryService(String name, JObject obj)
            : base(name, obj)
        {
            this._sparqlUri = this.Endpoint.Uri.Substring(0, this.Endpoint.Uri.IndexOf('{'));
        }

#if !SILVERLIGHT

        /// <summary>
        /// Makes a SPARQL Query against the Knowledge Base
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public Object Query(String sparqlQuery)
        {
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(this._sparqlUri));

            using (HttpWebResponse response = endpoint.QueryRaw(sparqlQuery))
            {
                try
                {
                    ISparqlResultsReader sparqlParser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                    SparqlResultSet results = new SparqlResultSet();
                    sparqlParser.Load(results, new StreamReader(response.GetResponseStream()));

                    response.Close();
                    return results;
                }
                catch (RdfParserSelectionException)
                {
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    Graph g = new Graph();
                    parser.Load(g, new StreamReader(response.GetResponseStream()));

                    response.Close();
                    return g;
                }
            }
        }

        /// <summary>
        /// Processes a SPARQL Query against the Knowledge Base passing the results to the RDF or Results handler as appropriate
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(this._sparqlUri));

            using (HttpWebResponse response = endpoint.QueryRaw(sparqlQuery))
            {
                try
                {
                    ISparqlResultsReader sparqlParser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                    sparqlParser.Load(resultsHandler, new StreamReader(response.GetResponseStream()));
                }
                catch (RdfParserSelectionException)
                {
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(rdfHandler, new StreamReader(response.GetResponseStream()));
                }
                response.Close();
            }
        }

#endif

        /// <summary>
        /// Makes a SPARQL Query against the Knowledge Base
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="graphCallback">Callback to invoke for queries that return a Graph</param>
        /// <param name="resultsCallback">Callback to invoke for queries that return a Result Set</param>
        /// <param name="state">State to pass to whichever callback function is invoked</param>
        /// <returns></returns>
        public void Query(String sparqlQuery, GraphCallback graphCallback, SparqlResultsCallback resultsCallback, Object state)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();

            this.Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, (gh, rh, _) =>
                {
                    if (results.ResultsType != SparqlResultsType.Unknown)
                    {
                        resultsCallback(results, state);
                    }
                    else
                    {
                        graphCallback(g, state);
                    }
                }, state);
        }

        /// <summary>
        /// Processes a SPARQL Query against the Knowledge Base passing the results to the RDF or Results handler as appropriate
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="callback">Callback to invoke once handling of results has completed</param>
        /// <param name="state">State to pass to the callback</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery, QueryCallback callback, Object state)
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(sparqlQuery);

            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(this._sparqlUri));
            switch (q.QueryType)
            {
                case SparqlQueryType.Ask:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    endpoint.QueryWithResultSet(sparqlQuery, (rs, _) =>
                        {
                            resultsHandler.Apply(rs);
                            callback(rdfHandler, resultsHandler, state);
                        }, state);
                    break;
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    endpoint.QueryWithResultGraph(sparqlQuery, (g, _) =>
                        {
                            rdfHandler.Apply(g);
                            callback(rdfHandler, resultsHandler, state);
                        }, state);
                    break;

                default:
                    throw new RdfQueryException("Cannot execute unknown query types against Pellet Server");
            }
        }
    }
}
