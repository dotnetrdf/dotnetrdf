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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Query;

/// <summary>
/// A class for connecting to multiple remote SPARQL endpoints and federating queries over them with the
/// results merged locally.
/// </summary>
/// <remarks>
/// <para>
/// Queries are federated by executing multiple requests simultaneously and asynchronously against the endpoints
/// in question with the data then merged locally.  The merging process does not attempt to remove duplicate data
/// it just naively merges the data.
/// </para>
/// </remarks>
public class FederatedSparqlQueryClient : ISparqlQueryClient, IConfigurationSerializable
{
    private readonly List<SparqlQueryClient> _endpoints = new();

    /// <summary>
    /// Get or set whether a failed request on one endpoint should cause the entire request to fail.
    /// </summary>
    public bool IgnoreFailedRequests { get; set; }

    /// <summary>
    /// Get or set the maximum number of endpoints that this client will issue queries to at any one time.
    /// </summary>
    public int MaxSimultaneousRequests { get; set; } = 4;


    /// <summary>
    /// Get or set the period of time to wait for a response from any given endpoint.
    /// </summary>
    public int Timeout { get; set; } = 30000;

    /// <summary>
    /// Create a new federated client that connects to a number of remote SPARQL endpoints.
    /// </summary>
    /// <param name="endpointClients">The remote endpoint clients to be federated.</param>
    public FederatedSparqlQueryClient(params SparqlQueryClient[] endpointClients)
    {
        _endpoints.AddRange(endpointClients);
    }

    /// <summary>
    /// Create a new federated client that connects to a number of remote SPARQL endpoints.
    /// </summary>
    /// <param name="endpointClients">The remote endpoint clients to be federated.</param>
    public FederatedSparqlQueryClient(IEnumerable<SparqlQueryClient> endpointClients)
    {
        _endpoints.AddRange(endpointClients);
    }

    /// <summary>
    /// Create a new federated client connecting to a number of remote SPARQL endpoints.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for all connections.</param>
    /// <param name="endpointUris">The URIs to connect to.</param>
    public FederatedSparqlQueryClient(HttpClient httpClient, params Uri[] endpointUris)
    {
        _endpoints.AddRange(endpointUris.Select(x=>new SparqlQueryClient(httpClient, x)));
    }

    /// <summary>
    /// Add a remote endpoint using the specified client instance.
    /// </summary>
    /// <param name="endpointClient">The client to use to connect to the remote endpoint.</param>
    /// <remarks>This method allows callers to use a specific HttpClient for certain remote endpoints by first constructing a <see cref="SparqlQueryClient"/> instance and then passing it to this method.</remarks>
    public void AddEndpoint(SparqlQueryClient endpointClient)
    {
        _endpoints.Add(endpointClient);
    }

    /// <summary>
    /// Remove a remote endpoint.
    /// </summary>
    /// <param name="endpointUri">The URI of the endpoint to be removed.</param>
    public void RemoveEndpoint(Uri endpointUri)
    {
        _endpoints.RemoveAll(x => x.EndpointUri.Equals(endpointUri));
    }

    /// <summary>
    /// Remove a remote endpoint.
    /// </summary>
    /// <param name="endpointClient">The endpoint client to be removed from the federation.</param>
    public void RemoveEndpoint(SparqlQueryClient endpointClient)
    {
        _endpoints.Remove(endpointClient);
    }

    /// <summary>
    /// Execute a SPARQL query where the expected result is a graph - i.e. a CONSTRUCT or DESCRIBE query.
    /// </summary>
    /// <param name="sparqlQuery">The SPARQL query string.</param>
    /// <returns>An RDF graph containing the combined results received from all remote endpoints.</returns>
    public Task<IGraph> QueryWithResultGraphAsync(string sparqlQuery)
    {
        return QueryWithResultGraphAsync(sparqlQuery, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<IGraph> QueryWithResultGraphAsync(string sparqlQuery, CancellationToken cancellationToken)
    {
        var g = new Graph();

        // If no endpoints return an empty graph
        if (_endpoints.Count == 0) return g;

        await QueryWithResultGraphAsync(sparqlQuery, new GraphHandler(g), cancellationToken);
        return g;
    }

    /// <summary>
    /// Execute a SPARQL query where the expected result is a graph - i.e. a CONSTRUCT or DESCRIBE query.
    /// </summary>
    /// <param name="sparqlQuery">The SPARQL query string.</param>
    /// <param name="handler">The handler to use when parsing the graph data returned by the server.</param>
    /// <returns>An RDF graph containing the combined results received from all remote endpoints.</returns>
    public async Task QueryWithResultGraphAsync(string sparqlQuery, IRdfHandler handler)
    {
        await QueryWithResultGraphAsync(sparqlQuery, handler, CancellationToken.None);
    }

    /// <summary>
    /// Execute a SPARQL query where the expected result is a graph - i.e. a CONSTRUCT or DESCRIBE query.
    /// </summary>
    /// <param name="sparqlQuery">The SPARQL query string.</param>
    /// <param name="handler">The handler to use when parsing the graph data returned by the server.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>An RDF graph containing the combined results received from all remote endpoints.</returns>
    public async Task QueryWithResultGraphAsync(string sparqlQuery, IRdfHandler handler,
        CancellationToken cancellationToken)
    {
        if (_endpoints.Count == 0) return;

        var asyncCalls = new List<Task<IGraph>>(_endpoints.Count);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var throttler = new SemaphoreSlim(MaxSimultaneousRequests);
        foreach (SparqlQueryClient endpoint in _endpoints)
        {
            await throttler.WaitAsync(cts.Token);
            if (!cancellationToken.IsCancellationRequested)
            {
                asyncCalls.Add(Task.Run(async () =>
                {
                    try
                    {
                        // If a timeout is set, apply a task-specific cancellation token with that timeout
                        // otherwise use the federated query cancellation token.
                        if (Timeout <= 0)
                        {
                            return await endpoint.QueryWithResultGraphAsync(sparqlQuery, cts.Token);
                        }
                        var taskCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
                        taskCts.CancelAfter(Timeout);
                        return await endpoint.QueryWithResultGraphAsync(sparqlQuery, taskCts.Token);
                    }
                    catch (Exception)
                    {
                        if (!IgnoreFailedRequests)
                        {
                            cts.Cancel(false);
                        }
                        throw;
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }, cts.Token));
            }
        }

        try
        {
            await Task.WhenAll(asyncCalls);
        }
        catch (TaskCanceledException ex)
        {
            if (ex.CancellationToken == cts.Token || ex.CancellationToken == cancellationToken)
            {
                // the entire operation was cancelled
                throw;
            }

            if (!IgnoreFailedRequests)
            {
                var cancelledTaskIndex = asyncCalls.FindIndex(t => t.IsCanceled);
                if (cancelledTaskIndex >= 0)
                {
                    var endpointUri = _endpoints[cancelledTaskIndex].EndpointUri.AbsoluteUri;
                    throw new RdfQueryTimeoutException(
                        $"Federated query failed due to a timeout against the endpoint '{endpointUri}'.");
                }
            }
        }
        catch (Exception)
        {
            if (!IgnoreFailedRequests)
            {
                // Report the first failed request in the task list
                var faultedTaskIndex = asyncCalls.FindIndex(t => t.IsFaulted);
                var faultedEndpointUri = _endpoints[faultedTaskIndex].EndpointUri.AbsoluteUri;
                throw new RdfQueryException(
                    $"Federated query failed due to the query against endpoint '{faultedEndpointUri}' failing.",
                    asyncCalls[faultedTaskIndex].Exception?.InnerException);
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
                if (!IgnoreFailedRequests)
                {
                    throw new RdfQueryException(
                        $"Federated Querying failed due to the query against the endpoint '{_endpoints[i]}' failing", ex);
                }
            }
        }
    }

    /// <summary>
    /// Run a federated query asynchronously.
    /// </summary>
    /// <param name="sparqlQuery">The query to be executed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>The Task object representing the asynchronous operation.</returns>
    /// <remarks>The query results are returned as a result of the task.</remarks>
    public async Task<SparqlResultSet> QueryWithResultSetAsync(string sparqlQuery, CancellationToken cancellationToken = default)
    {
        var results = new SparqlResultSet();
        await QueryWithResultSetAsync(sparqlQuery, new ResultSetHandler(results), cancellationToken);
        return results;
    }

    /// <summary>
    /// Run a federated query asynchronously.
    /// </summary>
    /// <param name="sparqlQuery"></param>
    /// <param name="handler"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The Task object representing the asynchronous operation.</returns>
    /// <remarks>The query results are reported through the specified <see cref="ISparqlResultsHandler"/>.</remarks>
    public async Task QueryWithResultSetAsync(string sparqlQuery, ISparqlResultsHandler handler,
        CancellationToken cancellationToken = default)
    {
        if(_endpoints.Count == 0) return;
        var asyncCalls = new List<Task<SparqlResultSet>>(_endpoints.Count);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var throttler = new SemaphoreSlim(MaxSimultaneousRequests);
        foreach (SparqlQueryClient endpoint in _endpoints)
        {
            await throttler.WaitAsync(cts.Token);
            if (!cancellationToken.IsCancellationRequested)
            {
                asyncCalls.Add(Task.Run(async () =>
                {
                    try
                    {
                        // If a timeout is set, apply a task-specific cancellation token with that timeout
                        // otherwise use the federated query cancellation token.
                        if (Timeout <= 0)
                        {
                            return await endpoint.QueryWithResultSetAsync(sparqlQuery, cts.Token);
                        }
                        var taskCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
                        taskCts.CancelAfter(Timeout);
                        return await endpoint.QueryWithResultSetAsync(sparqlQuery, taskCts.Token);
                    }
                    catch (Exception)
                    {
                        if (!IgnoreFailedRequests)
                        {
                            cts.Cancel(false);
                        }
                        throw;
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }, cts.Token));
            }
        }

        try
        {
            await Task.WhenAll(asyncCalls);
        }
        catch (TaskCanceledException ex)
        {
            if (ex.CancellationToken == cts.Token || ex.CancellationToken == cancellationToken)
            {
                // the entire operation was cancelled
                throw;
            }

            if (!IgnoreFailedRequests)
            {
                var cancelledTaskIndex = asyncCalls.FindIndex(t => t.IsCanceled);
                if (cancelledTaskIndex >= 0)
                {
                    var endpointUri = _endpoints[cancelledTaskIndex].EndpointUri.AbsoluteUri;
                    throw new RdfQueryTimeoutException(
                        $"Federated query failed due to a timeout against the endpoint '{endpointUri}'.");
                }
            }
        }
        catch (Exception)
        {
            if (!IgnoreFailedRequests)
            {
                // Report the first failed request in the task list
                var faultedTaskIndex = asyncCalls.FindIndex(t => t.IsFaulted);
                if (faultedTaskIndex >= 0)
                {
                    var faultedEndpointUri = _endpoints[faultedTaskIndex].EndpointUri.AbsoluteUri;
                    throw new RdfQueryException(
                        $"Federated query failed due to the query against endpoint '{faultedEndpointUri}' failing.",
                        asyncCalls[faultedTaskIndex].Exception?.InnerException);
                }
            }
        }

        // Now merge all the results together
        var cont = true;
        handler.StartResults();

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
                if (!IgnoreFailedRequests)
                {
                    // If a single request fails then the entire query fails
                    throw new RdfQueryException(
                        "Federated querying failed due to the query against the endpoint '" +
                        _endpoints[i].EndpointUri.AbsoluteUri + "' failing", ex);
                }

                // If we're ignoring failed requests we continue here
                continue;
            }

            // Merge the result into the final results
            // If the handler has previously told us to stop we skip this step
            if (cont)
            {
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
            }
        }
        handler.EndResults(cont);

    }

    /// <summary>
    /// Serializes the Endpoint's Configuration.
    /// </summary>
    /// <param name="context">Configuration Serialization Context.</param>
    public void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode endpoint = context.NextSubject;
        INode endpointClass = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassFederatedSparqlQueryClient));
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode endpointProp = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyQueryEndpoint));

        context.Graph.Assert(new Triple(endpoint, rdfType, endpointClass));
        context.Graph.Assert(new Triple(endpoint, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
        foreach (SparqlQueryClient ep in _endpoints)
        {
            // Serialize the child endpoint configuration
            INode epObj = context.Graph.CreateBlankNode();
            context.NextSubject = epObj;
            ep.SerializeConfiguration(context);

            // Link that serialization to this serialization
            context.Graph.Assert(new Triple(endpoint, endpointProp, epObj));
        }
    }
}
