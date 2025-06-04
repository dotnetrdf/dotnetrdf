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

using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF;

/// <summary>
/// Extension methods for working with a <see cref="IGraph"/> instance as an in-memory queryable SPARQL dataset.
/// </summary>
public static class InMemoryExtensions
{
    /// <summary>
    /// Executes a SPARQL Query on a Graph.
    /// </summary>
    /// <param name="g">Graph to query.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <returns></returns>
    public static object ExecuteQuery(this IGraph g, string sparqlQuery)
    {
        // Due to change in default graph behaviour ensure that we associate this graph as the default graph of the dataset
        var ds = new InMemoryDataset(g);
        var processor = new LeviathanQueryProcessor(ds, options => options.UriFactory = new CachingUriFactory(g.UriFactory));
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(sparqlQuery);
        return processor.ProcessQuery(q);
    }

    /// <summary>
    /// Executes a SPARQL Query on a Graph handling the results using the handlers provided.
    /// </summary>
    /// <param name="g">Graph to query.</param>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">SPARQL Results Handler.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    public static void ExecuteQuery(this IGraph g, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
    {
        var ds = new InMemoryDataset(g);
        var processor = new LeviathanQueryProcessor(ds, options => options.UriFactory = new CachingUriFactory(g.UriFactory));
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(sparqlQuery);
        processor.ProcessQuery(rdfHandler, resultsHandler, q);
    }

    /// <summary>
    /// Executes a SPARQL Query on a Graph.
    /// </summary>
    /// <param name="g">Graph to query.</param>
    /// <param name="query">SPARQL Query.</param>
    /// <returns></returns>
    public static object ExecuteQuery(this IGraph g, SparqlQuery query)
    {
        var ds = new InMemoryDataset(g);
        var processor = new LeviathanQueryProcessor(ds, options => options.UriFactory = new CachingUriFactory(g.UriFactory));
        return processor.ProcessQuery(query);
    }

    /// <summary>
    /// Executes a SPARQL Query on a Graph handling the results using the handlers provided.
    /// </summary>
    /// <param name="g">Graph to query.</param>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">SPARQL Results Handler.</param>
    /// <param name="query">SPARQL Query.</param>
    public static void ExecuteQuery(this IGraph g, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
    {
        var ds = new InMemoryDataset(g);
        var processor = new LeviathanQueryProcessor(ds, options => options.UriFactory = new CachingUriFactory(g.UriFactory));
        processor.ProcessQuery(rdfHandler, resultsHandler, query);
    }

    /// <summary>
    /// Executes a SPARQL Query on a Graph.
    /// </summary>
    /// <param name="g">Graph to query.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <returns></returns>
    public static object ExecuteQuery(this IGraph g, SparqlParameterizedString sparqlQuery)
    {
        return g.ExecuteQuery(sparqlQuery.ToString());
    }

    /// <summary>
    /// Executes a SPARQL Query on a Graph handling the results using the handlers provided.
    /// </summary>
    /// <param name="g">Graph to query.</param>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">SPARQL Results Handler.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    public static void ExecuteQuery(this IGraph g, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlParameterizedString sparqlQuery)
    {
        g.ExecuteQuery(rdfHandler, resultsHandler, sparqlQuery.ToString());
    }

    /// <summary>
    /// Turns a Graph into a Triple Store.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <returns></returns>
    internal static IInMemoryQueryableStore AsTripleStore(this IGraph g)
    {
        var store = new TripleStore();
        store.Add(g);
        return store;
    }


}
