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

using System.Threading;
using System.Threading.Tasks;

namespace VDS.RDF.Query;

/// <summary>
/// Interface to be implemented by classes that provide a client for accessing a remote SPARQL query service.
/// </summary>
public interface ISparqlQueryClient
{
    /// <summary>
    /// Execute a SPARQL query that is intended to return a SPARQL results set.
    /// </summary>
    /// <param name="sparqlQuery">The query to be executed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>The query results.</returns>
    /// <remarks>This method should be  used when processing SPARQL SELECT or ASK queries.</remarks>
    Task<SparqlResultSet> QueryWithResultSetAsync(string sparqlQuery, CancellationToken cancellationToken);

    /// <summary>
    /// Execute a SPARQL query that is intended to return a SPARQL results set.
    /// </summary>
    /// <param name="sparqlQuery">The query to be executed.</param>
    /// <param name="resultsHandler">The handler to use when parsing the results returned by the server.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>The query results.</returns>
    /// <remarks>This method should be  used when processing SPARQL SELECT or ASK queries.</remarks>
    Task QueryWithResultSetAsync(
        string sparqlQuery, ISparqlResultsHandler resultsHandler, CancellationToken cancellationToken);

    /// <summary>
    /// Execute a SPARQL query that is intended to return an RDF Graph.
    /// </summary>
    /// <param name="sparqlQuery">The query to be executed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>An RDF Graph.</returns>
    /// <remarks>This method should be used when processing SPARQL CONSTRUCT or DESCRIBE queries.</remarks>
    Task<IGraph> QueryWithResultGraphAsync(string sparqlQuery, CancellationToken cancellationToken);

    /// <summary>
    /// Execute a SPARQL query that is intended to return an RDF Graph.
    /// </summary>
    /// <param name="sparqlQuery">The query to be executed.</param>
    /// <param name="handler">The handler to use when parsing the graph data returned by the server.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>An RDF Graph.</returns>
    /// <remarks>This method should be used when processing SPARQL CONSTRUCT or DESCRIBE queries.</remarks>
    Task QueryWithResultGraphAsync(string sparqlQuery, IRdfHandler handler,
        CancellationToken cancellationToken);
}