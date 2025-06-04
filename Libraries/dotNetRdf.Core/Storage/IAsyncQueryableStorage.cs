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
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Storage;

/// <summary>
/// Interface for storage providers which allow SPARQL Queries to be made against them asynchronously.
/// </summary>
public interface IAsyncQueryableStorage
    : IAsyncStorageProvider
{
    /// <summary>
    /// Queries the store asynchronously.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query.</exception>
    /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query.</exception>
    /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results.</exception>
    /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand.</exception>
    [Obsolete("This method is obsolete and will be removed in a future version. Replaced by QueryAsync(string, CancellationToken)")]
    void Query(string sparqlQuery, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Queries the store asynchronously.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query.</exception>
    /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query.</exception>
    /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results.</exception>
    /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand.</exception>
    [Obsolete("Replaced by QueryAsync(IRdfHandler, ISparqlResultsHandler, string")]
    void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Queries the store asynchronously.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="cancellationToken"></param>
    Task<object> QueryAsync(string sparqlQuery, CancellationToken cancellationToken);

    /// <summary>
    /// Queries the store asynchronously.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler that will receive graph results from CONSTRUCT or DESCRIBE queries.</param>
    /// <param name="resultsHandler">SPARQL Results set handler that will receive results from ASK or SELECT queries.</param>
    /// <param name="sparqlQuery">The SPARQL query to execute.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task QueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, CancellationToken cancellationToken);
}