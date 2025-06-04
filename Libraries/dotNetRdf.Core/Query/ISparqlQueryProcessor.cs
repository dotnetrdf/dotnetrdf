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

namespace VDS.RDF.Query;

/// <summary>
/// Interface for SPARQL Query Processors.
/// </summary>
/// <remarks>
/// <para>
/// A SPARQL Query Processor is a class that knows how to evaluate SPARQL queries against some data source to which the processor has access.
/// </para>
/// <para>
/// The point of this interface is to allow for end users to implement custom query processors or to extend and modify the behaviour of the default Leviathan engine as required.
/// </para>
/// </remarks>
public interface ISparqlQueryProcessor
{
    /// <summary>
    /// Processes a SPARQL Query returning a <see cref="IGraph">IGraph</see> instance or a <see cref="SparqlResultSet">SparqlResultSet</see> depending on the type of the query.
    /// </summary>
    /// <param name="query">SPARQL Query.</param>
    /// <returns>
    /// Either an <see cref="IGraph">IGraph</see> instance of a <see cref="SparqlResultSet">SparqlResultSet</see> depending on the type of the query.
    /// </returns>
    object ProcessQuery(SparqlQuery query);

    /// <summary>
    /// Processes a SPARQL Query passing the results to the RDF or Results handler as appropriate.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="query">SPARQL Query.</param>
    void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query);

    /// <summary>
    /// Processes a SPARQL Query asynchronously invoking the relevant callback when the query completes.
    /// </summary>
    /// <param name="query">SPARQL QUery.</param>
    /// <param name="rdfCallback">Callback for queries that return a Graph.</param>
    /// <param name="resultsCallback">Callback for queries that return a Result Set.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("This method is obsolete and will be removed in a future version. Use the ProcessQueryAsync method instead.")]
    void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback, object state);

    /// <summary>
    /// Processes a SPARQL Query asynchronously passing the results to the relevant handler and invoking the callback when the query completes.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="query">SPARQL Query.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("This method is obsolete and will be removed in a future version. Use the ProcessQueryAsync method instead.")]
    void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query, QueryCallback callback, object state);

    /// <summary>
    /// Process a SPARQL query asynchronously returning either a <see cref="SparqlResultSet"/> or a <see cref="IGraph"/> depending on the type of the query.
    /// </summary>
    /// <param name="query">SPARQL query.</param>
    /// <returns>Either an &lt;see cref="IGraph"&gt;IGraph&lt;/see&gt; instance of a &lt;see cref="SparqlResultSet"&gt;SparqlResultSet&lt;/see&gt; depending on the type of the query.</returns>
    Task<object> ProcessQueryAsync(SparqlQuery query);

    /// <summary>
    /// Process a SPARQL query asynchronously, passing the results to teh relevant handler.
    /// </summary>
    /// <param name="rdfHandler">RDF handler invoked for queries that return RDF graphs.</param>
    /// <param name="resultsHandler">Results handler invoked for queries that return SPARQL results sets.</param>
    /// <param name="query">SPARQL query.</param>
    Task ProcessQueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query);
}
