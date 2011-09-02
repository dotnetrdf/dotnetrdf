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

#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Query
{

    /// <summary>
    /// A Class for connecting to multiple remote SPARQL Endpoints and federating queries over them with the data merging done locally
    /// </summary>
    /// <remarks>
    /// <para>
    /// Queries are federated by executing multiple requesting simultaneously and asynchronously against the endpoints in question with the data then merged locally.  The merging process does not attempt to remove duplicate data it just naively merges the data.
    /// </para>
    /// </remarks>
    public class FederatedSparqlRemoteEndpoint 
        : SparqlRemoteEndpoint
    {
        private List<SparqlRemoteEndpoint> _endpoints = new List<SparqlRemoteEndpoint>();
        private bool _ignoreFailedRequests = false;
        private int _maxSimultaneousRequests = 4;

        /// <summary>
        /// Creates a new Federated SPARQL Endpoint using a given Endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint</param>
        public FederatedSparqlRemoteEndpoint(SparqlRemoteEndpoint endpoint)
        {
            this._endpoints.Add(endpoint);
        }

        /// <summary>
        /// Creates a new Federated SPARQL Endpoint using the given Endpoints
        /// </summary>
        /// <param name="endpoints">Endpoints</param>
        public FederatedSparqlRemoteEndpoint(IEnumerable<SparqlRemoteEndpoint> endpoints)
        {
            this._endpoints.AddRange(endpoints);
        }

        /// <summary>
        /// Creates a new Federated SPARQL Endpoint by creating a new <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> for the given URI
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public FederatedSparqlRemoteEndpoint(Uri endpointUri)
        {
            this._endpoints.Add(new SparqlRemoteEndpoint(endpointUri));
        }

        /// <summary>
        /// Creates a new Federated SPARQL Endpoint by creating a <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> for each of the given URI
        /// </summary>
        /// <param name="endpointUris">Endpoint URIs</param>
        public FederatedSparqlRemoteEndpoint(IEnumerable<Uri> endpointUris)
        {
            this._endpoints.AddRange(endpointUris.Select(u => new SparqlRemoteEndpoint(u)));
        }

        /// <summary>
        /// Adds a additional endpoint to be used by this endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint</param>
        public void AddEndpoint(SparqlRemoteEndpoint endpoint)
        {
            this._endpoints.Add(endpoint);
        }

        /// <summary>
        /// Adds an additional endpoint to be used by this endpoint
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public void AddEndpoint(Uri endpointUri)
        {
            this._endpoints.Add(new SparqlRemoteEndpoint(endpointUri));
        }

        /// <summary>
        /// Removes a given endpoint from this endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint</param>
        public void RemoveEndpoint(SparqlRemoteEndpoint endpoint)
        {
            this._endpoints.Remove(endpoint);
        }

        /// <summary>
        /// Removes all endpoints with the given URI from this endpoint
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public void RemoveEndpoint(Uri endpointUri)
        {
            this._endpoints.RemoveAll(e => e.Uri.Equals(endpointUri));
        }

        /// <summary>
        /// Gets/Sets whether a failed request on one endpoint should cause the entire request to fail
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default if a request on any of the endpoint fails or times out then the entire request will fail
        /// </para>
        /// </remarks>
        public bool IgnoreFailedRequests
        {
            get
            {
                return this._ignoreFailedRequests;
            }
            set
            {
                this._ignoreFailedRequests = value;
            }
        }

        /// <summary>
        /// Gets/Sets the maximum number of endpoints this endpoint will issue queries to at any one time
        /// </summary>
        public int MaxSimultaneousRequests
        {
            get
            {
                return this._maxSimultaneousRequests;
            }
            set
            {
                if (value >= 1)
                {
                    this._maxSimultaneousRequests = value;
                }
            }
        }

        /// <summary>
        /// Makes a Query to a Sparql Endpoint and returns the raw Response
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query String</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown if more than one endpoint is in use since for any federated endpoint which used more than one endpoint there is no logical/sensible way to combine the result streams</exception>
        public override HttpWebResponse QueryRaw(string sparqlQuery)
        {
            //If we only have a single endpoint then we can still return a raw response
            if (this._endpoints.Count == 1) return this._endpoints[0].QueryRaw(sparqlQuery);

            //If we have any other number of endpoints we either have no responses or no way of logically/sensibly combining responses
            throw new NotSupportedException("Raw Query is not supported by the Federated Remote Endpoint");
        }

        /// <summary>
        /// Makes a Query to a Sparql Endpoint and returns the raw Response
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query String</param>
        /// <param name="mimeTypes">MIME Types to use for the Accept Header</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown if more than one endpoint is in use since for any federated endpoint which used more than one endpoint there is no logical/sensible way to combine the result streams</exception>
        public override HttpWebResponse QueryRaw(string sparqlQuery, string[] mimeTypes)
        {
            //If we only have a single endpoint then we can still return a raw response
            if (this._endpoints.Count == 1) return this._endpoints[0].QueryRaw(sparqlQuery, mimeTypes);

            //If we have any other number of endpoints we either have no responses or no way of logically/sensibly combining responses
            throw new NotSupportedException("Raw Query is not supported by the Federated Remote Endpoint");
        }

        /// <summary>
        /// Makes a Query where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query String</param>
        /// <returns>RDF Graph</returns>
        /// <remarks>
        /// <para>
        /// The query is executed by sending it federating it to all the endpoints this endpoint contains using simultaneous asychronous calls.  Once these calls complete the results are naivley merged together (no duplicate data removal) and returned as a single result.
        /// </para>
        /// <para>
        /// By default if any of the endpoints used return an error then the entire query will fail and an exception will be thrown, this behaviour can be overridden by setting the <see cref="FederatedSparqlRemoteEndpoint.IgnoreFailedRequests">IgnoreFailedRequests</see> property to be true in which case the result will be the merge of the results from all endpoints which successfully provided a result.
        /// </para>
        /// </remarks>
        /// <exception cref="RdfQueryException">Thrown if any of the requests to the endpoints fail</exception>
        /// <exception cref="RdfQueryTimeoutException">Thrown if not all the requests complete within the set timeout</exception>
        public override IGraph QueryWithResultGraph(string sparqlQuery)
        {
            //If no endpoints return an Empty Graph
            if (this._endpoints.Count == 0) return new Graph();

            Graph g = new Graph();
            this.QueryWithResultGraph(new GraphHandler(g), sparqlQuery);
            return g;
        }

        /// <summary>
        /// Makes a Query where the expected result is a Graph i.e. a CONSTRUCT or DESCRIBE query
        /// </summary>
        /// <param name="handler">RDF Handler to process the results</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public override void QueryWithResultGraph(IRdfHandler handler, string sparqlQuery)
        {
            //If no endpoints do nothing
            if (this._endpoints.Count == 0) return;

            //Fire off all the Asychronous Requests
            List<AsyncQueryWithResultGraph> asyncCalls = new List<AsyncQueryWithResultGraph>();
            List<IAsyncResult> asyncResults = new List<IAsyncResult>();
            int count = 0;
            foreach (SparqlRemoteEndpoint endpoint in this._endpoints)
            {
                //Limit the number of simultaneous requests we make to the user defined level (default 4)
                //We do this limiting check before trying to issue a request so that when the last request
                //is issued we'll always drop out of the loop and move onto our WaitAll()
                while (count >= this._maxSimultaneousRequests)
                {
                    //First check that the count of active requests is accurate
                    int active = asyncResults.Count(r => !r.IsCompleted);
                    if (active < count)
                    {
                        //Some of the requests have already completed so we don't need to wait
                        count = active;
                        break;
                    }
                    else if (active > count)
                    {
                        //There are more active requests then we thought
                        count = active;
                    }

                    //While the number of requests is at/above the maximum we'll wait for any of the requests to finish
                    //Then we can decrement the count and if this drops back below our maximum then we'll go back into the
                    //main loop and fire off our next request
                    WaitHandle.WaitAny(asyncResults.Select(r => r.AsyncWaitHandle).ToArray());
                    count--;
                }

                //Make an asynchronous query to the next endpoint
                AsyncQueryWithResultGraph d = new AsyncQueryWithResultGraph(endpoint.QueryWithResultGraph);
                asyncCalls.Add(d);
                IAsyncResult asyncResult = d.BeginInvoke(sparqlQuery, null, null);
                asyncResults.Add(asyncResult);
                count++;
            }

            //Wait for all our requests to finish
            int waitTimeout = (base.Timeout > 0) ? base.Timeout : System.Threading.Timeout.Infinite;
            WaitHandle.WaitAll(asyncResults.Select(r => r.AsyncWaitHandle).ToArray(), waitTimeout);

            //Check for and handle timeouts
            if (!this._ignoreFailedRequests && !asyncResults.All(r => r.IsCompleted))
            {
                for (int i = 0; i < asyncCalls.Count; i++)
                {
                    try
                    {
                        asyncCalls[i].EndInvoke(asyncResults[i]);
                    }
                    catch
                    {
                        //Exceptions don't matter as we're just ensuring all the EndInvoke() calls are made
                    }
                }
                throw new RdfQueryTimeoutException("Federated Querying failed due to one/more endpoints failing to return results within the Timeout specified which is currently " + (base.Timeout / 1000) + " seconds");
            }

            //Now merge all the results together
            HashSet<String> varsSeen = new HashSet<string>();
            bool cont = true;
            for (int i = 0; i < asyncCalls.Count; i++)
            {
                //Retrieve the result for this call
                AsyncQueryWithResultGraph call = asyncCalls[i];
                IGraph g;
                try
                {
                    g = call.EndInvoke(asyncResults[i]);
                }
                catch (Exception ex)
                {
                    if (!this._ignoreFailedRequests)
                    {
                        //Clean up in the event of an error
                        for (int j = i + 1; j < asyncCalls.Count; j++)
                        {
                            try
                            {
                                asyncCalls[j].EndInvoke(asyncResults[j]);
                            }
                            catch
                            {
                                //Exceptions don't matter as we're just ensuring all the EndInvoke() calls are made
                            }
                        }

                        //If a single request fails then the entire query fails
                        throw new RdfQueryException("Federated Querying failed due to the query against the endpoint '" + this._endpoints[i] + "' failing", ex);
                    }
                    else
                    {
                        //If we're ignoring failed requests we continue here
                        continue;
                    }
                }

                //Merge the result into the final results
                //If the handler has previously told us to stop we skip this step
                if (cont)
                {
                    handler.StartRdf();
                    foreach (Triple t in g.Triples)
                    {
                        cont = handler.HandleTriple(t);
                        //Stop if the Handler tells us to
                        if (!cont) break;
                    }
                    handler.EndRdf(true);
                }
            }
        }

        /// <summary>
        /// Makes a Query where the expected Result is a SparqlResultSet ie. SELECT and ASK Queries
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query String</param>
        /// <returns>A Sparql Result Set</returns>
        /// <remarks>
        /// <para>
        /// The query is executed by sending it federating it to all the endpoints this endpoint contains using simultaneous asychronous calls.  Once these calls complete the results are naivley merged together (no duplicate data removal) and returned as a single result.
        /// </para>
        /// <para>
        /// By default if any of the endpoints used return an error then the entire query will fail and an exception will be thrown, this behaviour can be overridden by setting the <see cref="FederatedSparqlRemoteEndpoint.IgnoreFailedRequests">IgnoreFailedRequests</see> property to be true in which case the result will be the merge of the results from all endpoints which successfully provided a result.
        /// </para>
        /// </remarks>
        /// <exception cref="RdfQueryException">Thrown if any of the requests to the endpoints fail</exception>
        /// <exception cref="RdfQueryTimeoutException">Thrown if not all the requests complete within the set timeout</exception>
        public override SparqlResultSet QueryWithResultSet(string sparqlQuery)
        {
            //If no endpoints return an empty Result Set
            if (this._endpoints.Count == 0) return new SparqlResultSet();

            //Declare a result set and invoke the other overload
            SparqlResultSet results = new SparqlResultSet();
            this.QueryWithResultSet(new MergingResultSetHandler(results), sparqlQuery);
            return results;
        }

        /// <summary>
        /// Makes a Query where the expected Result is a SparqlResultSet ie. SELECT and ASK Queries
        /// </summary>
        /// <param name="handler">Results Handler to process the results</param>
        /// <param name="sparqlQuery">SPARQL Query String</param>
        /// <exception cref="RdfQueryException">Thrown if any of the requests to the endpoints fail</exception>
        /// <exception cref="RdfQueryTimeoutException">Thrown if not all the requests complete within the set timeout</exception>
        public override void QueryWithResultSet(ISparqlResultsHandler handler, string sparqlQuery)
        {
            //If no endpoints do nothing
            if (this._endpoints.Count == 0) return;

            //Fire off all the Asychronous Requests
            List<AsyncQueryWithResultSet> asyncCalls = new List<AsyncQueryWithResultSet>();
            List<IAsyncResult> asyncResults = new List<IAsyncResult>();
            int count = 0;
            foreach (SparqlRemoteEndpoint endpoint in this._endpoints)
            {
                //Limit the number of simultaneous requests we make to the user defined level (default 4)
                //We do this limiting check before trying to issue a request so that when the last request
                //is issued we'll always drop out of the loop and move onto our WaitAll()
                while (count >= this._maxSimultaneousRequests)
                {
                    //First check that the count of active requests is accurate
                    int active = asyncResults.Count(r => !r.IsCompleted);
                    if (active < count)
                    {
                        //Some of the requests have already completed so we don't need to wait
                        count = active;
                        break;
                    }
                    else if (active > count)
                    {
                        //There are more active requests then we thought
                        count = active;
                    }

                    //While the number of requests is at/above the maximum we'll wait for any of the requests to finish
                    //Then we can decrement the count and if this drops back below our maximum then we'll go back into the
                    //main loop and fire off our next request
                    WaitHandle.WaitAny(asyncResults.Select(r => r.AsyncWaitHandle).ToArray());
                    count--;
                }

                //Make an asynchronous query to the next endpoint
                AsyncQueryWithResultSet d = new AsyncQueryWithResultSet(endpoint.QueryWithResultSet);
                asyncCalls.Add(d);
                IAsyncResult asyncResult = d.BeginInvoke(sparqlQuery, null, null);
                asyncResults.Add(asyncResult);
                count++;
            }

            //Wait for all our requests to finish
            int waitTimeout = (base.Timeout > 0) ? base.Timeout : System.Threading.Timeout.Infinite;
            WaitHandle.WaitAll(asyncResults.Select(r => r.AsyncWaitHandle).ToArray(), waitTimeout);

            //Check for and handle timeouts
            if (!this._ignoreFailedRequests && !asyncResults.All(r => r.IsCompleted))
            {
                for (int i = 0; i < asyncCalls.Count; i++)
                {
                    try
                    {
                        asyncCalls[i].EndInvoke(asyncResults[i]);
                    }
                    catch
                    {
                        //Exceptions don't matter as we're just ensuring all the EndInvoke() calls are made
                    }
                }
                throw new RdfQueryTimeoutException("Federated Querying failed due to one/more endpoints failing to return results within the Timeout specified which is currently " + (base.Timeout / 1000) + " seconds");
            }

            //Now merge all the results together
            HashSet<String> varsSeen = new HashSet<string>();
            bool cont = true;
            for (int i = 0; i < asyncCalls.Count; i++)
            {
                //Retrieve the result for this call
                AsyncQueryWithResultSet call = asyncCalls[i];
                SparqlResultSet partialResult;
                try
                {
                    partialResult = call.EndInvoke(asyncResults[i]);
                }
                catch (Exception ex)
                {
                    if (!this._ignoreFailedRequests)
                    {
                        //Clean up in the event of an error
                        for (int j = i + 1; j < asyncCalls.Count; j++)
                        {
                            try
                            {
                                asyncCalls[j].EndInvoke(asyncResults[j]);
                            }
                            catch
                            {
                                //Exceptions don't matter as we're just ensuring all the EndInvoke() calls are made
                            }
                        }

                        //If a single request fails then the entire query fails
                        throw new RdfQueryException("Federated Querying failed due to the query against the endpoint '" + this._endpoints[i].Uri.ToString() + "' failing", ex);
                    }
                    else
                    {
                        //If we're ignoring failed requests we continue here
                        continue;
                    }
                }

                //Merge the result into the final results
                //If the handler has previously told us to stop we skip this step
                if (cont)
                {
                    foreach (String variable in partialResult.Variables)
                    {
                        cont = handler.HandleVariable(variable);
                        //Stop if handler tells us to
                        if (!cont) break;
                    }

                    if (cont)
                    {
                        foreach (SparqlResult result in partialResult.Results)
                        {
                            cont = handler.HandleResult(result);
                            //Stop if handler tells us to
                            if (!cont) break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Serializes the Endpoint's Configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode endpointObj = context.NextSubject;
            INode endpointClass = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassSparqlEndpoint);
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode endpoint = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyEndpoint);

            context.Graph.Assert(new Triple(endpointObj, rdfType, endpointClass));
            context.Graph.Assert(new Triple(endpointObj, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            foreach (SparqlRemoteEndpoint ep in this._endpoints)
            {
                //Serialize the child endpoint configuration
                INode epObj = context.Graph.CreateBlankNode();
                context.NextSubject = epObj;
                ep.SerializeConfiguration(context);

                //Link that serialization to this serialization
                context.Graph.Assert(new Triple(endpointObj, endpoint, epObj));
            }
        }
    }
}

#endif
