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
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;

/// <summary>
/// A SPARQL Query Processor where the query is processed by passing it to the <see cref="IQueryableStorage.Query(string)">Query()</see> method of an <see cref="IQueryableStorage">IQueryableStorage</see>.
/// </summary>
public class GenericQueryProcessor 
    : QueryProcessorBase, ISparqlQueryProcessor
{
    private readonly IQueryableStorage _manager;
    private readonly SparqlFormatter _formatter = new SparqlFormatter();

    /// <summary>
    /// Creates a new Generic Query Processor.
    /// </summary>
    /// <param name="manager">Generic IO Manager.</param>
    public GenericQueryProcessor(IQueryableStorage manager)
    {
        _manager = manager;
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
            return _manager.Query(_formatter.Format(query));
        }
        finally
        {
            TimeSpan elapsed = (DateTime.Now - start);
            query.QueryExecutionTime = elapsed;
        }
    }

    /// <summary>
    /// Processes a SPARQL Query passing the results to the RDF or Results handler as appropriate.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="query">SPARQL Query.</param>
    public override void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
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

    /// <inheritdoc />
    public Task<object> ProcessQueryAsync(SparqlQuery query)
    {
        return Task.Factory.StartNew(() => ProcessQuery(query));
    }

    /// <inheritdoc />
    public Task ProcessQueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
    {
        return Task.Factory.StartNew(() => ProcessQuery(rdfHandler, resultsHandler, query));
    }
}