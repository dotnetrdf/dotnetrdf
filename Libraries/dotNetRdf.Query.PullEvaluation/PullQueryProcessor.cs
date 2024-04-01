using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation;

/**
 * A SPARQL query processor that uses asynchronous pull-based processing.
 */
public class PullQueryProcessor : ISparqlQueryProcessor
{
    private readonly ITripleStore _tripleStore;

    public long Timeout { get; set; } = 30000;
    public ISparqlResultFactory SparqlResultFactory { get; set; } = new SparqlResultFactory();
    
    /**
     * Construct a new query processor instance,
     * @param dataset - the SPARQL dataset to query against 
     */
    public PullQueryProcessor(ITripleStore tripleStore)
    {
        _tripleStore = tripleStore;
    }
    
    /**
     * Evaluate a SPARQL algebra against the dataset configured for this query processor.
     */
    public IAsyncEnumerable<ISet> Evaluate(ISparqlAlgebra algebra, PullEvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        var builder = new EvaluationBuilder();
        context ??= new PullEvaluationContext(_tripleStore, unionDefaultGraph: false);
        IAsyncEvaluation evaluation = builder.Build(algebra, context);
        return evaluation.Evaluate(context, null, null, cancellationToken);
    }

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
                var g = new VDS.RDF.Graph();
                ProcessQuery(new GraphHandler(g), null, query);
                return g;
            default:
                throw new RdfQueryException("Cannot process unknown query types");
        }
    }

    public void ProcessQuery(IRdfHandler? rdfHandler, ISparqlResultsHandler? resultsHandler, SparqlQuery query)
    {
        throw new NotImplementedException();
    }

    public void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback, object state)
    {
        throw new NotImplementedException();
    }

    public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query,
        QueryCallback callback, object state)
    {
        throw new NotImplementedException();
    }

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
                var g = new VDS.RDF.Graph();
                await ProcessQueryAsync(new GraphHandler(g), null, query);
                return g;
            default:
                throw new RdfQueryException("Cannot process unknown query types");
        }
    }

    public async Task ProcessQueryAsync(IRdfHandler? rdfHandler, ISparqlResultsHandler? resultsHandler, SparqlQuery query)
    {
        
        // Do Handler null checks before evaluating the query
        if (query == null) throw new ArgumentNullException(nameof(query), "Cannot evaluate a null query");
        if (rdfHandler == null && query.QueryType is SparqlQueryType.Construct or SparqlQueryType.Describe or SparqlQueryType.DescribeAll) throw new ArgumentNullException(nameof(rdfHandler), "Cannot use a null RDF Handler when the Query is a CONSTRUCT/DESCRIBE");
        if (resultsHandler == null && (query.QueryType == SparqlQueryType.Ask || SparqlSpecsHelper.IsSelectQuery(query.QueryType))) throw new ArgumentNullException(nameof(resultsHandler), "Cannot use a null resultsHandler when the Query is an ASK/SELECT");

        var cts = new CancellationTokenSource();
        try
        {
            ISparqlAlgebra algebra = query.ToAlgebra();
            if (this.Timeout > 0)
            {
                cts.CancelAfter(TimeSpan.FromMilliseconds(this.Timeout));
            }

            var evaluationContext =
                new PullEvaluationContext(_tripleStore, false, query.DefaultGraphNames, query.NamedGraphNames);
            IAsyncEnumerable<ISet> solutionBindings = Evaluate(algebra, evaluationContext, cts.Token);
            switch (query.QueryType)
            {
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    if (resultsHandler == null)
                    {
                        // Should already be handled above, but this keeps the compiler happy that we have checked the nullable argument.
                        throw new ArgumentNullException(nameof(resultsHandler), "Cannot use a null resultsHandler when the Query is an ASK/SELECT");
                    }
                    resultsHandler.StartResults();
                    foreach (var v in algebra.Variables)
                    {
                        resultsHandler.HandleVariable(v);
                    }
                    var ok = true;
                    await foreach (ISet? solutionBinding in solutionBindings)
                    {
                        ok = resultsHandler.HandleResult(SparqlResultFactory.MakeResult(solutionBinding, algebra.Variables));
                        if (!ok) break;
                    }
                    resultsHandler.EndResults(ok);
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
                                continue;
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
            throw new RdfQueryException("Unexpected error running SPARQL query.", e);
        }
    }
}