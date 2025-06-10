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

using VDS.RDF.Query;

namespace VDS.RDF;

/// <summary>
/// Interface for Triple Stores which can be queried natively i.e. the Stores provide their own SPARQL implementations.
/// </summary>
/// <remarks>
/// A Natively Queryable store will typically not load its Graphs and Triples into memory as this is generally unecessary.
/// </remarks>
public interface INativelyQueryableStore 
    : ITripleStore
{
    /// <summary>
    /// Executes a SPARQL Query on the Triple Store.
    /// </summary>
    /// <param name="query">Sparql Query as unparsed String.</param>
    /// <returns></returns>
    /// <remarks>
    /// This assumes that the Store has access to some native SPARQL query processor on/at the Store which will be used to return the results.  Implementations should parse the returned result into a <see cref="SparqlResultSet">SparqlResultSet</see> or <see cref="Graph">Graph</see>.
    /// </remarks>
    object ExecuteQuery(string query);

    /// <summary>
    /// Executes a SPARQL Query on the Triple Store processing the results using an appropriate handler from those provided.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="query">SPARQL Query as unparsed String.</param>
    void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string query);
}