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
using System.Threading.Tasks;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Query;

/// <summary>
/// Provides a default implementation of the methods that allow a query processor to run asynchronously with a results callback.
/// </summary>
public abstract class QueryProcessorBase
{
    /// <summary>
    /// When overridden in a derived class, processes a SPARQL Query passing the results to the RDF or Results handler as appropriate.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="query">SPARQL Query.</param>
    public abstract void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler,
        SparqlQuery query);

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
    public void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback, object state)
    {
        var resultGraph = new Graph();
        var resultSet = new SparqlResultSet();
        Task.Factory.StartNew(() =>
            ProcessQuery(new GraphHandler(resultGraph), new ResultSetHandler(resultSet), query)).ContinueWith(
            antecedent =>
            {
                if (antecedent.Exception != null)
                {
                    Exception innerException = antecedent.Exception.InnerExceptions[0];
                    RdfQueryException queryException = innerException as RdfQueryException ??
                                                       new RdfQueryException(
                                                           "Unexpected error while making an asynchronous query, see inner exception for details",
                                                           innerException);
                    rdfCallback?.Invoke(null, new AsyncError(queryException, state));
                    resultsCallback?.Invoke(null, new AsyncError(queryException, state));
                }
                else if (resultSet.ResultsType != SparqlResultsType.Unknown)
                {
                    resultsCallback?.Invoke(resultSet, state);
                }
                else
                {
                    rdfCallback?.Invoke(resultGraph, state);
                }
            });
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
    public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query, QueryCallback callback, object state)
    {
        Task.Factory.StartNew(() => ProcessQuery(rdfHandler, resultsHandler, query))
            .ContinueWith(antecedent =>
            {
                if (antecedent.Exception != null)
                {
                    Exception innerException = antecedent.Exception.InnerExceptions[0];
                    RdfQueryException queryException = innerException as RdfQueryException ??
                                                       new RdfQueryException(
                                                           "Unexpected error while making an asynchronous query, see inner exception for details",
                                                           innerException);
                    callback?.Invoke(rdfHandler, resultsHandler, new AsyncError(queryException, state));
                }
                else
                {
                    callback?.Invoke(rdfHandler, resultsHandler, state);
                }
            });
    }
}