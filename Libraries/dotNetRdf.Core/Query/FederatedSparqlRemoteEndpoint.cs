/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Query;


/// <summary>
/// A Class for connecting to multiple remote SPARQL Endpoints and federating queries over them with the data merging done locally.
/// </summary>
/// <remarks>
/// <para>
/// Queries are federated by executing multiple requesting simultaneously and asynchronously against the endpoints in question with the data then merged locally.  The merging process does not attempt to remove duplicate data it just naively merges the data.
/// </para>
/// </remarks>
[Obsolete("Replaced by VDS.RDF.Query.FederatedSparqlQueryClient")]
public class FederatedSparqlRemoteEndpoint 
    : SparqlRemoteEndpoint
{
    private readonly List<SparqlRemoteEndpoint> _endpoints = new List<SparqlRemoteEndpoint>();
    private bool _ignoreFailedRequests = false;
    private int _maxSimultaneousRequests = 4;

    /// <summary>
    /// Creates a new Federated SPARQL Endpoint using a given Endpoint.
    /// </summary>
    /// <param name="endpoint">Endpoint.</param>
    public FederatedSparqlRemoteEndpoint(SparqlRemoteEndpoint endpoint)
    {
        _endpoints.Add(endpoint);
    }

    /// <summary>
    /// Creates a new Federated SPARQL Endpoint using the given Endpoints.
    /// </summary>
    /// <param name="endpoints">Endpoints.</param>
    public FederatedSparqlRemoteEndpoint(IEnumerable<SparqlRemoteEndpoint> endpoints)
    {
        _endpoints.AddRange(endpoints);
    }

    /// <summary>
    /// Creates a new Federated SPARQL Endpoint by creating a new <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> for the given URI.
    /// </summary>
    /// <param name="endpointUri">Endpoint URI.</param>
    public FederatedSparqlRemoteEndpoint(Uri endpointUri)
    {
        _endpoints.Add(new SparqlRemoteEndpoint(endpointUri));
    }

    /// <summary>
    /// Creates a new Federated SPARQL Endpoint by creating a <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> for each of the given URI.
    /// </summary>
    /// <param name="endpointUris">Endpoint URIs.</param>
    public FederatedSparqlRemoteEndpoint(IEnumerable<Uri> endpointUris)
    {
        _endpoints.AddRange(endpointUris.Select(u => new SparqlRemoteEndpoint(u)));
    }

    /// <summary>
    /// Adds a additional endpoint to be used by this endpoint.
    /// </summary>
    /// <param name="endpoint">Endpoint.</param>
    public void AddEndpoint(SparqlRemoteEndpoint endpoint)
    {
        _endpoints.Add(endpoint);
    }

    /// <summary>
    /// Adds an additional endpoint to be used by this endpoint.
    /// </summary>
    /// <param name="endpointUri">Endpoint URI.</param>
    public void AddEndpoint(Uri endpointUri)
    {
        _endpoints.Add(new SparqlRemoteEndpoint(endpointUri));
    }

    /// <summary>
    /// Removes a given endpoint from this endpoint.
    /// </summary>
    /// <param name="endpoint">Endpoint.</param>
    public void RemoveEndpoint(SparqlRemoteEndpoint endpoint)
    {
        _endpoints.Remove(endpoint);
    }

    /// <summary>
    /// Removes all endpoints with the given URI from this endpoint.
    /// </summary>
    /// <param name="endpointUri">Endpoint URI.</param>
    public void RemoveEndpoint(Uri endpointUri)
    {
        _endpoints.RemoveAll(e => e.Uri.Equals(endpointUri));
    }

    /// <summary>
    /// Gets/Sets whether a failed request on one endpoint should cause the entire request to fail.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default if a request on any of the endpoint fails or times out then the entire request will fail.
    /// </para>
    /// </remarks>
    public bool IgnoreFailedRequests
    {
        get
        {
            return _ignoreFailedRequests;
        }
        set
        {
            _ignoreFailedRequests = value;
        }
    }

    /// <summary>
    /// Gets/Sets the maximum number of endpoints this endpoint will issue queries to at any one time.
    /// </summary>
    public int MaxSimultaneousRequests
    {
        get
        {
            return _maxSimultaneousRequests;
        }
        set
        {
            if (value >= 1)
            {
                _maxSimultaneousRequests = value;
            }
        }
    }

    /// <summary>
    /// Makes a Query to a Sparql Endpoint and returns the raw Response.
    /// </summary>
    /// <param name="sparqlQuery">Sparql Query String.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Thrown if more than one endpoint is in use since for any federated endpoint which used more than one endpoint there is no logical/sensible way to combine the result streams.</exception>
    public override HttpWebResponse QueryRaw(string sparqlQuery)
    {
        // If we only have a single endpoint then we can still return a raw response
        if (_endpoints.Count == 1) return _endpoints[0].QueryRaw(sparqlQuery);

        // If we have any other number of endpoints we either have no responses or no way of logically/sensibly combining responses
        throw new NotSupportedException("Raw Query is not supported by the Federated Remote Endpoint");
    }

    /// <summary>
    /// Makes a Query to a Sparql Endpoint and returns the raw Response.
    /// </summary>
    /// <param name="sparqlQuery">Sparql Query String.</param>
    /// <param name="mimeTypes">MIME Types to use for the Accept Header.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Thrown if more than one endpoint is in use since for any federated endpoint which used more than one endpoint there is no logical/sensible way to combine the result streams.</exception>
    public override HttpWebResponse QueryRaw(string sparqlQuery, string[] mimeTypes)
    {
        // If we only have a single endpoint then we can still return a raw response
        if (_endpoints.Count == 1) return _endpoints[0].QueryRaw(sparqlQuery, mimeTypes);

        // If we have any other number of endpoints we either have no responses or no way of logically/sensibly combining responses
        throw new NotSupportedException("Raw Query is not supported by the Federated Remote Endpoint");
    }

    /// <summary>
    /// Makes a Query where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query String.</param>
    /// <returns>RDF Graph.</returns>
    /// <remarks>
    /// <para>
    /// The query is executed by sending it federating it to all the endpoints this endpoint contains using simultaneous asychronous calls.  Once these calls complete the results are naivley merged together (no duplicate data removal) and returned as a single result.
    /// </para>
    /// <para>
    /// By default if any of the endpoints used return an error then the entire query will fail and an exception will be thrown, this behaviour can be overridden by setting the <see cref="FederatedSparqlRemoteEndpoint.IgnoreFailedRequests">IgnoreFailedRequests</see> property to be true in which case the result will be the merge of the results from all endpoints which successfully provided a result.
    /// </para>
    /// </remarks>
    /// <exception cref="RdfQueryException">Thrown if any of the requests to the endpoints fail.</exception>
    /// <exception cref="RdfQueryTimeoutException">Thrown if not all the requests complete within the set timeout.</exception>
    public override IGraph QueryWithResultGraph(string sparqlQuery)
    {
        // If no endpoints return an Empty Graph
        if (_endpoints.Count == 0) return new Graph();

        var g = new Graph();
        QueryWithResultGraph(new GraphHandler(g), sparqlQuery);
        return g;
    }

    /// <summary>
    /// Makes a Query where the expected result is a Graph i.e. a CONSTRUCT or DESCRIBE query.
    /// </summary>
    /// <param name="handler">RDF Handler to process the results.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    public override void QueryWithResultGraph(IRdfHandler handler, string sparqlQuery)
    {
        // If no endpoints do nothing
        if (_endpoints.Count == 0) return;

        // Fire off all the asynchronous requests
        var asyncCalls = new List<Task<IGraph>>();
        var count = 0;
        var cts = new CancellationTokenSource();
        foreach (SparqlRemoteEndpoint endpoint in _endpoints)
        {
            // Limit the number of simultaneous requests we make to the user defined level (default 4)
            // We do this limiting check before trying to issue a request so that when the last request
            // is issued we'll always drop out of the loop and move onto our WaitAll()
            while (count >= _maxSimultaneousRequests)
            {
                // First check that the count of active requests is accurate
                var active = asyncCalls.Count(r => !r.IsCompleted);
                if (active < count)
                {
                    // Some of the requests have already completed so we don't need to wait
                    count = active;
                    break;
                }
                else if (active > count)
                {
                    // There are more active requests then we thought
                    count = active;
                }

                // While the number of requests is at/above the maximum we'll wait for any of the requests to finish
                // Then we can decrement the count and if this drops back below our maximum then we'll go back into the
                // main loop and fire off our next request
                try
                {
                    Task.WaitAny(asyncCalls.ToArray());
                }
                catch (AggregateException ex)
                {
                    var faultedTaskIx = asyncCalls.FindIndex(x => x.IsFaulted);
                    SparqlRemoteEndpoint faultedEndpoint = _endpoints[faultedTaskIx];
                    if (!_ignoreFailedRequests)
                    {
                        throw new RdfQueryException("Federated Querying failed due to the query against the endpoint '" + faultedEndpoint + "' failing", ex);
                    }
                }

                count--;
            }

            // Make an asynchronous query to the next endpoint
            asyncCalls.Add(Task.Factory.StartNew(()=>endpoint.QueryWithResultGraph(sparqlQuery), cts.Token));
            count++;
        }

        // Wait for all our requests to finish
        var waitTimeout = (Timeout > 0) ? Timeout : System.Threading.Timeout.Infinite;

        // Check for and handle timeouts
        try
        {
            if (!Task.WaitAll(asyncCalls.ToArray(), waitTimeout, cts.Token))
            {
                // Handle timeouts - cancel overrunning tasks and optionally throw an exception
                cts.Cancel(false);
                if (!_ignoreFailedRequests)
                {
                    throw new RdfQueryTimeoutException(
                        "Federated Querying failed due to one/more endpoints failing to return results within the Timeout specified which is currently " +
                        (Timeout / 1000) + " seconds");
                }
            }
        }
        catch (AggregateException ex)
        {
            if (!_ignoreFailedRequests)
            {
                // Try to determine which endpoint faulted
                var faultedTaskIndex = asyncCalls.FindIndex(t => t.IsFaulted);
                var faultedEndpointUri = _endpoints[faultedTaskIndex].Uri.AbsoluteUri;
                throw new RdfQueryException(
                    "Federated querying failed due to the query against the endpoint '" + faultedEndpointUri +
                    "' failing.", ex.InnerException);
            }
        }

        // Now merge all the results together
        var cont = true;
        for (var i = 0; i < asyncCalls.Count; i++)
        {
            if (!asyncCalls[i].IsCompleted)
            {
                // This is a timed out task that has not transitioned to cancelled state yet
                // We will only get here if _ignoreFailedRequests is true
                continue;
            }

            try
            {
                IGraph g = asyncCalls[i].Result;

                // Merge the result into the final results
                // If the handler has previously told us to stop we skip this step
                if (cont)
                {
                    handler.StartRdf();
                    foreach (Triple t in g.Triples)
                    {
                        cont = handler.HandleTriple(t);
                        // Stop if the Handler tells us to
                        if (!cont) break;
                    }
                    handler.EndRdf(true);
                }
            }
            catch (AggregateException ex)
            {
                if (!_ignoreFailedRequests)
                {
                    throw new RdfQueryException("Federated Querying failed due to the query against the endpoint '" + _endpoints[i] + "' failing", ex);
                }
            }
            

        }
    }

    /// <summary>
    /// Makes a Query where the expected Result is a SparqlResultSet ie. SELECT and ASK Queries.
    /// </summary>
    /// <param name="sparqlQuery">Sparql Query String.</param>
    /// <returns>A Sparql Result Set.</returns>
    /// <remarks>
    /// <para>
    /// The query is executed by sending it federating it to all the endpoints this endpoint contains using simultaneous asychronous calls.  Once these calls complete the results are naivley merged together (no duplicate data removal) and returned as a single result.
    /// </para>
    /// <para>
    /// By default if any of the endpoints used return an error then the entire query will fail and an exception will be thrown, this behaviour can be overridden by setting the <see cref="FederatedSparqlRemoteEndpoint.IgnoreFailedRequests">IgnoreFailedRequests</see> property to be true in which case the result will be the merge of the results from all endpoints which successfully provided a result.
    /// </para>
    /// </remarks>
    /// <exception cref="RdfQueryException">Thrown if any of the requests to the endpoints fail.</exception>
    /// <exception cref="RdfQueryTimeoutException">Thrown if not all the requests complete within the set timeout.</exception>
    public override SparqlResultSet QueryWithResultSet(string sparqlQuery)
    {
        // If no endpoints return an empty Result Set
        if (_endpoints.Count == 0) return new SparqlResultSet();

        // Declare a result set and invoke the other overload
        var results = new SparqlResultSet();
        QueryWithResultSet(new MergingResultSetHandler(results), sparqlQuery);
        return results;
    }

    /// <summary>
    /// Makes a Query where the expected Result is a SparqlResultSet ie. SELECT and ASK Queries.
    /// </summary>
    /// <param name="handler">Results Handler to process the results.</param>
    /// <param name="sparqlQuery">SPARQL Query String.</param>
    /// <exception cref="RdfQueryException">Thrown if any of the requests to the endpoints fail.</exception>
    /// <exception cref="RdfQueryTimeoutException">Thrown if not all the requests complete within the set timeout.</exception>
    public override void QueryWithResultSet(ISparqlResultsHandler handler, string sparqlQuery)
    {
        // If no endpoints do nothing
        if (_endpoints.Count == 0) return;

        // Fire off all the asynchronous Requests
        var asyncCalls = new List<Task<SparqlResultSet>>();
        var cts = new CancellationTokenSource();
        var count = 0;
        foreach (SparqlRemoteEndpoint endpoint in _endpoints)
        {
            // Limit the number of simultaneous requests we make to the user defined level (default 4)
            // We do this limiting check before trying to issue a request so that when the last request
            // is issued we'll always drop out of the loop and move onto our WaitAll()
            while (count >= _maxSimultaneousRequests)
            {
                // First check that the count of active requests is accurate
                var active = asyncCalls.Count(r => !r.IsCompleted);
                if (active < count)
                {
                    // Some of the requests have already completed so we don't need to wait
                    count = active;
                    break;
                }
                if (active > count)
                {
                    // There are more active requests then we thought
                    count = active;
                }

                // While the number of requests is at/above the maximum we'll wait for any of the requests to finish
                // Then we can decrement the count and if this drops back below our maximum then we'll go back into the
                // main loop and fire off our next request
                Task.WaitAny(asyncCalls.ToArray(), cts.Token);
                count--;
            }

            // Make an asynchronous query to the next endpoint
            asyncCalls.Add(Task.Factory.StartNew(() => endpoint.QueryWithResultSet(sparqlQuery), cts.Token));
            count++;
        }

        // Wait for all our requests to finish
        var waitTimeout = (Timeout > 0) ? Timeout : System.Threading.Timeout.Infinite;

        try
        {
            if (!Task.WaitAll(asyncCalls.ToArray(), waitTimeout, cts.Token))
            {
                // Handle timeouts - cancel overrunning tasks and optionally throw an exception
                cts.Cancel(false);
                if (!_ignoreFailedRequests)
                {
                    throw new RdfQueryTimeoutException(
                        "Federated Querying failed due to one/more endpoints failing to return results within the Timeout specified which is currently " +
                        (Timeout / 1000) + " seconds");
                }
            }
        }
        catch (AggregateException ex)
        {
            if (!_ignoreFailedRequests)
            {
                // Try to determine which endpoint faulted
                var faultedTaskIndex = asyncCalls.FindIndex(t => t.IsFaulted);
                var faultedEndpointUri = _endpoints[faultedTaskIndex].Uri.AbsoluteUri;
                throw new RdfQueryException(
                    "Federated querying failed due to the query against the endpoint '" + faultedEndpointUri +
                    "' failing.", ex.InnerException);
            }
        }

        // Now merge all the results together
        var cont = true;
        for (var i = 0; i < asyncCalls.Count; i++)
        {
            if (!asyncCalls[i].IsCompleted)
            {
                // This is a timed out task that has not transitioned to cancelled state yet
                // We will only get here if _ignoreFailedRequests is true
                continue;
            }

            // Retrieve the result for this call
            SparqlResultSet partialResult;
            try
            {
                partialResult = asyncCalls[i].Result;
            }
            catch (Exception ex)
            {
                if (!_ignoreFailedRequests)
                {
                    // If a single request fails then the entire query fails
                    throw new RdfQueryException(
                        "Federated querying failed due to the query against the endpoint '" +
                        _endpoints[i].Uri.AbsoluteUri + "' failing", ex);
                }

                // If we're ignoring failed requests we continue here
                continue;
            }

            // Merge the result into the final results
            // If the handler has previously told us to stop we skip this step
            if (cont)
            {
                handler.StartResults();
                foreach (var variable in partialResult.Variables)
                {
                    cont = handler.HandleVariable(variable);
                    // Stop if handler tells us to
                    if (!cont) break;
                }

                if (cont)
                {
                    foreach (SparqlResult result in partialResult.Results)
                    {
                        cont = handler.HandleResult(result);
                        // Stop if handler tells us to
                        if (!cont) break;
                    }
                }
                handler.EndResults(cont);
            }
        }
    }

    /// <summary>
    /// Serializes the Endpoint's Configuration.
    /// </summary>
    /// <param name="context">Configuration Serialization Context.</param>
    public override void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode endpointObj = context.NextSubject;
        INode endpointClass = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassSparqlQueryEndpoint));
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode endpoint = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyQueryEndpoint));

        context.Graph.Assert(new Triple(endpointObj, rdfType, endpointClass));
        context.Graph.Assert(new Triple(endpointObj, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
        foreach (SparqlRemoteEndpoint ep in _endpoints)
        {
            // Serialize the child endpoint configuration
            INode epObj = context.Graph.CreateBlankNode();
            context.NextSubject = epObj;
            ep.SerializeConfiguration(context);

            // Link that serialization to this serialization
            context.Graph.Assert(new Triple(endpointObj, endpoint, epObj));
        }
    }
}
