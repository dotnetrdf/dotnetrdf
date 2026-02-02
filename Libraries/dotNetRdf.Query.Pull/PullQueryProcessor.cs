/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Describe;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Pull;

/**
 * A SPARQL query processor that attempts to perform a more streaming style of SPARQL query processing
 * utilising a pull processing model where the stages of processing are represented as asynchronous
 * enumerations.
 *
 * WARNING: This processor should be considered EXPERIMENTAL and not suitable for production use.
 */
public class PullQueryProcessor : ISparqlQueryProcessor
{
    private readonly ITripleStore _tripleStore;
    private readonly PullQueryOptions _options = new();

    /// <summary>
    /// Gets the query execution timeout (in ms) configured for this processor.
    /// </summary>
    public ulong QueryExecutionTimeout { get { return _options.QueryExecutionTimeout; } }
    
    /// <summary>
    /// Gets the union default graph setting configured for this processor.
    /// </summary>
    public bool UnionDefaultGraph {get {return _options.UnionDefaultGraph;}}
    
    /// <summary>
    /// Construct a new query processor instance.
    /// </summary>
    /// <param name="tripleStore">The store to query against</param>
    /// <param name="options">Receives a <see cref="PullQueryOptions"/> instance which may be modified to set the evaluation options for this processor.</param>
    public PullQueryProcessor(ITripleStore tripleStore, Action<PullQueryOptions>? options = null)
    {
        _tripleStore = tripleStore;
        options?.Invoke(_options);
    }

    /// <summary>
    /// Evaluate a SPARQL algebra against the dataset configured for this query processor.
    /// </summary>
    /// <param name="algebra"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <remarks>Used primarily for internal test purposes.</remarks>
    internal IAsyncEnumerable<ISet> Evaluate(ISparqlAlgebra algebra, PullEvaluationContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var builder = new EvaluationBuilder();
        context ??= new PullEvaluationContext(
            _tripleStore,
            _options.UnionDefaultGraph,
            defaultGraphNames: _options.DefaultGraphNames,
            uriFactory: _options.UriFactory,
            nodeComparer: _options.NodeComparer
        );
        IAsyncEvaluation evaluation = builder.Build(algebra, context);
        return evaluation.Evaluate(context, null, null, cancellationToken);
    }

    /// <summary>
    /// Process a SPARQL query against the triple store configured for this query processor.
    /// </summary>
    /// <param name="query">The parsed SPARQL query to be processed.</param>
    /// <returns>Either a <see cref="SparqlResultSet"/> (for ASK and SELECT queries) or a <see cref="VDS.RDF.Graph"/> (for CONSTRUCT or DESCRIBE queries).</returns>
    /// <exception cref="RdfQueryException">If there are errors encountered while processing the query.</exception>
    public object ProcessQuery(SparqlQuery query)
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
                var results = new SparqlResultSet();
                ProcessQuery(null, new ResultSetHandler(results), query);
                return results;
            case SparqlQueryType.Construct:
            case SparqlQueryType.Describe:
            case SparqlQueryType.DescribeAll:
                var g = new Graph();
                ProcessQuery(new GraphHandler(g), null, query);
                return g;
            default:
                throw new RdfQueryException("Cannot process unknown query types");
        }
    }

    /// <summary>
    /// Synchronously process a SPARQL query, passing results to the provided <see cref="IRdfHandler"/>
    /// for CONSTRUCT/DESCRIBE queries or the provided <see cref="ISparqlResultsHandler"/> for ASK/SELECT
    /// queries.
    /// </summary>
    /// <param name="rdfHandler">the handler to receive results from CONSTRUCT/DESCRIBE queries.</param>
    /// <param name="resultsHandler">the handler to receive results from CONSTRUCT/DESCRIBE queries.</param>
    /// <param name="query">the query to be processed.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="rdfHandler"/> is null and the query is a CONSTRUCT/DESCRIBE or if <paramref name="resultsHandler"/> is null and the query is an ASK/DESCRIBE.</exception>
    /// <exception cref="RdfQueryException">If there were errors encountered while processing the query.</exception>
    public void ProcessQuery(IRdfHandler? rdfHandler, ISparqlResultsHandler? resultsHandler, SparqlQuery query)
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
                if (resultsHandler is not null)
                {
                    ProcessQueryAsync(null, resultsHandler, query).GetAwaiter().GetResult();
                }
                else
                {
                    throw new ArgumentNullException(nameof(resultsHandler), "No results handler provided for an ASK/SELECT query.");
                }
                break;
            case SparqlQueryType.Construct:
            case SparqlQueryType.Describe:
            case SparqlQueryType.DescribeAll:
                if (rdfHandler is not null)
                {
                    ProcessQueryAsync(rdfHandler, null, query).GetAwaiter().GetResult();
                }
                else
                {
                    throw new ArgumentNullException(nameof(rdfHandler), "No RDF handler provided for a CONSTRUCT/DESCRIBE query.");
                }
                break;
            default:
                throw new RdfQueryException("Cannot process unknown query types");
        }
    }

    /// <summary>
    /// Synchronously process the provided SPARQL query, passing the results to either <paramref name="rdfCallback"/> or <paramref name="resultsCallback"/>
    /// depending on the query type.
    /// </summary>
    /// <param name="query">The parsed SPARQL query to be processed.</param>
    /// <param name="rdfCallback">The callback to be invoked with a graph result from CONSTRUCT/DESCRIBE queries.</param>
    /// <param name="resultsCallback">The callback to be invoked with a SPARQL results set from SELECT/ASK queries.</param>
    /// <param name="state">Additional state to pass to the callback method.</param>
    /// <exception cref="RdfQueryException">Raised if an error was encountered while processing the query.</exception>
    public void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback,
        object state)
    {
        var result = ProcessQueryAsync(query).GetAwaiter().GetResult();
        switch (result)
        {
            case null:
                throw new RdfQueryException("Query processing returned no results.");
            case IGraph g:
                rdfCallback(g, state);
                break;
            case SparqlResultSet s:
                resultsCallback(s, result);
                break;
        }
    }

    /// <summary>
    /// Synchronously process the provided query using the provided results handlers and invoking the
    /// provided callback method on completion.
    /// </summary>
    /// <param name="rdfHandler">The handler to invoke for CONSTRUCT/DESCRIBE query results.</param>
    /// <param name="resultsHandler">The handler to invoke for SELECT/ASK query results.</param>
    /// <param name="query">The parsed SPARQL query to be processed.</param>
    /// <param name="callback">The callback method to be invoked on completion of query processing.</param>
    /// <param name="state">Additional state to pass to <paramref name="callback"/>.</param>
    public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query,
        QueryCallback callback, object state)
    {
        ProcessQuery(rdfHandler, resultsHandler, query);
        callback(rdfHandler, resultsHandler, state);
    }

    /// <summary>
    /// Asynchronously process the provided SPARQL query. 
    /// </summary>
    /// <param name="query">The parsed query to be processed.</param>
    /// <returns>Either a <see cref="SparqlResultSet"/> (for ASK and SELECT queries) or a <see cref="VDS.RDF.Graph"/> (for CONSTRUCT or DESCRIBE queries)</returns>
    /// <exception cref="RdfQueryException">If the provided SPARQL query is not a supported type of query or if the query processing raised errors.</exception>
    public async Task<object> ProcessQueryAsync(SparqlQuery query)
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
                var results = new SparqlResultSet();
                await ProcessQueryAsync(null, new ResultSetHandler(results), query);
                return results;
            case SparqlQueryType.Construct:
            case SparqlQueryType.Describe:
            case SparqlQueryType.DescribeAll:
                var g = new Graph();
                await ProcessQueryAsync(new GraphHandler(g), null, query);
                return g;
            default:
                throw new RdfQueryException("Cannot process unknown query types");
        }
    }

    /// <summary>
    /// Process the provided SPARQL query asynchronously
    /// </summary>
    /// <param name="rdfHandler">The handler to invoke with results from CONSTRUCT/DESCRIBE queries</param>
    /// <param name="resultsHandler">The handler to invoke with results from ASK/SELECT queries.</param>
    /// <param name="query">The query to be processed.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="rdfHandler"/> is null and the query is a CONSTRUCT or DESCRIBE, or if <paramref name="resultsHandler"/> is null and the query is a SELECT or ASK.</exception>
    /// <exception cref="RdfQueryException">If errors are encountered during query processing.</exception>
    public async Task ProcessQueryAsync(IRdfHandler? rdfHandler, ISparqlResultsHandler? resultsHandler, SparqlQuery query)
    {
        
        // Do Handler null checks before evaluating the query
        if (query == null) throw new ArgumentNullException(nameof(query), "Cannot evaluate a null query");
        if (rdfHandler == null && query.QueryType is SparqlQueryType.Construct or SparqlQueryType.Describe or SparqlQueryType.DescribeAll) throw new ArgumentNullException(nameof(rdfHandler), "Cannot use a null RDF Handler when the Query is a CONSTRUCT/DESCRIBE");
        if (resultsHandler == null && (query.QueryType == SparqlQueryType.Ask || SparqlSpecsHelper.IsSelectQuery(query.QueryType))) throw new ArgumentNullException(nameof(resultsHandler), "Cannot use a null resultsHandler when the Query is an ASK/SELECT");

        var cts = new CancellationTokenSource();
        try
        {
            var autoVarPrefix = "_auto";
            while (query.Variables.Any(v => v.Name.StartsWith(autoVarPrefix)))
            {
                autoVarPrefix = "_" + autoVarPrefix;
            }
            var pushDownAggregatesOptimiser = new PushDownAggregatesOptimiser(autoVarPrefix);
            ISparqlAlgebra algebra = query.ToAlgebra(
                true, [pushDownAggregatesOptimiser]
                );
            
            if (this._options.QueryExecutionTimeout > 0)
            {
                cts.CancelAfter(TimeSpan.FromMilliseconds(this._options.QueryExecutionTimeout));
            }

            var evaluationContext =
                new PullEvaluationContext(
                    _tripleStore,
                    unionDefaultGraph: _options.UnionDefaultGraph,
                    defaultGraphNames: query.DefaultGraphNames.Any() ? query.DefaultGraphNames : _options.DefaultGraphNames, 
                    namedGraphs: query.NamedGraphNames,
                    autoVarPrefix: autoVarPrefix,
                    baseUri: query.BaseUri,
                    uriFactory: _options.UriFactory,
                    nodeComparer: _options.NodeComparer);
            IAsyncEnumerable<ISet> solutionBindings = Evaluate(algebra, evaluationContext, cts.Token);
            switch (query.QueryType)
            {
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                    await ProcessSelectQueryAsync(resultsHandler, algebra.Variables.ToList(), solutionBindings);
                    break;
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    await ProcessSelectQueryAsync(resultsHandler, query.Variables.Select(v => v.Name).ToList(),
                        solutionBindings);
                    break;
                case SparqlQueryType.Ask:
                    if (resultsHandler == null)
                    {
                        // Should already be handled above, but this keeps the compiler happy that we have checked the nullable argument.
                        throw new ArgumentNullException(nameof(resultsHandler), "Cannot use a null resultsHandler when the Query is an ASK/SELECT");
                    }
                    resultsHandler.StartResults();
                    await using (IAsyncEnumerator<ISet>? enumerator = solutionBindings.GetAsyncEnumerator(cts.Token))
                    {
                        var hasNext = await enumerator.MoveNextAsync();
                        resultsHandler.HandleBooleanResult(hasNext);
                    }
                    resultsHandler.EndResults(true);
                    break;
                case SparqlQueryType.Construct:
                    if (rdfHandler == null)
                    {
                        throw new ArgumentNullException(nameof(rdfHandler),
                            "Cannot use a null rdfHandler when the query is a CONSTRUCT/DESCRIBE");
                    }
                    rdfHandler.StartRdf();
                    try
                    {
                        foreach (var prefix in query.NamespaceMap.Prefixes)
                        {
                            if (!rdfHandler.HandleNamespace(prefix, query.NamespaceMap.GetNamespaceUri(prefix)))
                            {
                                ParserHelper.Stop();
                            }
                        }

                        var constructContext = new ConstructContext(rdfHandler, false);
                        await foreach (ISet? s in solutionBindings)
                        {
                            try
                            {
                                constructContext.Set = s;
                                foreach (IConstructTriplePattern p in query.ConstructTemplate.TriplePatterns
                                             .OfType<IConstructTriplePattern>())
                                {
                                    try
                                    {
                                        if (!rdfHandler.HandleTriple(p.Construct(constructContext)))
                                        {
                                            ParserHelper.Stop();
                                        }
                                    }
                                    catch (RdfQueryException)
                                    {
                                        // If we get an error here then we could not construct a specific triple
                                        // so we continue anyway
                                    }
                                }
                            }
                            catch (RdfQueryException)
                            {
                                // If we get an error here this means we couldn't construct for this solution so the
                                // entire solution is discarded
                            }
                        }

                        rdfHandler.EndRdf(true);
                    }
                    catch (RdfParsingTerminatedException)
                    {
                        rdfHandler.EndRdf(true);
                    }
                    catch
                    {
                        rdfHandler.EndRdf(false);
                        throw;
                    }
                    break;

                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    ISparqlDescribe describer = _options.Describer;
                    describer.Describe(rdfHandler, new DescriberContext(query, evaluationContext, solutionBindings));
                    break;
                default:
                    throw new NotImplementedException(
                        $"Support for query type {query.QueryType} has not yet been implemented.");
            }
        }
        catch (RdfQueryException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new RdfQueryException("Unexpected error running SPARQL query: " + e.Message, e);
        }
    }

    private async Task ProcessSelectQueryAsync(ISparqlResultsHandler? resultsHandler, List<string> projectionVars, IAsyncEnumerable<ISet> solutionBindings)
    {
        if (resultsHandler == null)
        {
            // Should already be handled above, but this keeps the compiler happy that we have checked the nullable argument.
            throw new ArgumentNullException(nameof(resultsHandler),
                "Cannot use a null resultsHandler when the Query is an ASK/SELECT");
        }

        resultsHandler.StartResults();
        foreach (var v in projectionVars)
        {
            resultsHandler.HandleVariable(v);
        }

        var ok = true;
        await foreach (ISet? solutionBinding in solutionBindings)
        {
            ok = resultsHandler.HandleResult(this._options.SparqlResultFactory.MakeResult(solutionBinding, projectionVars));
            if (!ok) break;
        }

        resultsHandler.EndResults(ok);
    }
}