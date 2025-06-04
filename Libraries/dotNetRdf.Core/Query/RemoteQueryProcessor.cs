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
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;

/// <summary>
/// A SPARQL Query Processor where the query is processed by passing it to a remote SPARQL endpoint.
/// </summary>
public class RemoteQueryProcessor
    : ISparqlQueryProcessor
{
    [Obsolete]
    private readonly SparqlRemoteEndpoint _endpoint;
    private readonly ISparqlQueryClient _client;
    private readonly SparqlFormatter _formatter = new SparqlFormatter();

    /// <summary>
    /// Creates a new Remote Query Processor.
    /// </summary>
    /// <param name="endpoint">SPARQL Endpoint.</param>
    [Obsolete(
        "This constructor is obsolete and will be removed in a future version. Use the constructor that accepts a SparqlClient")]
    public RemoteQueryProcessor(SparqlRemoteEndpoint endpoint)
    {
        _endpoint = endpoint;
    }

    /// <summary>
    /// Create a new Remote Query Processor.
    /// </summary>
    /// <param name="client">The remote SPARQL client to use.</param>
    public RemoteQueryProcessor(ISparqlQueryClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Processes a SPARQL Query.
    /// </summary>
    /// <param name="query">SPARQL Query.</param>
    /// <returns></returns>
    public object ProcessQuery(SparqlQuery query)
    {
        query.QueryExecutionTime = null;
        DateTime start = DateTime.Now;
        try
        {
            object temp;
            switch (query.QueryType)
            {
                case SparqlQueryType.Ask:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    temp = _client != null
                        ? Task.Run(()=>_client.QueryWithResultSetAsync(_formatter.Format(query), CancellationToken.None)).Result
                        : _endpoint.QueryWithResultSet(_formatter.Format(query));
                    break;
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    temp = _client != null
                        ? Task.Run(()=>_client.QueryWithResultGraphAsync(_formatter.Format(query), CancellationToken.None)).Result
                        : _endpoint.QueryWithResultGraph(_formatter.Format(query));
                    break;
                default:
                    throw new RdfQueryException(
                        "Unable to execute an unknown query type against a Remote Endpoint");
            }

            return temp;
        }
        finally
        {
            TimeSpan elapsed = DateTime.Now - start;
            query.QueryExecutionTime = elapsed;
        }
    }

    /// <summary>
    /// Processes a SPARQL Query passing the results to the RDF or Results handler as appropriate.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="query">SPARQL Query.</param>
    public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
    {
        query.QueryExecutionTime = null;
        DateTime start = DateTime.Now;
        try
        {
            switch (query.QueryType)
            {
                case SparqlQueryType.Ask:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    if (_client != null)
                    {
                        _client.QueryWithResultSetAsync(_formatter.Format(query), resultsHandler,
                            CancellationToken.None).Wait();
                    }
                    else
                    {
                        _endpoint.QueryWithResultSet(resultsHandler, _formatter.Format(query));
                    }

                    break;
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    if (_client != null)
                    {
                        _client.QueryWithResultGraphAsync(_formatter.Format(query), rdfHandler,
                            CancellationToken.None).Wait();
                    }
                    else
                    {
                        _endpoint.QueryWithResultGraph(rdfHandler, _formatter.Format(query));
                    }

                    break;
                default:
                    throw new RdfQueryException(
                        "Unable to execute an unknown query type against a Remote Endpoint");
            }
        }
        finally
        {
            TimeSpan elapsed = DateTime.Now - start;
            query.QueryExecutionTime = elapsed;
        }
    }

    /// <summary>
    /// Processes a SPARQL Query asynchronously invoking the relevant callback when the query completes.
    /// </summary>
    /// <param name="query">SPARQL QUery.</param>
    /// <param name="rdfCallback">Callback for queries that return a Graph.</param>
    /// <param name="resultsCallback">Callback for queries that return a Result Set.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <remarks>
    /// In the event of a success the appropriate callback will be invoked, if there is an error both callbacks will be invoked and passed an instance of <see cref="AsyncError"/> which contains details of the error and the original state information passed in.
    /// </remarks>
    public void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback,
        object state)
    {
        query.QueryExecutionTime = null;
        DateTime start = DateTime.Now;
        try
        {
            switch (query.QueryType)
            {
                case SparqlQueryType.Ask:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    if (_client != null)
                    {
                        _client.QueryWithResultSetAsync(_formatter.Format(query), CancellationToken.None)
                            .ContinueWith(
                                queryTask =>
                                {
                                    if (queryTask.IsCanceled)
                                    {
                                        resultsCallback(null,
                                            new AsyncError(new RdfQueryException("The operation was cancelled"),
                                                state));
                                    }
                                    else if (queryTask.IsFaulted)
                                    {
                                        resultsCallback(null, new AsyncError(queryTask.Exception, state));
                                    }
                                    else
                                    {
                                        resultsCallback(queryTask.Result, state);
                                    }
                                });
                    }
                    else
                    {
                        _endpoint.QueryWithResultSet(_formatter.Format(query), resultsCallback, state);
                    }

                    break;
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    if (_client != null)
                    {
                        _client.QueryWithResultGraphAsync(_formatter.Format(query), CancellationToken.None)
                            .ContinueWith(
                                queryTask =>
                                {
                                    if (queryTask.IsCanceled)
                                    {
                                        rdfCallback(null,
                                            new AsyncError(new RdfQueryException("The operation was cancelled"),
                                                state));
                                    }
                                    else if (queryTask.IsFaulted)
                                    {
                                        rdfCallback(null, new AsyncError(queryTask.Exception, state));
                                    }
                                    else
                                    {
                                        rdfCallback(queryTask.Result, state);
                                    }
                                });
                    }
                    else
                    {
                        _endpoint.QueryWithResultGraph(_formatter.Format(query), rdfCallback, state);
                    }

                    break;
                default:
                    throw new RdfQueryException(
                        "Unable to execute an unknown query type against a Remote Endpoint");
            }
        }
        finally
        {
            TimeSpan elapsed = DateTime.Now - start;
            query.QueryExecutionTime = elapsed;
        }
    }

    /// <summary>
    /// Processes a SPARQL Query asynchronously passing the results to the relevant handler and invoking the callback when the query completes.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="query">SPARQL Query.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <remarks>
    /// In the event of a success the callback will be invoked, if there is an error the callback will be invoked and passed an instance of <see cref="AsyncError"/> which contains details of the error and the original state information passed in.
    /// </remarks>
    public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query,
        QueryCallback callback, object state)
    {
        query.QueryExecutionTime = null;
        DateTime start = DateTime.Now;
        try
        {
            switch (query.QueryType)
            {
                case SparqlQueryType.Ask:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    if (_client != null)
                    {
                        _client.QueryWithResultSetAsync(_formatter.Format(query), resultsHandler,
                                CancellationToken.None)
                            .ContinueWith(
                                queryTask =>
                                {
                                    if (queryTask.IsCanceled)
                                    {
                                        callback(rdfHandler, resultsHandler,
                                            new AsyncError(new RdfQueryException("The operation was cancelled"),
                                                state));
                                    }
                                    else if (queryTask.IsFaulted)
                                    {
                                        callback(rdfHandler, resultsHandler,
                                            new AsyncError(queryTask.Exception, state));
                                    }
                                    else
                                    {
                                        callback(rdfHandler, resultsHandler, state);
                                    }
                                });
                    }
                    else
                    {
                        _endpoint.QueryWithResultSet(resultsHandler, _formatter.Format(query), callback, state);
                    }

                    break;
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    if (_client != null)
                    {
                        _client.QueryWithResultGraphAsync(_formatter.Format(query), rdfHandler,
                                CancellationToken.None)
                            .ContinueWith(
                                queryTask =>
                                {
                                    if (queryTask.IsCanceled)
                                    {
                                        callback(rdfHandler, resultsHandler,
                                            new AsyncError(new RdfQueryException("The operation was cancelled"),
                                                state));
                                    }
                                    else if (queryTask.IsFaulted)
                                    {
                                        callback(rdfHandler, resultsHandler,
                                            new AsyncError(queryTask.Exception, state));
                                    }
                                    else
                                    {
                                        callback(rdfHandler, resultsHandler, state);
                                    }
                                });
                    }
                    else
                    {
                        _endpoint.QueryWithResultGraph(rdfHandler, _formatter.Format(query), callback, state);
                    }

                    break;
                default:
                    throw new RdfQueryException(
                        "Unable to execute an unknown query type against a Remote Endpoint");
            }
        }
        finally
        {
            TimeSpan elapsed = (DateTime.Now - start);
            query.QueryExecutionTime = elapsed;
        }
    }

    /// <inheritdoc />
    public async Task<object> ProcessQueryAsync(SparqlQuery query)
    {
        query.QueryExecutionTime = null;
        DateTime start = DateTime.Now;
        try
        {
            switch (query.QueryType)
            {
                case SparqlQueryType.Ask:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    if (_client != null)
                    {
                        return await _client.QueryWithResultSetAsync(_formatter.Format(query),
                            CancellationToken.None);
                    }
                    else
                    {
                        return await Task.Factory.StartNew(() =>
                            _endpoint.QueryWithResultSet(_formatter.Format(query)));
                    }
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    if (_client != null)
                    {
                        return await _client.QueryWithResultGraphAsync(_formatter.Format(query),
                            CancellationToken.None);
                    }
                    else
                    {
                        return await Task.Factory.StartNew(() =>
                            _endpoint.QueryWithResultGraph(_formatter.Format(query)));
                    }
                default:
                    throw new RdfQueryException(
                        "Unable to execute an unknown query type against a Remote Endpoint");
            }
        }
        finally

        {
            TimeSpan elapsed = (DateTime.Now - start);
            query.QueryExecutionTime = elapsed;
        }
    }

    /// <inheritdoc />
    public Task ProcessQueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
    {
        query.QueryExecutionTime = null;
        DateTime start = DateTime.Now;
        try
        {
            switch (query.QueryType)
            {
                case SparqlQueryType.Ask:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    if (_client != null)
                    {
                        return _client.QueryWithResultSetAsync(_formatter.Format(query), resultsHandler,
                            CancellationToken.None);
                    }
                    else
                    {
                        return Task.Factory.StartNew(() =>
                            _endpoint.QueryWithResultSet(resultsHandler, _formatter.Format(query)));
                    }
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    if (_client != null)
                    {
                        return _client.QueryWithResultGraphAsync(_formatter.Format(query), rdfHandler,
                            CancellationToken.None);
                    }
                    else
                    {
                        return Task.Factory.StartNew(() =>
                            _endpoint.QueryWithResultGraph(rdfHandler, _formatter.Format(query)));
                    }
                default:
                    throw new RdfQueryException(
                        "Unable to execute an unknown query type against a Remote Endpoint");
            }
        }
        finally

        {
            TimeSpan elapsed = (DateTime.Now - start);
            query.QueryExecutionTime = elapsed;
        }
    }
}
