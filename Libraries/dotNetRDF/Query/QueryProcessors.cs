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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    /// <summary>
    /// A SPARQL Query Processor where the query is processed by passing it to the <see cref="INativelyQueryableStore.ExecuteQuery(string)">ExecuteQuery()</see> method of an <see cref="INativelyQueryableStore">INativelyQueryableStore</see>
    /// </summary>
    public class SimpleQueryProcessor 
        : ISparqlQueryProcessor
    {
        private INativelyQueryableStore _store;
        private SparqlFormatter _formatter = new SparqlFormatter();

        /// <summary>
        /// Creates a new Simple Query Processor
        /// </summary>
        /// <param name="store">Triple Store</param>
        public SimpleQueryProcessor(INativelyQueryableStore store)
        {
            _store = store;
        }

        /// <summary>
        /// Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public object ProcessQuery(SparqlQuery query)
        {
            query.QueryExecutionTime = null;
            DateTime start = DateTime.Now;
            try
            {
                Object temp = _store.ExecuteQuery(_formatter.Format(query));
                return temp;
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = elapsed;
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
                _store.ExecuteQuery(rdfHandler, resultsHandler, _formatter.Format(query));
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = elapsed;
            }
        }

        /// <summary>
        /// Delegate used for asychronous execution
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        private delegate void ProcessQueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query);

        /// <summary>
        /// Processes a SPARQL Query asynchronously invoking the relevant callback when the query completes
        /// </summary>
        /// <param name="query">SPARQL QUery</param>
        /// <param name="rdfCallback">Callback for queries that return a Graph</param>
        /// <param name="resultsCallback">Callback for queries that return a Result Set</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// In the event of a success the appropriate callback will be invoked, if there is an error both callbacks will be invoked and passed an instance of <see cref="AsyncError"/> which contains details of the error and the original state information passed in.
        /// </remarks>
        public void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback, Object state)
        {
            Graph g = new Graph();
            SparqlResultSet rset = new SparqlResultSet();
            ProcessQueryAsync d = new ProcessQueryAsync(ProcessQuery);
            d.BeginInvoke(new GraphHandler(g), new ResultSetHandler(rset), query, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        if (rset.ResultsType != SparqlResultsType.Unknown)
                        {
                            resultsCallback(rset, state);
                        }
                        else
                        {
                            rdfCallback(g, state);
                        }
                    }
                    catch (RdfQueryException queryEx)
                    {
                        if (rdfCallback != null) rdfCallback(null, new AsyncError(queryEx, state));
                        if (resultsCallback != null) resultsCallback(null, new AsyncError(queryEx, state));
                    }
                    catch (Exception ex)
                    {
                        RdfQueryException queryEx = new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex);
                        if (rdfCallback != null) rdfCallback(null, new AsyncError(queryEx, state));
                        if (resultsCallback != null) resultsCallback(null, new AsyncError(queryEx, state));
                    }
                }, state);
        }

        /// <summary>
        /// Processes a SPARQL Query asynchronously passing the results to the relevant handler and invoking the callback when the query completes
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// In the event of a success the callback will be invoked normally, if there is an error the callback will be invoked and passed an instance of <see cref="AsyncError"/> which contains details of the error and the original state information passed in.
        /// </remarks>
        public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query, QueryCallback callback, Object state)
        {
            ProcessQueryAsync d = new ProcessQueryAsync(ProcessQuery);
            d.BeginInvoke(rdfHandler, resultsHandler, query, r =>
            {
                try
                {
                    d.EndInvoke(r);
                    callback(rdfHandler, resultsHandler, state);
                }
                catch (RdfQueryException queryEx)
                {
                    callback(rdfHandler, resultsHandler, new AsyncError(queryEx, state));
                }
                catch (Exception ex)
                {
                    callback(rdfHandler, resultsHandler, new AsyncError(new RdfQueryException("Unexpected error making an asynchronous query", ex), state));
                }
            }, state);
        }
    }

    /// <summary>
    /// A SPARQL Query Processor where the query is processed by passing it to the <see cref="IQueryableStorage.Query(String)">Query()</see> method of an <see cref="IQueryableStorage">IQueryableStorage</see>
    /// </summary>
    public class GenericQueryProcessor 
        : ISparqlQueryProcessor
    {
        private IQueryableStorage _manager;
        private SparqlFormatter _formatter = new SparqlFormatter();

        /// <summary>
        /// Creates a new Generic Query Processor
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        public GenericQueryProcessor(IQueryableStorage manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public object ProcessQuery(SparqlQuery query)
        {
            query.QueryExecutionTime = null;
            DateTime start = DateTime.Now;
            try
            {
                Object temp = _manager.Query(_formatter.Format(query));
                return temp;
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = elapsed;
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
                _manager.Query(rdfHandler, resultsHandler, _formatter.Format(query));
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = elapsed;
            }
        }

        /// <summary>
        /// Delegate used for asychronous execution
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        private delegate void ProcessQueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query);

        /// <summary>
        /// Processes a SPARQL Query asynchronously invoking the relevant callback when the query completes
        /// </summary>
        /// <param name="query">SPARQL QUery</param>
        /// <param name="rdfCallback">Callback for queries that return a Graph</param>
        /// <param name="resultsCallback">Callback for queries that return a Result Set</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// In the event of a success the appropriate callback will be invoked, if there is an error both callbacks will be invoked and passed an instance of <see cref="AsyncError"/> which contains details of the error and the original state information passed in.
        /// </remarks>
        public void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback, Object state)
        {
            Graph g = new Graph();
            SparqlResultSet rset = new SparqlResultSet();
            ProcessQueryAsync d = new ProcessQueryAsync(ProcessQuery);
            d.BeginInvoke(new GraphHandler(g), new ResultSetHandler(rset), query, r =>
            {
                try
                {
                    d.EndInvoke(r);
                    if (rset.ResultsType != SparqlResultsType.Unknown)
                    {
                        resultsCallback(rset, state);
                    }
                    else
                    {
                        rdfCallback(g, state);
                    }
                }
                catch (RdfQueryException queryEx)
                {
                    if (rdfCallback != null) rdfCallback(null, new AsyncError(queryEx, state));
                    if (resultsCallback != null) resultsCallback(null, new AsyncError(queryEx, state));
                }
                catch (Exception ex)
                {
                    RdfQueryException queryEx = new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex);
                    if (rdfCallback != null) rdfCallback(null, new AsyncError(queryEx, state));
                    if (resultsCallback != null) resultsCallback(null, new AsyncError(queryEx, state));
                }
            }, state);
        }

        /// <summary>
        /// Processes a SPARQL Query asynchronously passing the results to the relevant handler and invoking the callback when the query completes
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// In the event of a success the callback will be invoked, if there is an error the callback will be invoked and passed an instance of <see cref="AsyncError"/> which contains details of the error and the original state information passed in.
        /// </remarks>
        public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query, QueryCallback callback, Object state)
        {
            ProcessQueryAsync d = new ProcessQueryAsync(ProcessQuery);
            d.BeginInvoke(rdfHandler, resultsHandler, query, r =>
            {
                try
                {
                    d.EndInvoke(r);
                    callback(rdfHandler, resultsHandler, state);
                }
                catch (RdfQueryException queryEx)
                {
                    callback(rdfHandler, resultsHandler, new AsyncError(queryEx, state));
                }
                catch (Exception ex)
                {
                    callback(rdfHandler, resultsHandler, new AsyncError(new RdfQueryException("Unexpected error making an asynchronous query", ex), state));
                }
            }, state);
        }
    }

    /// <summary>
    /// A SPARQL Query Processor where the query is processed by passing it to a remote SPARQL endpoint
    /// </summary>
    public class RemoteQueryProcessor 
        : ISparqlQueryProcessor
    {
        private SparqlRemoteEndpoint _endpoint;
        private SparqlFormatter _formatter = new SparqlFormatter();

        /// <summary>
        /// Creates a new Remote Query Processor
        /// </summary>
        /// <param name="endpoint">SPARQL Endpoint</param>
        public RemoteQueryProcessor(SparqlRemoteEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        /// <summary>
        /// Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public object ProcessQuery(SparqlQuery query)
        {
            query.QueryExecutionTime = null;
            DateTime start = DateTime.Now;
            Object temp;
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
                        temp = _endpoint.QueryWithResultSet(_formatter.Format(query));
                        break;
                    case SparqlQueryType.Construct:
                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        temp = _endpoint.QueryWithResultGraph(_formatter.Format(query));
                        break;
                    default:
                        throw new RdfQueryException("Unable to execute an unknown query type against a Remote Endpoint");
                }
                return temp;
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = elapsed;
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
                switch (query.QueryType)
                {
                    case SparqlQueryType.Ask:
                    case SparqlQueryType.Select:
                    case SparqlQueryType.SelectAll:
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectAllReduced:
                    case SparqlQueryType.SelectDistinct:
                    case SparqlQueryType.SelectReduced:
                        _endpoint.QueryWithResultSet(resultsHandler, _formatter.Format(query));
                        break;
                    case SparqlQueryType.Construct:
                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        _endpoint.QueryWithResultGraph(rdfHandler, _formatter.Format(query));
                        break;
                    default:
                        throw new RdfQueryException("Unable to execute an unknown query type against a Remote Endpoint");
                }
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = elapsed;
            }
        }

        /// <summary>
        /// Processes a SPARQL Query asynchronously invoking the relevant callback when the query completes
        /// </summary>
        /// <param name="query">SPARQL QUery</param>
        /// <param name="rdfCallback">Callback for queries that return a Graph</param>
        /// <param name="resultsCallback">Callback for queries that return a Result Set</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// In the event of a success the appropriate callback will be invoked, if there is an error both callbacks will be invoked and passed an instance of <see cref="AsyncError"/> which contains details of the error and the original state information passed in.
        /// </remarks>
        public void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback, Object state)
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
                        _endpoint.QueryWithResultSet(_formatter.Format(query), resultsCallback, state);
                        break;
                    case SparqlQueryType.Construct:
                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        _endpoint.QueryWithResultGraph(_formatter.Format(query), rdfCallback, state);
                        break;
                    default:
                        throw new RdfQueryException("Unable to execute an unknown query type against a Remote Endpoint");
                }
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = elapsed;
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
        /// <remarks>
        /// In the event of a success the callback will be invoked, if there is an error the callback will be invoked and passed an instance of <see cref="AsyncError"/> which contains details of the error and the original state information passed in.
        /// </remarks>
        public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query, QueryCallback callback, Object state)
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
                        _endpoint.QueryWithResultSet(resultsHandler, _formatter.Format(query), callback, state);
                        break;
                    case SparqlQueryType.Construct:
                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        _endpoint.QueryWithResultGraph(rdfHandler, _formatter.Format(query), callback, state);
                        break;
                    default:
                        throw new RdfQueryException("Unable to execute an unknown query type against a Remote Endpoint");
                }
            }
            finally
            {
                TimeSpan elapsed = (DateTime.Now - start);
                query.QueryExecutionTime = elapsed;
            }
        }
    }
}
