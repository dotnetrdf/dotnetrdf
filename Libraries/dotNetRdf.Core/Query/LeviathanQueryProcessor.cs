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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Describe;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query;

/// <summary>
/// Default SPARQL Query Processor provided by the library's Leviathan SPARQL Engine.
/// </summary>
/// <remarks>
/// <para>
/// The Leviathan Query Processor simply invokes the <see cref="ISparqlAlgebra">Evaluate</see> method of the SPARQL Algebra it is asked to process.
/// </para>
/// <para>
/// In future releases much of the Leviathan Query engine logic will be moved into this class to make it possible for implementors to override specific bits of the algebra processing but this is not possible at this time.
/// </para>
/// </remarks>
public class LeviathanQueryProcessor 
    : ISparqlQueryProcessor,
        ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext>
{
    private readonly ISparqlDataset _dataset;
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly LeviathanQueryOptions _options = new();
    private readonly ISparqlExpressionProcessor<IValuedNode, SparqlEvaluationContext, int> _expressionProcessor;
    private readonly ISparqlAggregateProcessor<IValuedNode, SparqlEvaluationContext, int> _aggregateProcessor;
    private readonly IDictionary<Bindings, BaseMultiset> _bindingsCache = new Dictionary<Bindings, BaseMultiset>();

    /// <summary>
    /// Creates a new Leviathan Query Processor.
    /// </summary>
    /// <param name="store">Triple Store.</param>
    /// <param name="options">Set the processor options.</param>
    public LeviathanQueryProcessor(IInMemoryQueryableStore store, Action<LeviathanQueryOptions> options = null)
        : this(new InMemoryDataset(store), options) { }

    /// <summary>
    /// Creates a new Leviathan Query Processor.
    /// </summary>
    /// <param name="data">SPARQL Dataset.</param>
    /// <param name="options">Set the processor options.</param>
    public LeviathanQueryProcessor(ISparqlDataset data, Action<LeviathanQueryOptions> options = null)
    {
        _dataset = data;

        if (!_dataset.UsesUnionDefaultGraph)
        {
            if (!_dataset.HasGraph((IRefNode)null))
            {
                // Create the Default unnamed Graph if it doesn't exist and then Flush() the change
                _dataset.AddGraph(new Graph());
                _dataset.Flush();
            }
        }

        options?.Invoke(_options);
        var expressionProcessor = new LeviathanExpressionProcessor(_options, this);
        _aggregateProcessor = expressionProcessor.AggregateProcessor;
        _expressionProcessor = expressionProcessor;
    }

    /// <summary>
    /// Create a new Leviathan query processor.
    /// </summary>
    /// <param name="data">SPARQL dataset.</param>
    /// <param name="options">Query processor options.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public LeviathanQueryProcessor(ISparqlDataset data, LeviathanQueryOptions options)
    {
        _options = options ??
                   throw new ArgumentNullException(nameof(options), "Query options parameter must not be null.");

        _dataset = data;

        if (!_dataset.UsesUnionDefaultGraph)
        {
            if (!_dataset.HasGraph((IRefNode)null))
            {
                // Create the Default unnamed Graph if it doesn't exist and then Flush() the change
                _dataset.AddGraph(new Graph());
                _dataset.Flush();
            }
        }
        var expressionProcessor = new LeviathanExpressionProcessor(_options, this);
        _aggregateProcessor = expressionProcessor.AggregateProcessor;
        _expressionProcessor = expressionProcessor;
    }

    /// <summary>
    /// Processes a SPARQL Query.
    /// </summary>
    /// <param name="query">SPARQL Query.</param>
    /// <returns></returns>
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
                IGraph g = new Graph();
                ProcessQuery(new GraphHandler(g), null, query);
                return g;
            default:
                throw new RdfQueryException("Cannot process unknown query types");
        }
    }

    /// <summary>
    /// Processes a SPARQL Query sending the results to a RDF/SPARQL Results handler as appropriate.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="query">SPARQL Query.</param>
    public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
    {
        // Do Handler null checks before evaluating the query
        if (query == null) throw new ArgumentNullException(nameof(query), "Cannot evaluate a null query");
        if (rdfHandler == null && query.QueryType is SparqlQueryType.Construct or SparqlQueryType.Describe or SparqlQueryType.DescribeAll) throw new ArgumentNullException(nameof(rdfHandler), "Cannot use a null RDF Handler when the Query is a CONSTRUCT/DESCRIBE");
        if (resultsHandler == null && (query.QueryType == SparqlQueryType.Ask || SparqlSpecsHelper.IsSelectQuery(query.QueryType))) throw new ArgumentNullException(nameof(resultsHandler), "Cannot use a null resultsHandler when the Query is an ASK/SELECT");

        // Handle the Thread Safety of the Query Evaluation
        ReaderWriterLockSlim currLock = (_dataset is IThreadSafeDataset threadSafeDataset) ? threadSafeDataset.Lock : _lock;
        try
        {
            currLock.EnterReadLock();
            // Reset Query Timers
            query.QueryExecutionTime = null;

            bool datasetOk = false, defGraphOk = false;

            try
            {
                // Set up the Default and Active Graphs
                if (query.DefaultGraphNames.Any())
                {
                    // Call HasGraph() on each Default Graph but ignore the results, we just do this
                    // in case a dataset has any kind of load on demand behaviour
                    foreach (IRefNode defaultGraphName in query.DefaultGraphNames)
                    {
                        _dataset.HasGraph(defaultGraphName);
                    }
                    _dataset.SetDefaultGraph(new List<IRefNode>(query.DefaultGraphNames));
                    defGraphOk = true;
                }
                else if (query.NamedGraphNames.Any())
                {
                    // No FROM Clauses but one/more FROM NAMED means the Default Graph is the empty graph
                    _dataset.SetDefaultGraph(new List<IRefNode>(0));
                }
                _dataset.SetActiveGraph(_dataset.DefaultGraphNames.ToList());
                datasetOk = true;

                // Convert to Algebra and execute the Query
                SparqlEvaluationContext context = GetContext(query);
                try
                {
                    context.StartExecution(_options.QueryExecutionTimeout);
                    ISparqlAlgebra algebra = query.ToAlgebra(_options.AlgebraOptimisation, _options.AlgebraOptimisers);
                    BaseMultiset _ = context.Evaluate(algebra);

                    context.EndExecution();
                    query.QueryExecutionTime = new TimeSpan(context.QueryTimeTicks);
                }
                catch (RdfQueryException)
                {
                    context.EndExecution();
                    query.QueryExecutionTime = new TimeSpan(context.QueryTimeTicks);
                    throw;
                }
                catch
                {
                    context.EndExecution();
                    query.QueryExecutionTime = new TimeSpan(context.QueryTimeTicks);
                    throw;
                }

                // Return the Results
                switch (query.QueryType)
                {
                    case SparqlQueryType.Ask:
                    case SparqlQueryType.Select:
                    case SparqlQueryType.SelectAll:
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectAllReduced:
                    case SparqlQueryType.SelectDistinct:
                    case SparqlQueryType.SelectReduced:
                        // For SELECT and ASK can populate a Result Set directly from the Evaluation Context
                        // return new SparqlResultSet(context);
                        resultsHandler.Apply(context);
                        break;

                    case SparqlQueryType.Construct:
                        // Create a new Empty Graph for the Results
                        try
                        {
                            rdfHandler.StartRdf();

                            foreach (var prefix in query.NamespaceMap.Prefixes)
                            {
                                if (!rdfHandler.HandleNamespace(prefix, query.NamespaceMap.GetNamespaceUri(prefix))) ParserHelper.Stop();
                            }

                            // Construct the Triples for each Solution
                            if (context.OutputMultiset is IdentityMultiset) context.OutputMultiset = new SingletonMultiset();
                            foreach (ISet s in context.OutputMultiset.Sets)
                            {
                                var constructContext = new ConstructContext(rdfHandler, false);
                                try
                                {
                                    constructContext.Set = s;
                                    foreach (IConstructTriplePattern p in query.ConstructTemplate.TriplePatterns.OfType<IConstructTriplePattern>())
                                    {
                                        try
                                        {
                                            if (!rdfHandler.HandleTriple(p.Construct(constructContext))) ParserHelper.Stop();
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

                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        // For DESCRIBE we retrieve the Describe algorithm and apply it
                        ISparqlDescribe describer = context.Options.Describer;
                        describer.Describe(rdfHandler, (ISparqlDescribeContext)context);
                        break;

                    default:
                        throw new RdfQueryException("Unknown query types cannot be processed by Leviathan");
                }
            }
            finally
            {
                if (defGraphOk) _dataset.ResetDefaultGraph();
                if (datasetOk) _dataset.ResetActiveGraph();
            }
        }
        finally
        {
            currLock.ExitReadLock();
        }
    }

    
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
        Task.Factory.StartNew(() => ProcessQuery(new GraphHandler(resultGraph), new ResultSetHandler(resultSet), query))
            .ContinueWith(antecedent =>
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
    /// Process a SPARQL query asynchronously returning either a <see cref="SparqlResultSet"/> or a <see cref="IGraph"/> depending on the type of the query.
    /// </summary>
    /// <param name="query">SPARQL query.</param>
    /// <returns>Either an &lt;see cref="IGraph"&gt;IGraph&lt;/see&gt; instance of a &lt;see cref="SparqlResultSet"&gt;SparqlResultSet&lt;/see&gt; depending on the type of the query.</returns>
    public Task<object> ProcessQueryAsync(SparqlQuery query)
    {
        return Task.Factory.StartNew(() => ProcessQuery(query));
    }

    /// <summary>
    /// Process a SPARQL query asynchronously, passing the results to teh relevant handler.
    /// </summary>
    /// <param name="rdfHandler">RDF handler invoked for queries that return RDF graphs.</param>
    /// <param name="resultsHandler">Results handler invoked for queries that return SPARQL results sets.</param>
    /// <param name="query">SPARQL query.</param>
    public Task ProcessQueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
    {
        return Task.Factory.StartNew(() => ProcessQuery(rdfHandler, resultsHandler, query));
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

    /// <summary>
    /// Creates a new Evaluation Context.
    /// </summary>
    /// <returns></returns>
    protected SparqlEvaluationContext GetContext()
    {
        return GetContext(null);
    }

    /// <summary>
    /// Creates a new Evaluation Context for the given Query.
    /// </summary>
    /// <param name="q">Query.</param>
    /// <returns></returns>
    private SparqlEvaluationContext GetContext(SparqlQuery q)
    {
        return new SparqlEvaluationContext(q, _dataset, GetProcessorForContext(), _options);
    }

    /// <summary>
    /// Gets the Query Processor for a Context.
    /// </summary>
    /// <returns></returns>
    private ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> GetProcessorForContext()
    {
        return GetType() == typeof(LeviathanQueryProcessor) ? null : this;
    }


    #region Algebra Processor Implementation

    /// <summary>
    /// Processes SPARQL Algebra.
    /// </summary>
    /// <param name="algebra">Algebra.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public BaseMultiset ProcessAlgebra(ISparqlAlgebra algebra, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        return algebra.Accept(this, context);
    }

    /// <summary>
    /// Processes an Ask.
    /// </summary>
    /// <param name="ask">Ask.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessAsk(Ask ask, SparqlEvaluationContext context)
    {
        context ??= GetContext();

        try
        {
            context.InputMultiset = ask.InnerAlgebra.Accept(this, context);
        }
        catch (RdfQueryTimeoutException)
        {
            if (!context.Query.PartialResultsOnTimeout) throw;
        }

        if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
        {
            context.OutputMultiset = context.InputMultiset;
        }
        else
        {
            if (context.InputMultiset.IsEmpty)
            {
                context.OutputMultiset = new NullMultiset();
            }
            else
            {
                context.OutputMultiset = new IdentityMultiset();
            }
        }

        return context.OutputMultiset;
    }

    /// <inheritdoc />
    public virtual BaseMultiset ProcessAskAnyTriples(AskAnyTriples askAny, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        context.OutputMultiset =
            context.Data.HasTriples ? (BaseMultiset)new IdentityMultiset() : new NullMultiset();
        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a BGP.
    /// </summary>
    /// <param name="bgp">BGP.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessBgp(IBgp bgp, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        switch (bgp)
        {
            case AskBgp askBgp:
                context.CheckTimeout();
                BaseMultiset results = EvaluateAskBgp(askBgp.TriplePatterns.ToList(), context, 0, out _);
                if (results is Multiset && results.IsEmpty) results = new NullMultiset();
                context.CheckTimeout();

                context.OutputMultiset = results;
                context.OutputMultiset.Trim();
                return context.OutputMultiset;
            case LazyBgp lazyBgp:
                return EvaluateLazyBgp(lazyBgp, context);
            default:
                return EvaluateDefaultBgp(bgp, context);
        }
    }

    private BaseMultiset EvaluateDefaultBgp(IBgp bgp, SparqlEvaluationContext context)
    {
        IList<ITriplePattern> triplePatterns = bgp.TriplePatterns.ToList();
        if (triplePatterns.Count > 0)
        {
            for (var i = 0; i < triplePatterns.Count; i++)
            {
                if (i == 0)
                {
                    // If the 1st thing in a BGP is a BIND/LET/FILTER the Input becomes the Identity Multiset
                    if (triplePatterns[i].PatternType == TriplePatternType.Filter ||
                        triplePatterns[i].PatternType == TriplePatternType.BindAssignment ||
                        triplePatterns[i].PatternType == TriplePatternType.LetAssignment)
                    {
                        if (triplePatterns[i].PatternType == TriplePatternType.BindAssignment)
                        {
                            if (context.InputMultiset.ContainsVariable(
                                ((IAssignmentPattern) triplePatterns[i]).VariableName))
                                throw new RdfQueryException(
                                    "Cannot use a BIND assignment to BIND to a variable that has previously been declared");
                        }
                        else
                        {
                            context.InputMultiset = new IdentityMultiset();
                        }
                    }
                }

                // Create a new Output Multiset
                context.OutputMultiset = new Multiset();

                triplePatterns[i].Accept(this, context);

                // If at any point we've got an Empty Multiset as our Output then we terminate BGP execution
                if (context.OutputMultiset.IsEmpty) break;

                // Check for Timeout before attempting the Join
                context.CheckTimeout();

                // If this isn't the first Pattern we do Join/Product the Output to the Input
                if (i > 0)
                {
                    if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                    {
                        // Disjoint so do a Product
                        context.OutputMultiset =
                            context.InputMultiset.ProductWithTimeout(context.OutputMultiset,
                                context.RemainingTimeout);
                    }
                    else
                    {
                        // Normal Join
                        context.OutputMultiset = context.InputMultiset.Join(context.OutputMultiset);
                    }
                }

                // Then the Input for the next Pattern is the Output from the previous Pattern
                context.InputMultiset = context.OutputMultiset;
            }

            if (context.TrimTemporaryVariables)
            {
                // Trim the Multiset - this eliminates any temporary variables
                context.OutputMultiset.Trim();
            }
        }
        else
        {
            // For an Empty BGP we just return the Identity Multiset
            context.OutputMultiset = new IdentityMultiset();
        }

        // If we've ended with an Empty Multiset then we turn it into the Null Multiset
        // to indicate that this BGP did not match anything
        if (context.OutputMultiset is Multiset && context.OutputMultiset.IsEmpty)
            context.OutputMultiset = new NullMultiset();

        // Return the Output Multiset
        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Bindings modifier.
    /// </summary>
    /// <param name="b">Bindings.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessBindings(Bindings b, SparqlEvaluationContext context)
    {
        if (!_bindingsCache.TryGetValue(b, out BaseMultiset multiSet))
        {
            multiSet = b.BindingsPattern.ToMultiset();
            _bindingsCache[b] = multiSet;
        }

        return multiSet;
    }

    /// <summary>
    /// Processes a Distinct modifier.
    /// </summary>
    /// <param name="distinct">Distinct modifier.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessDistinct(Distinct distinct, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        context.InputMultiset = distinct.InnerAlgebra.Accept(this, context);

        if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
        {
            context.OutputMultiset = context.InputMultiset;
            return context.OutputMultiset;
        }
        if (distinct.TrimTemporaryVariables)
        {
            // Trim temporary variables
            context.InputMultiset.Trim();
        }

        // Apply distinctness
        context.OutputMultiset = new Multiset(context.InputMultiset.Variables);
        IEnumerable<ISet> sets = context.InputMultiset.Sets.Distinct();
        foreach (ISet s in sets)
        {
            context.OutputMultiset.Add(s.Copy());
        }
        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes an Extend.
    /// </summary>
    /// <param name="extend">Extend.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessExtend(Extend extend, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        // First evaluate the inner algebra
        BaseMultiset results = extend.InnerAlgebra.Accept(this, context);
        context.OutputMultiset = new Multiset();

        if (results is NullMultiset)
        {
            context.OutputMultiset = results;
        }
        else if (results is IdentityMultiset)
        {
            context.OutputMultiset.AddVariable(extend.VariableName);
            var s = new Set();
            try
            {
                INode temp = extend.AssignExpression.Accept(_expressionProcessor, context, 0);
                s.Add(extend.VariableName, temp);
            }
            catch
            {
                // No assignment if there's an error
                s.Add(extend.VariableName, null);
            }
            context.OutputMultiset.Add(s.Copy());
        }
        else
        {
            if (results.ContainsVariable(extend.VariableName))
            {
                throw new RdfQueryException("Cannot assign to the variable ?" + extend.VariableName + "since it has previously been used in the Query");
            }

            context.InputMultiset = results;
            context.OutputMultiset.AddVariable(extend.VariableName);
            if (context.Options.UsePLinqEvaluation && extend.AssignExpression.CanParallelise)
            {
                results.SetIDs.AsParallel().ForAll(id => EvalExtend(extend, context, results, id));
            }
            else
            {
                foreach (var id in results.SetIDs)
                {
                    EvalExtend(extend, context, results, id);
                }
            }
        }

        return context.OutputMultiset;
    }

    private void EvalExtend(Extend extend, SparqlEvaluationContext context, BaseMultiset results, int id)
    {
        ISet s = results[id].Copy();
        try
        {
            // Make a new assignment
            INode temp = extend.AssignExpression.Accept(_expressionProcessor, context, id);
            s.Add(extend.VariableName, temp);
        }
        catch
        {
            // No assignment if there's an error but the solution is preserved
        }
        context.OutputMultiset.Add(s);
    }

    /// <summary>
    /// Processes an Exists Join.
    /// </summary>
    /// <param name="existsJoin">Exists Join.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessExistsJoin(IExistsJoin existsJoin, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        BaseMultiset lhsResult = existsJoin.Lhs.Accept(this, context);
        context.CheckTimeout();

        if (lhsResult is NullMultiset)
        {
            context.OutputMultiset = lhsResult;
        }
        else if (lhsResult.IsEmpty)
        {
            context.OutputMultiset = new NullMultiset();
        }
        else
        {
            // Only execute the RHS if the LHS had results
            context.InputMultiset = lhsResult;
            BaseMultiset rhsResult = existsJoin.Rhs.Accept(this, context);
            context.CheckTimeout();

            context.OutputMultiset = lhsResult.ExistsJoin(rhsResult, existsJoin.MustExist);
            context.CheckTimeout();
        }

        context.InputMultiset = context.OutputMultiset;
        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Filter.
    /// </summary>
    /// <param name="filter">Filter.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessFilter(IFilter filter, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        // Apply the Pattern first
        context.InputMultiset = filter.InnerAlgebra.Accept(this, context);

        if (context.InputMultiset is NullMultiset)
        {
            // If we get a NullMultiset then the FILTER has no effect since there are already no results
        }
        else if (context.InputMultiset is IdentityMultiset)
        {

            try
            {
                if (!filter.SparqlFilter.Expression.Accept(_expressionProcessor, context, 0).AsSafeBoolean())
                {
                    context.OutputMultiset = new NullMultiset();
                    return context.OutputMultiset;
                }
            }
            catch
            {
                context.OutputMultiset = new NullMultiset();
                return context.OutputMultiset;
            }

        }
        else
        {
            filter.SparqlFilter.Accept(this, context);
        }
        context.OutputMultiset = context.InputMultiset;
        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Graph.
    /// </summary>
    /// <param name="graph">Graph.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessGraph(Algebra.Graph graph, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        // Q: Can we optimise GRAPH when the input is the Null Multiset to just return the Null Multiset?

        var datasetOk = false;
        try
        {
            var activeGraphs = new List<string>();

            // Get the URIs of Graphs that should be evaluated over
            if (graph.GraphSpecifier.TokenType != Token.VARIABLE)
            {
                switch (graph.GraphSpecifier.TokenType)
                {
                    case Token.URI:
                    case Token.QNAME:
                        IRefNode activeGraphName = new UriNode(context.UriFactory.Create(Tools.ResolveUriOrQName(graph.GraphSpecifier, context.Query.NamespaceMap, context.Query.BaseUri)));
                        if (context.Data.HasGraph(activeGraphName))
                        {
                            // If the Graph is explicitly specified and there are FROM/FROM NAMED present then the Graph 
                            // URI must be in the graphs specified by a FROM/FROM NAMED or the result is null
                            if (context.Query == null ||
                                (!context.Query.DefaultGraphNames.Any() && !context.Query.NamedGraphNames.Any()) ||
                                context.Query.NamedGraphNames.Contains(activeGraphName))
                            {
                                // Either there was no Query 
                                // OR there were no Default/Named Graphs (hence any Graph URI is permitted) 
                                // OR the specified URI was a Named Graph URI
                                // In any case we can go ahead and set the active Graph
                                activeGraphs.Add(activeGraphName.ToSafeString());
                            }
                            else
                            {
                                // The specified URI was not present in the Named Graphs so return null
                                context.OutputMultiset = new NullMultiset();
                                return context.OutputMultiset;
                            }
                        }
                        else
                        {
                            // If specifies a specific Graph and not in the Dataset result is a null multiset
                            context.OutputMultiset = new NullMultiset();
                            return context.OutputMultiset;
                        }
                        break;
                    default:
                        throw new RdfQueryException("Cannot use a '" + graph.GraphSpecifier.GetType() + "' Token to specify the Graph for a GRAPH clause");
                }
            }
            else
            {
                var gvar = graph.GraphSpecifier.Value.Substring(1);

                // Watch out for the case in which the Graph Variable is not bound for all Sets in which case
                // we still need to operate over all Graphs
                if (context.InputMultiset.ContainsVariable(gvar) && context.InputMultiset.Sets.All(s => s[gvar] != null))
                {
                    // If there are already values bound to the Graph variable for all Input Solutions then we limit the Query to those Graphs
                    var graphUris = new List<Uri>();
                    foreach (ISet s in context.InputMultiset.Sets)
                    {
                        INode temp = s[gvar];
                        if (temp == null) continue;
                        if (temp.NodeType != NodeType.Uri) continue;
                        activeGraphs.Add(temp.ToString());
                        graphUris.Add(((IUriNode)temp).Uri);
                    }
                }
                else
                {
                    // Nothing yet bound to the Graph Variable so the Query is over all the named Graphs
                    if (context.Query != null && context.Query.NamedGraphNames.Any())
                    {
                        // Query specifies one/more named Graphs
                        activeGraphs.AddRange(context.Query.NamedGraphNames.Select(u => u.ToSafeString()));
                    }
                    else if (context.Query != null && context.Query.DefaultGraphNames.Any() && !context.Query.NamedGraphNames.Any())
                    {
                        // Gives null since the query dataset does not include any named graphs
                        context.OutputMultiset = new NullMultiset();
                        return context.OutputMultiset;
                    }
                    else
                    {
                        // Query is over entire dataset/default Graph since no named Graphs are explicitly specified
                        activeGraphs.AddRange(context.Data.GraphNames.Select(u => u.ToSafeString()));
                    }
                }
            }

            // Remove all duplicates from Active Graphs to avoid duplicate results
            activeGraphs = activeGraphs.Distinct().ToList();

            // Evaluate the inner pattern
            BaseMultiset initialInput = context.InputMultiset;
            BaseMultiset finalResult = new Multiset();

            // Evaluate for each Graph URI and union the results
            foreach (var uri in activeGraphs)
            {
                // Always use the same Input for each Graph URI and set that Graph to be the Active Graph
                // Be sure to translate String.Empty back to the null URI to select the default graph
                // correctly
                context.InputMultiset = initialInput;
                IRefNode currGraphName = uri.Equals(string.Empty) ? null :
                    uri.StartsWith("_:") ? (IRefNode)new BlankNode(uri) : new UriNode(context.UriFactory.Create(uri));

                // Set Active Graph
                if (currGraphName == null)
                {
                    // GRAPH operates over named graphs only so default graph gets skipped
                    continue;
                }
                // The result of the HasGraph() call is ignored we just make it so datasets with any kind of 
                // load on demand behaviour work properly
                context.Data.HasGraph(currGraphName);
                // All we actually care about is setting the active graph
                context.Data.SetActiveGraph(currGraphName);
                datasetOk = true;

                // Evaluate for the current Active Graph
                BaseMultiset result = graph.InnerAlgebra.Accept(this, context);

                // Merge the Results into our overall Results
                if (result is NullMultiset)
                {
                    // Don't do anything, adds nothing to the results
                }
                else if (result is IdentityMultiset)
                {
                    // Adds a single row to the results
                    if (graph.GraphSpecifier.TokenType == Token.VARIABLE)
                    {
                        // Include graph variable if not yet bound
                        INode currGraph = currGraphName;
                        var s = new Set();
                        s.Add(graph.GraphSpecifier.Value.Substring(1), currGraph);
                        finalResult.Add(s);
                    }
                    else
                    {
                        finalResult.Add(new Set());
                    }
                }
                else
                {
                    // If the Graph Specifier is a Variable then we must either bind the
                    // variable or eliminate solutions which have an incorrect value for it
                    if (graph.GraphSpecifier.TokenType == Token.VARIABLE)
                    {
                        var gvar = graph.GraphSpecifier.Value.Substring(1);
                        foreach (var id in result.SetIDs.ToList())
                        {
                            ISet s = result[id];
                            if (s[gvar] == null)
                            {
                                // If Graph Variable is not yet bound for solution bind it
                                s.Add(gvar, currGraphName);
                            }
                            else if (!s[gvar].Equals(currGraphName))
                            {
                                // If Graph Variable is bound for solution and doesn't match
                                // current Graph then we have to remove the solution
                                result.Remove(id);
                            }
                        }
                    }
                    // Union solutions into the Results
                    finalResult.Union(result);
                }

                // Reset the Active Graph after each pass
                context.Data.ResetActiveGraph();
                datasetOk = false;
            }

            // Return the final result
            if (finalResult.IsEmpty) finalResult = new NullMultiset();
            context.OutputMultiset = finalResult;
        }
        finally
        {
            if (datasetOk) context.Data.ResetActiveGraph();
        }

        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Group By.
    /// </summary>
    /// <param name="groupBy">Group By.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessGroupBy(GroupBy groupBy, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        BaseMultiset results = groupBy.InnerAlgebra.Accept(this, context);
        context.InputMultiset = results;

        // Identity/Null yields an empty multiset
        if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
        {
            results = new Multiset();
        }
        var groupSet = new GroupMultiset(results);
        List<BindingGroup> groups;

        // Calculate Groups
        if (context.Query.GroupBy != null)
        {
            groups = ApplyGrouping(context.Query.GroupBy, context);
        }
        else if (groupBy.Grouping != null)
        {
            groups = ApplyGrouping(groupBy.Grouping, context);
        }
        else
        {
            groups = new List<BindingGroup> { new BindingGroup(results.SetIDs) };
        }

        // Add Groups to the GroupMultiset
        var vars = new HashSet<string>();
        foreach (BindingGroup group in groups)
        {
            foreach (KeyValuePair<string, INode> assignment in group.Assignments)
            {
                if (vars.Contains(assignment.Key)) continue;

                groupSet.AddVariable(assignment.Key);
                vars.Add(assignment.Key);
            }
            groupSet.AddGroup(group);
        }
        // If grouping produced no groups, there are aggregates present, then an implicit group is created unless the input multiset was empty and explicit grouping was specified.
        if (groups.Count == 0 && groupBy.Aggregates.Any() && !(context.InputMultiset.IsEmpty && (groupBy.Grouping != null || context.Query.GroupBy != null)))
        {
            groupSet.AddGroup(new BindingGroup());
        }

        // Apply the aggregates
        context.InputMultiset = groupSet;
        context.Binder.SetGroupContext(true);
        foreach (SparqlVariable var in groupBy.Aggregates)
        {
            if (!vars.Contains(var.Name))
            {
                groupSet.AddVariable(var.Name);
                vars.Add(var.Name);
            }

            foreach (ISet s in groupSet.Sets)
            {
                try
                {
                    INode value = var.Aggregate.Accept(_aggregateProcessor, context, groupSet.GroupSetIDs(s.ID));
                    s.Add(var.Name, value);
                }
                catch (RdfQueryException)
                {
                    s.Add(var.Name, null);
                }
            }
        }
        context.Binder.SetGroupContext(false);

        context.OutputMultiset = groupSet;
        return context.OutputMultiset;
    }

    private List<BindingGroup> ApplyGrouping(ISparqlGroupBy groupBy, SparqlEvaluationContext context)
    {
        var groups = new Dictionary<INode, BindingGroup>();
        var nulls = new BindingGroup();
        var error = new BindingGroup();
        foreach (var id in context.Binder.BindingIDs)
        {
            try
            {
                INode value = groupBy.Expression.Accept(_expressionProcessor, context, id);
                if (value != null)
                {
                    if (!groups.TryGetValue(value, out BindingGroup group))
                    {
                        group = new BindingGroup();
                        if (groupBy.AssignVariable != null)
                        {
                            group.AddAssignment(groupBy.AssignVariable, value);
                        }

                        groups.Add(value, group);
                    }

                    group.Add(id);
                }
                else
                {
                    nulls.Add(id);
                }
            }
            catch (RdfQueryException)
            {
                error.Add(id);
            }
        }

        // Build the List of Groups
        // Null and Error Group are included if required
        var parentGroups = (from g in groups.Values select g).ToList();
        if (error.BindingIDs.Any())
        {
            parentGroups.Add(error);
            if (groupBy.AssignVariable != null) error.AddAssignment(groupBy.AssignVariable, null);
        }
        if (nulls.BindingIDs.Any())
        {
            parentGroups.Add(nulls);
            if (groupBy.AssignVariable != null) nulls.AddAssignment(groupBy.AssignVariable, null);
        }

        return groupBy.Child != null ? ApplyGrouping(groupBy.Child, context, parentGroups) : parentGroups;
    }

    private List<BindingGroup> ApplyGrouping(ISparqlGroupBy groupBy, SparqlEvaluationContext context,
        List<BindingGroup> groups)
    {
        var outGroups = new List<BindingGroup>();
        foreach (BindingGroup group in groups)
        {
            var subgroups = new Dictionary<INode, BindingGroup>();
            var error = new BindingGroup();
            var nulls = new BindingGroup(group);

            foreach (var id in group.BindingIDs)
            {
                try
                {
                    INode value = groupBy.Expression.Accept(_expressionProcessor, context, id);

                    if (value != null)
                    {
                        if (!subgroups.ContainsKey(value))
                        {
                            subgroups.Add(value, new BindingGroup(group));
                            if (groupBy.AssignVariable != null)
                            {
                                subgroups[value].AddAssignment(groupBy.AssignVariable, value);
                            }
                        }

                        subgroups[value].Add(id);
                    }
                    else
                    {
                        nulls.Add(id);
                    }
                }
                catch (RdfQueryException)
                {
                    error.Add(id);
                }
            }

            // Build the List of Groups
            // Null and Error Group are included if required
            foreach (BindingGroup g in subgroups.Values)
            {
                outGroups.Add(g);
            }
            if (error.BindingIDs.Any())
            {
                outGroups.Add(error);
                if (groupBy.AssignVariable != null) error.AddAssignment(groupBy.AssignVariable, null);
                error = new BindingGroup();
            }
            if (nulls.BindingIDs.Any())
            {
                outGroups.Add(nulls);
                if (groupBy.AssignVariable != null) nulls.AddAssignment(groupBy.AssignVariable, null);
                nulls = new BindingGroup();
            }
        }

        return groupBy.Child == null ? outGroups : ApplyGrouping(groupBy.Child, context, outGroups);
    }
    /// <summary>
    /// Processes a Having.
    /// </summary>
    /// <param name="having">Having.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessHaving(Having having, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        context.InputMultiset = having.InnerAlgebra.Accept(this, context);

        if (context.Query != null)
        {
            context.Query.Having?.Accept(this, context);
        }
        else
        {
            having.HavingClause?.Accept(this, context);
        }

        context.OutputMultiset = context.InputMultiset;
        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Join.
    /// </summary>
    /// <param name="join">Join.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessJoin(IJoin join, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        if (join is ParallelJoin pj) return EvaluateParallelJoin(pj, context);
        BaseMultiset lhsResult = join.Lhs.Accept(this, context);
        context.CheckTimeout();

        if (lhsResult is NullMultiset)
        {
            context.OutputMultiset = lhsResult;
        }
        else if (lhsResult.IsEmpty)
        {
            context.OutputMultiset = new NullMultiset();
        }
        else
        {
            // Only Execute the RHS if the LHS has some results
            context.InputMultiset = lhsResult;
            BaseMultiset rhsResult = join.Rhs.Accept(this, context);
            context.CheckTimeout();

            context.OutputMultiset = lhsResult.Join(rhsResult);
            context.CheckTimeout();
        }

        context.InputMultiset = context.OutputMultiset;
        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a LeftJoin.
    /// </summary>
    /// <param name="leftJoin">Left Join.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessLeftJoin(ILeftJoin leftJoin, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        // Need to be careful about whether we linearize (CORE-406)
        if (!CanLinearizeLhs(leftJoin, context))
        {
            context.InputMultiset = new IdentityMultiset();
        }

        BaseMultiset lhsResult = leftJoin.Lhs.Accept(this, context);
        context.CheckTimeout();

        if (lhsResult is NullMultiset)
        {
            context.OutputMultiset = lhsResult;
        }
        else if (lhsResult.IsEmpty)
        {
            context.OutputMultiset = new NullMultiset();
        }
        else
        {
            // Only execute the RHS if the LHS had some results
            // Need to be careful about whether we linearize (CORE-406)
            context.InputMultiset = CanFlowResultsToRhs(leftJoin) && !IsCrossProduct(leftJoin) ? lhsResult : new IdentityMultiset();
            BaseMultiset rhsResult = leftJoin.Rhs.Accept(this, context);
            context.CheckTimeout();

            context.OutputMultiset = lhsResult.LeftJoin(rhsResult, leftJoin.Filter.Expression, context, _expressionProcessor);
            context.CheckTimeout();
        }

        context.InputMultiset = context.OutputMultiset;
        return context.OutputMultiset;
    }

    private static bool CanLinearizeLhs(IAbstractJoin leftJoin, SparqlEvaluationContext context)
    {
        // Must be no floating variables already present in the results to be flowed
        return leftJoin.Lhs.FloatingVariables.All(v => !context.InputMultiset.ContainsVariable(v));
    }

    private static bool CanFlowResultsToRhs(IAbstractJoin leftJoin)
    {
        // Can't have any conflicting variables
        var lhsFixed = new HashSet<string>(leftJoin.Lhs.FixedVariables);
        var lhsFloating = new HashSet<string>(leftJoin.Lhs.FloatingVariables);
        var rhsFloating = new HashSet<string>(leftJoin.Rhs.FloatingVariables);
        var rhsFixed = new HashSet<string>(leftJoin.Rhs.FixedVariables);

        // RHS Floating can't be floating/fixed on LHS
        if (rhsFloating.Any(v => lhsFloating.Contains(v) || lhsFixed.Contains(v))) return false;
        // RHS Fixed can't be floating on LHS
        if (rhsFixed.Any(v => lhsFloating.Contains(v))) return false;

        // Otherwise OK
        return true;
    }

    private static bool IsCrossProduct(IAbstractJoin join)
    {
        return join.Lhs.Variables.IsDisjoint(join.Rhs.Variables);
    }

    /// <summary>
    /// Processes a Minus.
    /// </summary>
    /// <param name="minus">Minus.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessMinus(IMinus minus, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        BaseMultiset initialInput = context.InputMultiset;
        BaseMultiset lhsResult = minus.Lhs.Accept(this, context);
        context.CheckTimeout();

        if (lhsResult is NullMultiset)
        {
            context.OutputMultiset = lhsResult;
        }
        else if (lhsResult.IsEmpty)
        {
            context.OutputMultiset = new NullMultiset();
        }
        else if (minus.Lhs.Variables.IsDisjoint(minus.Rhs.Variables))
        {
            // If the RHS is disjoint then there is no need to evaluate the RHS
            context.OutputMultiset = lhsResult;
        }
        else
        {
            // If we get here then the RHS is not disjoint so it does affect the ouput

            // Only execute the RHS if the LHS had results
            // context.InputMultiset = lhsResult;
            context.InputMultiset = initialInput;
            BaseMultiset rhsResult = minus.Rhs.Accept(this, context);
            context.CheckTimeout();

            context.OutputMultiset = lhsResult.MinusJoin(rhsResult);
            context.CheckTimeout();
        }

        context.InputMultiset = context.OutputMultiset;
        return context.OutputMultiset;

    }

    /// <summary>
    /// Processes a Negated Property Set.
    /// </summary>
    /// <param name="negPropSet">Negated Property Set.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    /// <returns></returns>
    public virtual BaseMultiset ProcessNegatedPropertySet(NegatedPropertySet negPropSet, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        IEnumerable<Triple> ts;
        var subjVars = negPropSet.PathStart.Variables.ToList();
        var objVars = negPropSet.PathEnd.Variables.ToList();
        if (subjVars.Any() && context.InputMultiset.ContainsVariables(subjVars))
        {
            if (objVars.Any() && context.InputMultiset.ContainsVariables(objVars))
            {
                ts = from s in context.InputMultiset.Sets
                    where s.BindsAll(subjVars) && s.BindsAll(objVars)
                    from triple in context.Data.GetTriplesWithSubjectObject(negPropSet.PathStart.Bind(s),
                        negPropSet.PathStart.Bind(s))
                    select triple;
            }
            else
            {
                ts = from s in context.InputMultiset.Sets
                    let subj = negPropSet.PathStart.Bind(s)
                    where subj != null
                    from t in context.Data.GetTriplesWithSubject(subj)
                    select t;
            }
        }
        else if (objVars.Any() && context.InputMultiset.ContainsVariables(objVars))
        {
            ts = from s in context.InputMultiset.Sets
                where s.BindsAll(objVars)
                from t in context.Data.GetTriplesWithObject(negPropSet.PathEnd.Bind(s))
                select t;
        }
        else
        {
            ts = context.Data.Triples;
        }

        context.OutputMultiset = new Multiset();
        ISet<INode> properties = new HashSet<INode>(negPropSet.Properties);

        // Q: Should this not go at the start of evaluation?
        if (negPropSet.Inverse)
        {
            (subjVars, objVars) = (objVars, subjVars);
        }
        foreach (Triple t in ts)
        {
            if (!properties.Contains(t.Predicate))
            {
                var s = new Set();
                negPropSet.PathStart.AddBindings(t.Subject, s);
                negPropSet.PathEnd.AddBindings(t.Object, s);
                context.OutputMultiset.Add(s);
            }
        }

        if (!subjVars.Any() && !objVars.Any())
        {
            if (context.OutputMultiset.Count == 0)
            {
                context.OutputMultiset = new NullMultiset();
            }
            else
            {
                context.OutputMultiset = new IdentityMultiset();
            }
        }

        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Null Operator.
    /// </summary>
    /// <param name="nullOp">Null Operator.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    /// <returns></returns>
    public virtual BaseMultiset ProcessNullOperator(NullOperator nullOp, SparqlEvaluationContext context)
    {
        return new NullMultiset();
    }

    /// <summary>
    /// Processes a One or More Path.
    /// </summary>
    /// <param name="oneOrMorePath">Path.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    /// <returns></returns>
    public virtual BaseMultiset ProcessOneOrMorePath(OneOrMorePath oneOrMorePath, SparqlEvaluationContext context)
    {
        if (context == null) context = GetContext();
        var paths = new List<List<INode>>();
        BaseMultiset initialInput = context.InputMultiset;
        int step = 0, prevCount = 0, skipCount = 0;

        var subjVars = oneOrMorePath.PathStart.Variables.ToList();
        var objVars = oneOrMorePath.PathEnd.Variables.ToList();
        var bothTerms = !subjVars.Any() && !objVars.Any();
        var reverse = false;

        if (context.InputMultiset.ContainsVariables(subjVars) || !context.InputMultiset.ContainsVariables(objVars))
        {
            // Work Forwards from the Starting Term or Bound Variable
            // OR if there is no Ending Term or Bound Variable work forwards regardless
            if (!subjVars.Any())
            {
                paths.Add(((NodeMatchPattern)oneOrMorePath.PathStart).Node.AsEnumerable().ToList());
            }
            else
            {
                paths.AddRange(context.InputMultiset.Sets
                    .Where(s=>s.BindsAll(subjVars))
                    .Select(s => oneOrMorePath.PathStart.Bind(s))
                    .Distinct()
                    .Select(n => n.AsEnumerable().ToList()));
            }
        }
        else if (context.InputMultiset.ContainsVariables(objVars))
        {
            // Work Backwards from Ending Term or Bound Variable
            if (!objVars.Any())
            {
                paths.Add(((NodeMatchPattern)oneOrMorePath.PathEnd).Node.AsEnumerable().ToList());
            }
            else
            {
                paths.AddRange(context.InputMultiset.Sets
                    .Where(s => s.BindsAll(objVars))
                    .Select(s => oneOrMorePath.PathEnd.Bind(s))
                    .Distinct()
                    .Select(n => n.AsEnumerable().ToList()));
            }
            reverse = true;
        }

        if (paths.Count == 0)
        {
            GetPathStarts(oneOrMorePath, context, paths, reverse);
        }

        // Traverse the Paths
        do
        {
            prevCount = paths.Count;
            foreach (List<INode> path in paths.Skip(skipCount).ToList())
            {
                foreach (INode nextStep in EvaluateStep(oneOrMorePath, context, path, reverse))
                {
                    var newPath = new List<INode>(path) {nextStep};
                    paths.Add(newPath);
                }
            }

            if (step == 0)
            {
                // Remove any 1 length paths as these denote path starts that couldn't be traversed
                paths.RemoveAll(p => p.Count == 1);
                prevCount = paths.Count;
            }

            // Update Counts
            // skipCount is used to indicate the paths which we will ignore for the purposes of
            // trying to further extend since we've already done them once
            step++;
            if (paths.Count == 0) break;
            if (step > 1)
            {
                skipCount = prevCount;
            }

            // Can short circuit evaluation here if both are terms and any path is acceptable
            if (bothTerms)
            {
                var exit = false;
                foreach (List<INode> path in paths)
                {
                    var s = new Set();
                    if (reverse)
                    {
                        if (oneOrMorePath.PathEnd.Accepts(context, path[0], s) && oneOrMorePath.PathStart.Accepts(context, path[path.Count - 1], s)) // - should these be using different sets?
                        {
                            exit = true;
                            break;
                        }
                    }
                    else
                    {
                        if (oneOrMorePath.PathStart.Accepts(context, path[0], s) && oneOrMorePath.PathEnd.Accepts(context, path[path.Count - 1], s))
                        {
                            exit = true;
                            break;
                        }
                    }
                }
                if (exit) break;
            }
        } while (paths.Count > prevCount || (step == 1 && paths.Count == prevCount));

        if (paths.Count == 0)
        {
            // If all path starts lead nowhere then we get the Null Multiset as a result
            context.OutputMultiset = new NullMultiset();
        }
        else
        {
            context.OutputMultiset = new Multiset();

            // Evaluate the Paths to check that are acceptable
            var returnedPaths = new HashSet<ISet>();
            foreach (List<INode> path in paths)
            {
                if (reverse)
                {
                    var s = new Set();
                    if (oneOrMorePath.PathEnd.Accepts(context, path[0], s) && oneOrMorePath.PathStart.Accepts(context, path[path.Count - 1], s))
                    {
                        // Make sure to check for uniqueness
                        if (returnedPaths.Contains(s)) continue;
                        context.OutputMultiset.Add(s);
                        returnedPaths.Add(s);

                        // If both are terms can short circuit evaluation here
                        // It is sufficient just to determine that there is one path possible
                        if (bothTerms) break;
                    }
                }
                else
                {
                    var s = new Set();
                    if (oneOrMorePath.PathStart.Accepts(context, path[0], s) && oneOrMorePath.PathEnd.Accepts(context, path[path.Count - 1], s))
                    {
                        // Make sure to check for uniqueness
                        if (returnedPaths.Contains(s)) continue;
                        context.OutputMultiset.Add(s);
                        returnedPaths.Add(s);

                        // If both are terms can short circuit evaluation here
                        // It is sufficient just to determine that there is one path possible
                        if (bothTerms) break;
                    }
                }
            }

            if (bothTerms)
            {
                // If both were terms transform to an Identity/Null Multiset as appropriate
                if (context.OutputMultiset.IsEmpty)
                {
                    context.OutputMultiset = new NullMultiset();
                }
                else
                {
                    context.OutputMultiset = new IdentityMultiset();
                }
            }
        }

        context.InputMultiset = initialInput;
        return context.OutputMultiset;
    }



    /// <summary>
    /// Determines the starting points for Path evaluation.
    /// </summary>
    /// <param name="pathOperator">Path Operator.</param>
    /// <param name="context">Evaluation Context.</param>
    /// <param name="paths">Paths.</param>
    /// <param name="reverse">Whether to evaluate Paths in reverse.</param>
    protected void GetPathStarts(IPathOperator pathOperator, SparqlEvaluationContext context, List<List<INode>> paths, bool reverse)
    {
        var nodes = new HashSet<KeyValuePair<INode, INode>>();
        if (pathOperator is ZeroOrMorePath)
        {
            // Include the trivial identity mapping for any node that is not a predicate node
            // ISparqlDataset does not currently provide an accessor for all nodes, so construct the distinct set here
            var distinctNodes = new HashSet<INode>();
            foreach (Triple t in context.Data.Triples)
            {
                distinctNodes.Add(t.Subject);
                distinctNodes.Add(t.Object);
            }
            foreach (INode node in distinctNodes)
            {
                nodes.Add(new KeyValuePair<INode, INode>(node, node));
            }
        }
        if (pathOperator.Path is Property path)
        {
            INode predicate = path.Predicate;
            foreach (Triple t in context.Data.GetTriplesWithPredicate(predicate))
            {
                nodes.Add(reverse
                    ? new KeyValuePair<INode, INode>(t.Object, t.Subject)
                    : new KeyValuePair<INode, INode>(t.Subject, t.Object));
            }
        }
        else
        {
            BaseMultiset initialInput = context.InputMultiset;
            context.InputMultiset = new IdentityMultiset();
            var x = new VariablePattern("?x");
            var y = new VariablePattern("?y");
            var bgp = new Bgp(new PropertyPathPattern(x, pathOperator.Path, y));

            BaseMultiset results = context.Evaluate(bgp); //bgp.Evaluate(context);
            context.InputMultiset = initialInput;

            if (!results.IsEmpty)
            {
                foreach (ISet s in results.Sets)
                {
                    if (s["x"] != null && s["y"] != null)
                    {
                        if (reverse)
                        {
                            nodes.Add(new KeyValuePair<INode, INode>(s["y"], s["x"]));
                        }
                        else
                        {
                            nodes.Add(new KeyValuePair<INode, INode>(s["x"], s["y"]));
                        }
                    }
                }
            }
        }

        paths.AddRange(nodes.Select(kvp => new List<INode>(new INode[] { kvp.Key, kvp.Value })));
    }

    /// <summary>
    /// Evaluates a step of the Path.
    /// </summary>
    /// <param name="pathOperator">Path operator.</param>
    /// <param name="context">Context.</param>
    /// <param name="path">Paths.</param>
    /// <param name="reverse">Whether to evaluate Paths in reverse.</param>
    /// <returns></returns>
    protected List<INode> EvaluateStep(IPathOperator pathOperator, SparqlEvaluationContext context, List<INode> path, bool reverse)
    {
        if (pathOperator.Path is Property pathOperatorPath)
        {
            var nodes = new HashSet<INode>();
            INode predicate = pathOperatorPath.Predicate;
            IEnumerable<Triple> ts = reverse 
                ? context.Data.GetTriplesWithPredicateObject(predicate, path[path.Count - 1]) 
                : context.Data.GetTriplesWithSubjectPredicate(path[path.Count - 1], predicate);
            foreach (Triple t in ts)
            {
                if (reverse)
                {
                    if (!path.Contains(t.Subject))
                    {
                        nodes.Add(t.Subject);
                    }
                }
                else
                {
                    if (!path.Contains(t.Object))
                    {
                        nodes.Add(t.Object);
                    }
                }
            }
            return nodes.ToList();
        }
        else
        {
            var nodes = new HashSet<INode>();

            BaseMultiset initialInput = context.InputMultiset;
            var currInput = new Multiset();
            var x = new VariablePattern("?x");
            var y = new VariablePattern("?y");
            var temp = new Set();
            if (reverse)
            {
                temp.Add("y", path[path.Count - 1]);
            }
            else
            {
                temp.Add("x", path[path.Count - 1]);
            }
            currInput.Add(temp);
            context.InputMultiset = currInput;

            var bgp = new Bgp(new PropertyPathPattern(x, pathOperator.Path, y));
            BaseMultiset results = context.Evaluate(bgp); //bgp.Evaluate(context);
            context.InputMultiset = initialInput;

            if (!results.IsEmpty)
            {
                foreach (ISet s in results.Sets)
                {
                    if (reverse)
                    {
                        if (s["x"] != null)
                        {
                            if (!path.Contains(s["x"]))
                            {
                                nodes.Add(s["x"]);
                            }
                        }
                    }
                    else
                    {
                        if (s["y"] != null)
                        {
                            if (!path.Contains(s["y"]))
                            {
                                nodes.Add(s["y"]);
                            }
                        }
                    }
                }
            }

            return nodes.ToList();
        }
    }

    /// <summary>
    /// Processes an Order By.
    /// </summary>
    /// <param name="orderBy"></param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessOrderBy(OrderBy orderBy, SparqlEvaluationContext context)
    {
        if (context == null) context = GetContext();
        context.InputMultiset = orderBy.InnerAlgebra.Accept(this, context);

        if (context.Query != null)
        {
            if (context.Query.OrderBy != null)
            {
                //context.Query.OrderBy.Context = context;
                
                context.InputMultiset.Sort(Comparer<ISet>.Create(MakeSetComparer(context.Query.OrderBy, context)));
                //context.InputMultiset.Sort(context.Query.OrderBy);
            }
        }
        else if (orderBy.Ordering != null)
        {
            context.InputMultiset.Sort(Comparer<ISet>.Create(MakeSetComparer(orderBy.Ordering, context)));
        }
        context.OutputMultiset = context.InputMultiset;
        return context.OutputMultiset;
    }

    private Comparison<ISet> MakeSetComparer(ISparqlOrderBy ordering, SparqlEvaluationContext context)
    {
        switch (ordering)
        {
            case OrderByVariable obv:
                return (x, y) => CompareSets(x, y, obv, context);
            case OrderByExpression obe:
                return (x, y) => CompareSets(x, y, obe, context);
            default:
                throw new RdfQueryException("Unable to process ordering algebra " + ordering.GetType());
        }
    }

    private int CompareSets(ISet x, ISet y, OrderByVariable obv, SparqlEvaluationContext context)
    {
        INode xval = x[obv.Variable];
        if (xval == null)
        {
            if (y[obv.Variable] == null)
            {
                return obv.Child != null ? MakeSetComparer(obv.Child, context).Invoke(x, y) : 0;
            }

            return obv.Descending ? 1 : -1;
        }
        else
        {
            var c = context.OrderingComparer.Compare(xval, y[obv.Variable]);

            if (c == 0 && obv.Child != null)
            {
                return MakeSetComparer(obv.Child, context).Invoke(x, y);
            }
            else
            {
                return c * (obv.Descending ? -1 : 1);
            }
        }
    }

    private int CompareSets(ISet x, ISet y, OrderByExpression ordering, SparqlEvaluationContext context)
    {
        if (context == null)
        {
            return 0;
        }
        else if (x.ID == y.ID)
        {
            return 0;
        }
        else
        {
            INode a, b;
            try
            {
                a = ordering.Expression.Accept(_expressionProcessor, context, x.ID);

                try
                {
                    b = ordering.Expression.Accept(_expressionProcessor, context, y.ID);
                }
                catch
                {
                    // If evaluating b errors consider this a NULL and rank a > b
                    return ordering.Descending ? -1 : 1;
                }

                // If both give a value then compare
                if (a != null)
                {
                    var c = context.OrderingComparer.Compare(a, b);
                    if (c == 0 && ordering.Child != null)
                    {
                        return MakeSetComparer(ordering.Child, context).Invoke(x, y);
                    }
                    else
                    {
                        return c * (ordering.Descending ? -1 : 1);
                    }
                }
                else
                {
                    // a is NULL so a < b
                    return ordering.Descending ? 1 : -1;
                }
            }
            catch
            {
                try
                {
                    ordering.Expression.Accept(_expressionProcessor, context, y.ID);

                    // If evaluating a errors but b evaluates correctly consider a to be NULL and rank a < b
                    return ordering.Descending ? 1 : -1;
                }
                catch
                {
                    // If both error then use child if any to evaluate, otherwise consider a = b
                    if (ordering.Child != null)
                    {
                        return MakeSetComparer(ordering.Child, context).Invoke(x, y);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

        }
    }
    /// <summary>
    /// Processes a Property Path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    /// <returns></returns>
    public virtual BaseMultiset ProcessPropertyPath(PropertyPath path, SparqlEvaluationContext context)
    {
        if (context == null) context = GetContext();
        // Try and generate an Algebra expression
        // Make sure we don't generate clashing temporary variable IDs over the life of the
        // Evaluation
        var transformContext = new PathTransformContext(path.PathStart, path.PathEnd);
        if (context["PathTransformID"] != null)
        {
            transformContext.NextID = (int)context["PathTransformID"];
        }
        ISparqlAlgebra algebra = path.Path.ToAlgebra(transformContext);
        context["PathTransformID"] = transformContext.NextID;

        // Now we can evaluate the resulting algebra
        BaseMultiset initialInput = context.InputMultiset;
        var trimMode = context.TrimTemporaryVariables;
        var rigMode = context.Options.RigorousEvaluation;
        try
        {
            // Must enable rigorous evaluation or we get incorrect interactions between property and non-property path patterns
            context.Options.RigorousEvaluation = true;

            // Note: We may need to preserve Blank Node variables across evaluations
            // which we usually don't do BUT because of the way we translate only part of the path
            // into an algebra at a time and may need to do further nested translate calls we do
            // need to do this here
            context.TrimTemporaryVariables = false;
            BaseMultiset result = context.Evaluate(algebra);

            // Also note that we don't trim temporary variables here even if we've set the setting back
            // to enabled since a Trim will be done at the end of whatever BGP we are being evaluated in

            // Once we have our results can join then into our input
            context.OutputMultiset = initialInput.Join(result);
        }
        finally
        {
            context.TrimTemporaryVariables = trimMode;
            context.Options.RigorousEvaluation = rigMode;
        }

        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Reduced modifier.
    /// </summary>
    /// <param name="reduced">Reduced modifier.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessReduced(Reduced reduced, SparqlEvaluationContext context)
    {
        if (context == null) context = GetContext();
        context.InputMultiset = reduced.InnerAlgebra.Accept(this, context);

        if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
        {
            context.OutputMultiset = context.InputMultiset;
            return context.OutputMultiset;
        }

        if (context.Query.Limit > 0)
        {
            context.OutputMultiset = new Multiset(context.InputMultiset.Variables);
            foreach (ISet s in context.InputMultiset.Sets.Distinct())
            {
                context.OutputMultiset.Add(s.Copy());
            }
        }
        else
        {
            context.OutputMultiset = context.InputMultiset;
        }
        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Select.
    /// </summary>
    /// <param name="select">Select.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessSelect(Select select, SparqlEvaluationContext context)
    {
        if (context == null) context = GetContext();
        try
        {
            context.InputMultiset = select.InnerAlgebra.Accept(this, context);
        }
        catch (RdfQueryTimeoutException)
        {
            // If not partial results throw the error
            if (context.Query == null || !context.Query.PartialResultsOnTimeout) throw;
        }

        // Ensure expected variables are present
        var vars = new HashSet<SparqlVariable>(select.SparqlVariables);
        if (context.InputMultiset is NullMultiset)
        {
            context.InputMultiset = new Multiset(vars.Select(v => v.Name));
        }
        else if (context.InputMultiset is IdentityMultiset)
        {
            context.InputMultiset = new Multiset(vars.Select(v => v.Name));
            context.InputMultiset.Add(new Set());
        }
        else if (context.InputMultiset.IsEmpty)
        {
            foreach (SparqlVariable var in vars)
            {
                context.InputMultiset.AddVariable(var.Name);
            }
        }

        // Trim Variables that aren't being SELECTed
        if (!select.IsSelectAll)
        {
            foreach (var var in context.InputMultiset.Variables.ToList())
            {
                if (!vars.Any(v => v.Name.Equals(var) && v.IsResultVariable))
                {
                    // If not a Result variable then trim from results
                    context.InputMultiset.Trim(var);
                }
            }
        }

        // Ensure all SELECTed variables are present
        foreach (SparqlVariable var in vars)
        {
            if (!context.InputMultiset.ContainsVariable(var.Name))
            {
                context.InputMultiset.AddVariable(var.Name);
            }
        }

        context.OutputMultiset = context.InputMultiset;

        // Apply variable ordering if applicable
        if (!select.IsSelectAll && (context.Query == null || SparqlSpecsHelper.IsSelectQuery(context.Query.QueryType)))
        {
            context.OutputMultiset.SetVariableOrder(context.Query.Variables.Where(v => v.IsResultVariable).Select(v => v.Name));
        }
        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Select Distinct Graphs.
    /// </summary>
    /// <param name="selDistGraphs">Select Distinct Graphs.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs, SparqlEvaluationContext context)
    {
        if (context == null) context = GetContext();
        context.OutputMultiset = new Multiset();
        var var = context.Query != null ? context.Query.Variables.First(v => v.IsResultVariable).Name : selDistGraphs.GraphVariable;

        foreach (IRefNode graphName in context.Data.GraphNames)
        {
            var s = new Set();
            s.Add(var, graphName);
            context.OutputMultiset.Add(s);
        }

        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Service.
    /// </summary>
    /// <param name="service">Service.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessService(Service service, SparqlEvaluationContext context)
    {
        if (context == null) context = GetContext();
        ISparqlQueryClient endpoint = GetRemoteEndpoint(service.EndpointSpecifier, context);
        try
        {
            context.OutputMultiset = new Multiset();
            foreach (SparqlQuery query in GetRemoteQueries(context, service.Pattern, GetBindings(context, service.Pattern)))
            {
                // Try and get a Result Set from the Service
                var cts = new CancellationTokenSource();
                var remainingTimeMillis = context.RemainingTimeout;
                if (remainingTimeMillis > 0)
                {
                    cts.CancelAfter(TimeSpan.FromMilliseconds(remainingTimeMillis));
                }

                Task<SparqlResultSet> task = endpoint.QueryWithResultSetAsync(query.ToString(), cts.Token);
                task.Wait(cts.Token);
                context.CheckTimeout();

                // Transform this Result Set back into a Multiset
                foreach (ISparqlResult r in task.Result)
                {
                    var set = new Set();
                    foreach(var v in r.Variables) { set.Add(v, r[v]);}
                    context.OutputMultiset.Add(set);
                }
            }
            return context.OutputMultiset;
        }
        catch (Exception ex)
        {
            if (service.Silent)
            {
                // If Evaluation Errors are SILENT is specified then a Multiset containing a single set with all values unbound is returned
                // Unless some of the SPARQL queries did return results in which we just return the results we did obtain
                if (context.OutputMultiset.IsEmpty)
                {
                    var s = new Set();
                    foreach (var variable in service.Pattern.Variables.Distinct())
                    {
                        s.Add(variable, null);
                    }
                    context.OutputMultiset.Add(s);
                }
                return context.OutputMultiset;
            }
            else
            {
                throw new RdfQueryException("Query execution failed because evaluating a SERVICE clause failed - this may be due to an error with the remote service", ex);
            }
        }
    }

    private ISet[] GetBindings(SparqlEvaluationContext context, GraphPattern pattern)
    {
        var bindings = new HashSet<ISet>();
        var existingVars = (from v in pattern.Variables
                                     where context.InputMultiset.ContainsVariable(v)
                                     select v).ToList();

        if (existingVars.Any() || context.Query.Bindings != null)
        {
            // Build the set of possible bindings

            if (context.Query.Bindings != null && !pattern.Variables.IsDisjoint(context.Query.Bindings.Variables))
            {
                // Possible Bindings comes from BINDINGS clause
                // In this case each possibility is a distinct binding tuple defined in the BINDINGS clause
                foreach (BindingTuple tuple in context.Query.Bindings.Tuples)
                {
                    var set = new Set();
                    foreach (KeyValuePair<string, PatternItem> binding in tuple.Values)
                    {
                        set.Add(binding.Key, tuple[binding.Key]);
                    }
                    bindings.Add(set);
                }
            }
            else
            {
                // Possible Bindings get built from current input (if there was a BINDINGS clause the variables it defines are not in this SERVICE clause)
                // In this case each possibility only contains Variables bound so far
                foreach (ISet s in context.InputMultiset.Sets)
                {
                    var t = new Set();
                    foreach (var var in existingVars)
                    {
                        t.Add(var, s[var]);
                    }
                    bindings.Add(t);
                }
            }
        }

        return bindings.ToArray();
    }

    private IEnumerable<SparqlQuery> GetRemoteQueries(SparqlEvaluationContext context, GraphPattern pattern, ISet[] bindings)
    {
        if (bindings.Length == 0)
        {
            // No pre-bound variables/BINDINGS clause so just return the query
            yield return GetRemoteQuery(context, pattern);
        }
        else
        {
            // Split bindings in chunks and inject them
            foreach (ISet[] chunk in bindings.ChunkBy(100))
            {
                IEnumerable<string> vars = chunk.SelectMany(x => x.Variables).Distinct();
                var data = new BindingsPattern(vars);
                foreach (ISet set in chunk)
                {
                    var tuple = new BindingTuple(
                        new List<string>(set.Variables),
                        new List<PatternItem>(set.Values.Select(x => new NodeMatchPattern(x))));
                    data.AddTuple(tuple);
                }
                SparqlQuery sparqlQuery = GetRemoteQuery(context, pattern);
                sparqlQuery.RootGraphPattern.AddInlineData(data);
                yield return sparqlQuery;
            }
        }
    }

    private SparqlQuery GetRemoteQuery(SparqlEvaluationContext context, GraphPattern pattern)
    {
        // Pass through LIMIT and OFFSET to the remote service

        // Calculate a LIMIT which is the LIMIT plus the OFFSET
        // We'll apply OFFSET locally so don't pass that through explicitly
        var limit = context.Query.Limit;
        if (context.Query.Offset > 0) limit += context.Query.Offset;

        return SparqlQuery.FromServiceQuery(pattern, limit);
    }

    private ISparqlQueryClient GetRemoteEndpoint(IToken endpointSpecifier, SparqlEvaluationContext context)
    {
        if (endpointSpecifier.TokenType == Token.URI)
        {
            var baseUri = (context.Query.BaseUri == null) ? String.Empty : context.Query.BaseUri.AbsoluteUri;
            Uri endpointUri = context.UriFactory.Create(Tools.ResolveUri(endpointSpecifier.Value, baseUri));
            return new SparqlQueryClient(context.GetHttpClient(endpointUri), endpointUri);
        }

        if (endpointSpecifier.TokenType == Token.VARIABLE)
        {
            // Get all the URIs that are bound to this Variable in the Input
            var var = endpointSpecifier.Value.Substring(1);
            if (!context.InputMultiset.ContainsVariable(var)) throw new RdfQueryException("Cannot evaluate a SERVICE clause which uses a Variable as the Service specifier when the Variable is unbound");

            IEnumerable<SparqlQueryClient> serviceEndpoints = context.InputMultiset.Sets
                .Select(set => set[var])
                .OfType<IUriNode>()
                .Distinct()
                .Select(u => new SparqlQueryClient(context.GetHttpClient(u.Uri), u.Uri));

            return new FederatedSparqlQueryClient(serviceEndpoints);
        }

        throw new RdfQueryException("SERVICE Specifier must be a URI/Variable Token but a " + endpointSpecifier.GetType() + " Token was provided");
    }

    /// <summary>
    /// Processes a Slice modifier.
    /// </summary>
    /// <param name="slice">Slice modifier.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessSlice(Slice slice, SparqlEvaluationContext context)
    {
        if (context == null) context = GetContext();
        // Detect the Offset and Limit from the Query if required
        int limit = slice.Limit, offset = slice.Offset;
        if (slice.DetectFromQuery)
        {
            if (context.Query != null)
            {
                limit = Math.Max(-1, context.Query.Limit);
                offset = Math.Max(0, context.Query.Offset);
            }
        }

        // First check what variables are present, we need this if we have to create
        // a new multiset
        IEnumerable<string> vars;
        if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
        {
            vars = (context.Query != null) ? context.Query.Variables.Where(v => v.IsResultVariable).Select(v => v.Name) : context.InputMultiset.Variables;
        }
        else
        {
            vars = context.InputMultiset.Variables;
        }

        if (limit == 0)
        {
            // If Limit is Zero we can skip evaluation
            context.OutputMultiset = new Multiset(vars);
            return context.OutputMultiset;
        }
        else
        {
            // Otherwise we have a limit/offset to apply

            // Firstly evaluate the inner algebra
            context.InputMultiset = slice.InnerAlgebra.Accept(this, context);
            context.InputMultiset.VirtualCount = context.InputMultiset.Count;
            // Then apply the offset
            if (offset > 0)
            {
                if (offset > context.InputMultiset.Count)
                {
                    // If the Offset is greater than the count return nothing
                    context.OutputMultiset = new Multiset(vars);
                    return context.OutputMultiset;
                }
                else
                {
                    // Otherwise discard the relevant number of Bindings
                    foreach (var id in context.InputMultiset.SetIDs.Take(offset).ToList())
                    {
                        context.InputMultiset.Remove(id);
                    }
                }
            }
            // Finally apply the limit
            if (limit > 0)
            {
                if (context.InputMultiset.Count > limit)
                {
                    // If the number of results is greater than the limit remove the extraneous
                    // results
                    foreach (var id in context.InputMultiset.SetIDs.Skip(limit).ToList())
                    {
                        context.InputMultiset.Remove(id);
                    }
                }
            }

            // Then return the result
            context.OutputMultiset = context.InputMultiset;
            return context.OutputMultiset;
        }
    }

    /// <summary>
    /// Processes a Subquery.
    /// </summary>
    /// <param name="subquery">Subquery.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    /// <returns></returns>
    public virtual BaseMultiset ProcessSubQuery(SubQuery subquery, SparqlEvaluationContext context)
    {
        if (context == null) context = GetContext();
        // Use the same algebra optimisers as the parent query (if any)
        if (context.Query != null)
        {
            subquery.Query.AlgebraOptimisers = context.Query.AlgebraOptimisers;
        }

        if (context.InputMultiset is NullMultiset)
        {
            context.OutputMultiset = context.InputMultiset;
        }
        else if (context.InputMultiset.IsEmpty)
        {
            context.OutputMultiset = new NullMultiset();
        }
        else
        {
            var subcontext = new SparqlEvaluationContext(subquery.Query, context.Data, context.Processor, context.Options);

            // Add any Named Graphs to the subquery
            if (context.Query != null)
            {
                foreach (IRefNode graphName in context.Query.NamedGraphNames)
                {
                    subquery.Query.AddNamedGraph(graphName);
                }
            }

            ISparqlAlgebra query = subquery.Query.ToAlgebra(_options.AlgebraOptimisation, _options.AlgebraOptimisers);
            try
            {
                // Evaluate the Subquery
                context.OutputMultiset = subcontext.Evaluate(query);

                // If the Subquery contains a GROUP BY it may return a Group Multiset in which case we must flatten this to a Multiset
                if (context.OutputMultiset is GroupMultiset groupMultiset)
                {
                    context.OutputMultiset = new Multiset(groupMultiset);
                }

                // Strip out any Named Graphs from the subquery
                if (subquery.Query.NamedGraphNames.Any())
                {
                    subquery.Query.ClearNamedGraphs();
                }
            }
            catch (RdfQueryException queryEx)
            {
                throw new RdfQueryException("Query failed due to a failure in sub-query execution:\n" + queryEx.Message, queryEx);
            }
        }

        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Union.
    /// </summary>
    /// <param name="union">Union.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    public virtual BaseMultiset ProcessUnion(IUnion union, SparqlEvaluationContext context)
    {
        if (context == null) context = GetContext();
        return union switch
        {
            LazyUnion lazyUnion => EvaluateLazyUnion(lazyUnion, context),
            ParallelUnion parallelUnion => EvaluateParallelUnion(parallelUnion, context),
            _ => EvaluateDefaultUnion(union, context)
        };
    }

    private BaseMultiset EvaluateLazyUnion(LazyUnion union, SparqlEvaluationContext context)
    {
        BaseMultiset initialInput = context.InputMultiset;
        if (union.Lhs is Extend || union.Rhs is Extend) initialInput = new IdentityMultiset();

        context.InputMultiset = initialInput;
        BaseMultiset lhsResult = union.Lhs.Accept(this, context);
        context.CheckTimeout();

        if (lhsResult.Count >= union.RequiredResults || union.RequiredResults == -1)
        {
            // Only evaluate the RHS if the LHS didn't yield sufficient results
            context.InputMultiset = initialInput;
            BaseMultiset rhsResult = union.Rhs.Accept(this, context);
            context.CheckTimeout();

            context.OutputMultiset = lhsResult.Union(rhsResult);
            context.CheckTimeout();

            context.InputMultiset = context.OutputMultiset;
        }
        else
        {
            context.OutputMultiset = lhsResult;
        }
        return context.OutputMultiset;
    }

    private BaseMultiset EvaluateDefaultUnion(IUnion union, SparqlEvaluationContext context)
    {
        BaseMultiset initialInput = context.InputMultiset;
        if (union.Lhs is Extend || union.Rhs is Extend) initialInput = new IdentityMultiset();
        if (union is AskUnion)
        {
            context.InputMultiset = initialInput;
            BaseMultiset lhsResult = union.Lhs.Accept(this, context);
            context.CheckTimeout();

            if (lhsResult.IsEmpty)
            {
                // Only evaluate the RHS if the LHS was empty
                context.InputMultiset = initialInput;
                BaseMultiset rhsResult = union.Rhs.Accept(this, context);
                context.CheckTimeout();

                context.OutputMultiset = lhsResult.Union(rhsResult);
                context.CheckTimeout();

                context.InputMultiset = context.OutputMultiset;
            }
            else
            {
                context.OutputMultiset = lhsResult;
            }

            return context.OutputMultiset;
        }
        else
        {
            context.InputMultiset = initialInput;
            BaseMultiset lhsResult = union.Lhs.Accept(this, context);
            context.CheckTimeout();

            context.InputMultiset = initialInput;
            BaseMultiset rhsResult = union.Rhs.Accept(this, context);
            context.CheckTimeout();

            context.OutputMultiset = lhsResult.Union(rhsResult);
            context.CheckTimeout();

            context.InputMultiset = context.OutputMultiset;
            return context.OutputMultiset;
        }
    }

    /// <summary>
    /// Processes a Zero Length Path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    /// <returns></returns>
    public virtual BaseMultiset ProcessZeroLengthPath(ZeroLengthPath path, SparqlEvaluationContext context)
    {
        if (context == null) context = GetContext();
        if (path.AreBothTerms())
        {
            if (path.AreSameTerms())
            {
                return new IdentityMultiset();
            }

            return new NullMultiset();
        }

        var subjVars = path.PathStart.Variables.ToList();
        var objVars = path.PathEnd.Variables.ToList();
        context.OutputMultiset = new Multiset();

        // Determine the Triples to which this applies
        if (!path.PathStart.IsFixed)
        {
            // Subject is a Variable
            if (context.InputMultiset.ContainsVariables(subjVars))
            {
                // Subject is Bound
                if (!path.PathEnd.IsFixed)
                {
                    // Object is a Variable
                    if (context.InputMultiset.ContainsVariables(objVars))
                    {
                        // Both Subject and Object are Bound
                        foreach (ISet s in context.InputMultiset.Sets.Where(set =>
                                     set.BindsAll(subjVars) && set.BindsAll(objVars)))
                        {
                            INode subj = path.PathStart.Bind(s);
                            INode obj = path.PathEnd.Bind(s);
                            ISet x = new Set();
                            if (path.PathStart.Accepts(context, subj, x))
                            {
                                ISet y = new Set();
                                if (path.PathEnd.Accepts(context, obj, y))
                                {
                                    context.OutputMultiset.Add(x);
                                    context.OutputMultiset.Add(y);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Subject is bound but Object is Unbound
                        foreach (ISet s in context.InputMultiset.Sets.Where(set => set.BindsAll(subjVars)))
                        {
                            INode subj = path.PathStart.Bind(s);
                            ISet x = s.Copy();
                            if (path.PathStart.Accepts(context, subj, x))
                            {
                                objVars.ForEach(objVar => x.Add(objVar, subj));
                                context.OutputMultiset.Add(x);
                            }
                        }
                    }
                }
                else
                {
                    // Object is a Term
                    // Preserve sets where the Object Term is equal to the currently bound Subject
                    INode objTerm = path.PathEnd.Bind(new Set());
                    foreach (ISet s in context.InputMultiset.Sets)
                    {
                        INode temp = path.PathStart.Bind(s);
                        if (temp != null && temp.Equals(objTerm))
                        {
                            context.OutputMultiset.Add(s.Copy());
                        }
                    }
                }
            }
            else
            {
                // Subject is Unbound
                if (!path.PathEnd.IsFixed)
                {
                    // Object is a Variable
                    if (context.InputMultiset.ContainsVariables(objVars))
                    {
                        // Object is Bound but Subject is unbound
                        foreach (ISet s in context.InputMultiset.Sets.Where(set => set.BindsAll(objVars)))
                        {
                            INode obj = path.PathEnd.Bind(s);
                            ISet x = s.Copy();
                            if (path.PathEnd.Accepts(context, obj, x))
                            {
                                subjVars.ForEach(subjVar => x.Add(subjVar, obj));
                                context.OutputMultiset.Add(x);
                            }
                        }
                    }
                    else
                    {
                        // Subject and Object are Unbound
                        var nodes = new HashSet<INode>();
                        foreach (Triple t in context.Data.Triples)
                        {
                            nodes.Add(t.Subject);
                            nodes.Add(t.Object);
                        }
                        foreach (INode n in nodes)
                        {
                            if ((path.PathStart is QuotedTriplePattern || path.PathEnd is QuotedTriplePattern) && n is not ITripleNode) continue;
                            var s = new Set();
                            path.PathStart.AddBindings(n, s);
                            path.PathEnd.AddBindings(n, s);
                            context.OutputMultiset.Add(s);
                        }
                    }
                }
                else
                {
                    // Object is a Term
                    // Create a single set with the Variable bound to the Object Term
                    var s = new Set();
                    INode objNode = path.PathEnd.Bind(new Set());
                    path.PathStart.AddBindings(objNode, s);
                    context.OutputMultiset.Add(s);
                }
            }
        }
        else if (!path.PathEnd.IsFixed)
        {
            // Subject is a Term but Object is a Variable
            INode subjTerm = path.PathStart.Bind(new Set());
            if (context.InputMultiset.ContainsVariables(objVars))
            {
                // Object is Bound
                // Preserve sets where the Subject Term is equal to the currently bound Object
                foreach (ISet s in context.InputMultiset.Sets)
                {
                    INode temp = path.PathEnd.Bind(s);
                    if (temp != null && temp.Equals(subjTerm))
                    {
                        context.OutputMultiset.Add(s.Copy());
                    }
                }
            }
            else
            {
                // Object is Unbound
                // Create a single set with the Variable bound to the Subject Term
                var s = new Set();
                path.PathEnd.AddBindings(subjTerm, s);
                context.OutputMultiset.Add(s);
            }
        }
        else
        {
            // Should already have dealt with this earlier (the AreBothTerms() and AreSameTerms() branch)
            throw new RdfQueryException("Reached unexpected point of ZeroLengthPath evaluation");
        }

        return context.OutputMultiset;
    }

    /// <summary>
    /// Processes a Zero or More Path.
    /// </summary>
    /// <param name="zeroOrMorePath">Path.</param>
    /// <param name="context">SPARQL Evaluation Context.</param>
    /// <returns></returns>
    public virtual BaseMultiset ProcessZeroOrMorePath(ZeroOrMorePath zeroOrMorePath, SparqlEvaluationContext context)
    {
        context ??= GetContext();
        var paths = new List<List<INode>>();
        BaseMultiset initialInput = context.InputMultiset;
        int step = 0, prevCount = 0, skipCount = 0;
        var subjVars = zeroOrMorePath.PathStart.Variables.ToList();
        var objVars = zeroOrMorePath.PathEnd.Variables.ToList();
        var bothTerms = zeroOrMorePath.PathStart.IsFixed && zeroOrMorePath.PathEnd.IsFixed;
        var reverse = false;

        if (zeroOrMorePath.PathStart.IsFixed || context.InputMultiset.ContainsVariables(subjVars))
        {
            // Work Forwards from the Starting Term or Bound Variable
            // OR if there is no Ending Term or Bound Variable work forwards regardless
            if (zeroOrMorePath.PathStart.IsFixed)
            {
                paths.Add(zeroOrMorePath.PathStart.Bind(new Set()).AsEnumerable().ToList());
            }
            else
            {
                paths.AddRange((from s in context.InputMultiset.Sets
                                where s.BindsAll(subjVars)
                                select zeroOrMorePath.PathStart.Bind(s)).Distinct().Select(n => n.AsEnumerable().ToList()));
            }
        }
        else if (zeroOrMorePath.PathEnd.IsFixed || (context.InputMultiset.ContainsVariables(objVars)))
        {
            // Work Backwards from Ending Term or Bound Variable
            if (zeroOrMorePath.PathEnd.IsFixed)
            {
                paths.Add(zeroOrMorePath.PathEnd.Bind(new Set()).AsEnumerable().ToList());
            }
            else
            {
                paths.AddRange((from s in context.InputMultiset.Sets
                                where s.BindsAll(objVars)
                                select zeroOrMorePath.PathEnd.Bind(s)).Distinct().Select(n => n.AsEnumerable().ToList()));
            }
            reverse = true;
        }

        if (paths.Count == 0)
        {
            GetPathStarts(zeroOrMorePath, context, paths, reverse);
        }

        // Traverse the Paths
        do
        {
            prevCount = paths.Count;
            foreach (List<INode> path in paths.Skip(skipCount).ToList())
            {
                foreach (INode nextStep in EvaluateStep(zeroOrMorePath, context, path, reverse))
                {
                    var newPath = new List<INode>(path) { nextStep };
                    paths.Add(newPath);
                }
            }

            // Update Counts
            // skipCount is used to indicate the paths which we will ignore for the purposes of
            // trying to further extend since we've already done them once
            step++;
            if (paths.Count == 0) break;
            skipCount = prevCount;

            // Can short circuit evaluation here if both are terms and any path is acceptable
            if (bothTerms)
            {
                var exit = false;
                foreach (List<INode> path in paths)
                {
                    var s = new Set();
                    if (reverse)
                    {
                        if (zeroOrMorePath.PathEnd.Accepts(context, path[0], s) && zeroOrMorePath.PathStart.Accepts(context, path[path.Count - 1], s)) // - Should these be separate sets?
                        {
                            exit = true;
                            break;
                        }
                    }
                    else
                    {
                        if (zeroOrMorePath.PathStart.Accepts(context, path[0], s) && zeroOrMorePath.PathEnd.Accepts(context, path[path.Count - 1], s))
                        {
                            exit = true;
                            break;
                        }
                    }
                }
                if (exit) break;
            }
        } while (paths.Count > prevCount || (step == 1 && paths.Count == prevCount));

        if (paths.Count == 0)
        {
            // If all path starts lead nowhere then we get the Null Multiset as a result
            context.OutputMultiset = new NullMultiset();
        }
        else
        {
            context.OutputMultiset = new Multiset();

            // Evaluate the Paths to check that are acceptable
            var returnedPaths = new HashSet<ISet>();
            foreach (List<INode> path in paths)
            {
                if (reverse)
                {
                    var s = new Set();
                    if (zeroOrMorePath.PathEnd.Accepts(context, path[0], s) && zeroOrMorePath.PathStart.Accepts(context, path[path.Count - 1], s))
                    {
                        // Make sure to check for uniqueness
                        if (returnedPaths.Contains(s)) continue;
                        context.OutputMultiset.Add(s);
                        returnedPaths.Add(s);

                        // If both are terms can short circuit evaluation here
                        // It is sufficient just to determine that there is one path possible
                        if (bothTerms) break;
                    }
                }
                else
                {
                    var s = new Set();
                    if (zeroOrMorePath.PathStart.Accepts(context, path[0], s) && zeroOrMorePath.PathEnd.Accepts(context, path[path.Count - 1], s))
                    {
                        // Make sure to check for uniqueness
                        if (returnedPaths.Contains(s)) continue;
                        context.OutputMultiset.Add(s);
                        returnedPaths.Add(s);

                        // If both are terms can short circuit evaluation here
                        // It is sufficient just to determine that there is one path possible
                        if (bothTerms) break;
                    }
                }
            }

            if (bothTerms)
            {
                // If both were terms transform to an Identity/Null Multiset as appropriate
                if (context.OutputMultiset.IsEmpty)
                {
                    context.OutputMultiset = new NullMultiset();
                }
                else
                {
                    context.OutputMultiset = new IdentityMultiset();
                }
            }

            // Then union in the zero length paths
            context.InputMultiset = initialInput;
            var zeroPath = new ZeroLengthPath(zeroOrMorePath.PathStart, zeroOrMorePath.PathEnd, zeroOrMorePath.Path);
            BaseMultiset currResults = context.OutputMultiset;
            context.OutputMultiset = new Multiset();
            BaseMultiset results = context.Evaluate(zeroPath);
            context.OutputMultiset = currResults;
            context.OutputMultiset.Merge(results);
        }

        context.InputMultiset = initialInput;
        return context.OutputMultiset;
    }

    /// <inheritdoc />
    public BaseMultiset ProcessBoundFilter(BoundFilter filter, SparqlEvaluationContext context)
    {
        if (context.InputMultiset is NullMultiset) return context.InputMultiset;
        if (context.InputMultiset is IdentityMultiset)
        {
            // If the Input is the Identity Multiset then nothing is Bound so return the Null Multiset
            context.InputMultiset = new NullMultiset();
            return context.InputMultiset;
        }

        // BOUND is always safe to parallelise
        if (context.Options.UsePLinqEvaluation)
        {
            context.InputMultiset.SetIDs.ToList().AsParallel().ForAll(i => EvalBoundFilter(filter, context, i));
        }
        else
        {
            foreach (var id in context.InputMultiset.SetIDs.ToList())
            {
                EvalBoundFilter(filter, context, id);
            }
        }
        return context.InputMultiset;
    }

    private void EvalBoundFilter(ISparqlFilter filter, SparqlEvaluationContext context, int id)
    {
        try
        {
            if (filter.Expression.Accept(_expressionProcessor, context, id) == null)
            {
                context.InputMultiset.Remove(id);
            }
        }
        catch
        {
            context.InputMultiset.Remove(id);
        }
    }

    /// <inheritdoc />
    public BaseMultiset ProcessUnaryExpressionFilter(UnaryExpressionFilter filter, SparqlEvaluationContext context)
    {
        switch (context.InputMultiset)
        {
            case NullMultiset _:
                return context.InputMultiset;
            case IdentityMultiset _ when !filter.Variables.Any():
                // If the Filter has no variables and is applied to an Identity Multiset then if the
                // Filter Expression evaluates to False then the Null Multiset is returned
                try
                {
                    if (!filter.Expression.Accept(_expressionProcessor, context, 0).AsSafeBoolean())
                    {
                        context.InputMultiset = new NullMultiset();
                    }
                }
                catch
                {
                    // Error is treated as false for Filters so Null Multiset is returned
                    context.InputMultiset = new NullMultiset();
                }

                break;
            case IdentityMultiset _:
                // As no variables are in scope the effect is that the Null Multiset is returned
                context.InputMultiset = new NullMultiset();
                break;
            default:
                // Remember that not all expressions are safe to parallelise
                if (context.Options.UsePLinqEvaluation && filter.Expression.CanParallelise)
                {
                    context.InputMultiset.SetIDs.ToList().AsParallel()
                        .ForAll(i => EvalUnaryExpressionFilter(filter, context, i));
                }
                else
                {
                    foreach (var id in context.InputMultiset.SetIDs.ToList())
                    {
                        EvalUnaryExpressionFilter(filter, context, id);
                    }
                }

                break;
        }

        return context.InputMultiset;
    }

    private void EvalUnaryExpressionFilter(ISparqlFilter filter, SparqlEvaluationContext context, int id)
    {
        try
        {
            if (!filter.Expression.Accept(_expressionProcessor, context, id).AsSafeBoolean())
            {
                context.InputMultiset.Remove(id);
            }
        }
        catch
        {
            context.InputMultiset.Remove(id);
        }
    }

    /// <inheritdoc />
    public BaseMultiset ProcessChainFilter(ChainFilter filter, SparqlEvaluationContext context)
    {
        if (context.InputMultiset is NullMultiset) return context.InputMultiset;

        foreach (ISparqlFilter subFilter in filter.Filters)
        {
            subFilter.Accept(this, context);
        }

        return context.InputMultiset;
    }

    /// <inheritdoc />
    public BaseMultiset ProcessSingleValueRestrictionFilter(SingleValueRestrictionFilter filter, SparqlEvaluationContext context)
    {
        INode term = filter.RestrictionValue.Accept(_expressionProcessor, context, 0);

        // First take appropriate pre-filtering actions
        if (context.InputMultiset is IdentityMultiset)
        {
            // If the Input is Identity switch the input to be a Multiset containing a single Set
            // where the variable is bound to the term
            context.InputMultiset = new Multiset();
            var s = new Set();
            s.Add(filter.RestrictionVariable, term);
            context.InputMultiset.Add(s);
        }
        else if (context.InputMultiset is NullMultiset)
        {
            // If Input is Null then output is Null
            context.OutputMultiset = context.InputMultiset;
            return context.OutputMultiset;
        }
        else
        {
            if (context.InputMultiset.ContainsVariable(filter.RestrictionVariable))
            {
                // If the Input Multiset contains the variable then pre-filter
                foreach (var id in context.InputMultiset.SetIDs.ToList())
                {
                    ISet x = context.InputMultiset[id];
                    try
                    {
                        if (x.ContainsVariable(filter.RestrictionVariable))
                        {
                            // If does exist check it has appropriate value and if not remove it
                            if (!term.Equals(x[filter.RestrictionVariable])) context.InputMultiset.Remove(id);
                        }
                        else
                        {
                            // If doesn't exist for this set then bind it to the term
                            x.Add(filter.RestrictionVariable, term);
                        }
                    }
                    catch (RdfQueryException)
                    {
                        context.InputMultiset.Remove(id);
                    }
                }
            }
            else
            {
                // If it doesn't contain the variable then bind for each existing set
                foreach (ISet x in context.InputMultiset.Sets)
                {
                    x.Add(filter.RestrictionVariable, term);
                }
            }
        }

        // Then evaluate the inner algebra
        BaseMultiset results = context.Evaluate(filter.InnerAlgebra);
        if (results is NullMultiset || results is IdentityMultiset) return results;

        // Filter the results to ensure that the variable is indeed bound to the term
        foreach (var id in results.SetIDs.ToList())
        {
            ISet x = results[id];
            try
            {
                if (!term.Equals(x[filter.RestrictionVariable]))
                {
                    results.Remove(id);
                }
            }
            catch (RdfQueryException)
            {
                results.Remove(id);
            }
        }

        if (results.Count > 0)
        {
            context.OutputMultiset = results;
        }
        else
        {
            context.OutputMultiset = new NullMultiset();
        }
        return context.OutputMultiset;
    }

    /// <inheritdoc />
    public BaseMultiset ProcessBindPattern(BindPattern bindPattern, SparqlEvaluationContext context)
    {
        if (context.InputMultiset is NullMultiset)
        {
            context.OutputMultiset = context.InputMultiset;
        }
        else if (context.InputMultiset is IdentityMultiset)
        {
            context.OutputMultiset.AddVariable(bindPattern.Variable);
            var s = new Set();
            try
            {
                INode temp = bindPattern.InnerExpression.Accept(_expressionProcessor, context, 0);
                s.Add(bindPattern.Variable, temp);
            }
            catch
            {
                // No assignment if there's an error
                s.Add(bindPattern.Variable, null);
            }
            context.OutputMultiset.Add(s);
        }
        else
        {
            if (context.InputMultiset.ContainsVariable(bindPattern.Variable))
            {
                throw new RdfQueryException("Cannot use a BIND assigment to BIND to a variable that has previously been used in the Query");
            }

            context.OutputMultiset.AddVariable(bindPattern.Variable);
            foreach (var id in context.InputMultiset.SetIDs.ToList())
            {
                ISet s = context.InputMultiset[id].Copy();
                try
                {
                    // Make a new assignment
                    INode temp = bindPattern.InnerExpression.Accept(_expressionProcessor, context, id);
                    s.Add(bindPattern.Variable, temp);
                }
                catch
                {
                    // No assignment if there's an error but the solution is preserved
                }
                context.OutputMultiset.Add(s);
            }
        }

        return context.OutputMultiset;
    }

    /// <inheritdoc />
    public BaseMultiset ProcessFilterPattern(FilterPattern filterPattern, SparqlEvaluationContext context)
    {
        if (context.InputMultiset is NullMultiset)
        {
            // If we get a NullMultiset then the FILTER has no effect since there are already no results
        }
        else if (context.InputMultiset is IdentityMultiset)
        {
            if (!filterPattern.Filter.Variables.Any())
            {
                // If we get an IdentityMultiset then the FILTER only has an effect if there are no
                // variables - otherwise it is not in scope and is ignored

                try
                {
                    if (!filterPattern.Filter.Expression.Accept(_expressionProcessor, context, 0).AsSafeBoolean())
                    {
                        context.OutputMultiset = new NullMultiset();
                        return context.OutputMultiset;
                    }
                }
                catch
                {
                    context.OutputMultiset = new NullMultiset();
                    return context.OutputMultiset;
                }
            }
        }
        else
        {
            filterPattern.Filter.Accept(this, context);
        }
        context.OutputMultiset = new IdentityMultiset();
        return context.OutputMultiset;
    }

    /// <inheritdoc />
    public BaseMultiset ProcessLetPattern(LetPattern letPattern, SparqlEvaluationContext context)
    {
        if (context.InputMultiset is NullMultiset)
        {
            context.OutputMultiset = context.InputMultiset;
        }
        else if (context.InputMultiset is IdentityMultiset)
        {
            var s = new Set();
            try
            {
                INode temp = letPattern.AssignExpression.Accept(_expressionProcessor, context, 0);
                s.Add(letPattern.VariableName, temp);
                context.OutputMultiset.Add(s);
            }
            catch
            {
                // No assignment if there's an error
            }
        }
        else
        {
            foreach (var id in context.InputMultiset.SetIDs.ToList())
            {
                ISet s = context.InputMultiset[id];
                if (s.ContainsVariable(letPattern.VariableName))
                {
                    try
                    {
                        // A value already exists so see if the two values match
                        INode current = s[letPattern.VariableName];
                        INode temp = letPattern.AssignExpression.Accept(_expressionProcessor, context, id);
                        if (!current.Equals(temp))
                        {
                            // Where the values aren't equal the solution is eliminated
                            context.InputMultiset.Remove(id);
                        }
                    }
                    catch
                    {
                        // If an error occurs the solution is eliminated
                        context.InputMultiset.Remove(id);
                    }
                }
                else
                {
                    context.InputMultiset.AddVariable(letPattern.VariableName);
                    try
                    {
                        // Make a new assignment
                        INode temp = letPattern.AssignExpression.Accept(_expressionProcessor, context, id);
                        s.Add(letPattern.VariableName, temp);
                    }
                    catch
                    {
                        // If an error occurs no assignment happens
                    }
                }
            }
            context.OutputMultiset = new IdentityMultiset();
        }

        return context.OutputMultiset;
    }

    /// <inheritdoc />
    public BaseMultiset ProcessPropertyFunction(PropertyFunction propertyFunction, SparqlEvaluationContext context)
    {
        context.InputMultiset = propertyFunction.InnerAlgebra.Accept(this, context);
        if (propertyFunction.Function is ILeviathanPropertyFunction leviathanPropertyFunction)
        {
            return leviathanPropertyFunction.Evaluate(context);
        }

        throw new RdfQueryException("Unable to process property function " + propertyFunction +
                                    " as no implementation compatible with the Leviathan engine could be found.");
    }

    /// <inheritdoc />
    public BaseMultiset ProcessPropertyPathPattern(PropertyPathPattern propertyPathPattern, SparqlEvaluationContext context)
    {
        // Try and generate an Algebra expression
        // Make sure we don't generate clashing temporary variable IDs over the life of the
        // Evaluation
        var transformContext = new PathTransformContext(propertyPathPattern.Subject, propertyPathPattern.Object);
        if (context["PathTransformID"] != null)
        {
            transformContext.NextID = (int)context["PathTransformID"];
        }
        ISparqlAlgebra algebra = propertyPathPattern.Path.ToAlgebra(transformContext);
        context["PathTransformID"] = transformContext.NextID + 1;

        // Now we can evaluate the resulting algebra
        BaseMultiset initialInput = context.InputMultiset;
        var trimMode = context.TrimTemporaryVariables;
        var rigMode = context.Options.RigorousEvaluation;
        try
        {
            // Must enable rigorous evaluation or we get incorrect interactions between property and non-property path patterns
            context.Options.RigorousEvaluation = true;

            // Note: We may need to preserve Blank Node variables across evaluations
            // which we usually don't do BUT because of the way we translate only part of the path
            // into an algebra at a time and may need to do further nested translate calls we do
            // need to do this here
            context.TrimTemporaryVariables = false;
            BaseMultiset result = context.Evaluate(algebra);//algebra.Evaluate(context);
            // Also note that we don't trim temporary variables here even if we've set the setting back
            // to enabled since a Trim will be done at the end of whatever BGP we are being evaluated in

            // Once we have our results can join then into our input
            context.OutputMultiset = result is NullMultiset ? new NullMultiset() : initialInput.Join(result);

            // If we reach here we've successfully evaluated the simple pattern and can return
            return context.OutputMultiset;
        }
        finally
        {
            context.TrimTemporaryVariables = trimMode;
            context.Options.RigorousEvaluation = rigMode;
        }
    }

    /// <inheritdoc />
    /// <inheritdoc />
    /// <inheritdoc />
    /// <inheritdoc />
    public BaseMultiset ProcessSubQueryPattern(SubQueryPattern subQueryPattern, SparqlEvaluationContext context)
    {
        // Use the same algebra optimisers as the parent query (if any)
        if (context.Query != null)
        {
            subQueryPattern.SubQuery.AlgebraOptimisers = context.Query.AlgebraOptimisers;
        }

        if (context.InputMultiset is NullMultiset)
        {
            context.OutputMultiset = context.InputMultiset;
        }
        else if (context.InputMultiset.IsEmpty)
        {
            context.OutputMultiset = new NullMultiset();
        }
        else
        {
            var subcontext = new SparqlEvaluationContext(subQueryPattern.SubQuery, context.Data, context.Processor,
                context.Options) {InputMultiset = context.InputMultiset};

            // Add any Named Graphs to the subquery
            if (context.Query != null)
            {
                foreach (IRefNode u in context.Query.NamedGraphNames)
                {
                    subQueryPattern.SubQuery.AddNamedGraph(u);
                }
            }

            ISparqlAlgebra query = subQueryPattern.SubQuery.ToAlgebra(_options.AlgebraOptimisation, _options.AlgebraOptimisers);
            try
            {
                // Evaluate the Subquery
                context.OutputMultiset = subcontext.Evaluate(query);

                // If the Subquery contains a GROUP BY it may return a Group Multiset in which case we must flatten this to a Multiset
                if (context.OutputMultiset is GroupMultiset groupMultiset)
                {
                    context.OutputMultiset = new Multiset(groupMultiset);
                }

                // Strip out any Named Graphs from the subquery
                if (subQueryPattern.SubQuery.NamedGraphNames.Any())
                {
                    subQueryPattern.SubQuery.ClearNamedGraphs();
                }
            }
            catch (RdfQueryException queryEx)
            {
                throw new RdfQueryException("Query failed due to a failure in Subquery Execution:\n" + queryEx.Message, queryEx);
            }
        }

        return context.OutputMultiset;
    }

    /// <inheritdoc />
    public BaseMultiset ProcessPropertyFunctionPattern(PropertyFunctionPattern propFunctionPattern,
        SparqlEvaluationContext context)
    {
        if (propFunctionPattern.PropertyFunction is ILeviathanPropertyFunction leviathanPropertyFunction)
        {
            return leviathanPropertyFunction.Evaluate(context);
        }

        throw new RdfQueryException("Unable to process property function " + propFunctionPattern.PropertyFunction +
                                    " as no implementation compatible with the Leviathan engine could be found.");

    }

    /// <inheritdoc />
    public BaseMultiset ProcessTriplePattern(TriplePattern triplePattern, SparqlEvaluationContext context)
    {
        context.Evaluate(triplePattern);
        return context.OutputMultiset;
    }

    /// <inheritdoc />
    public virtual BaseMultiset ProcessUnknownOperator(ISparqlAlgebra op, SparqlEvaluationContext context)
    {
        switch (op)
        {
            case FilteredProduct fp:
                return EvaluateFilteredProduct(fp, context);
            case Table table:
                context.OutputMultiset = table.Multiset;
                return context.OutputMultiset;
            case ILeviathanAlgebraExtension ext:
                return ext.Evaluate(context);
            default:
                throw new RdfQueryException("Unable to process unrecognized algebra type " + op.GetType());
        }
    }

    #endregion

    #region AskBgp Evaluation
    private BaseMultiset EvaluateAskBgp(IList<ITriplePattern> triplePatterns, SparqlEvaluationContext context, int pattern, out bool halt)
    {
        halt = false;

        // Handle Empty BGPs
        if (pattern == 0 && !triplePatterns.Any())
        {
            context.OutputMultiset = new IdentityMultiset();
            return context.OutputMultiset;
        }

        BaseMultiset initialInput, localOutput, results;

        // Set up the Input and Output Multiset appropriately
        switch (pattern)
        {
            case 0:
                // Input is as given and Output is new empty multiset
                initialInput = context.InputMultiset;
                localOutput = new Multiset();
                break;

            case 1:
                // Input becomes current Output and Output is new empty multiset
                initialInput = context.OutputMultiset;
                localOutput = new Multiset();
                break;

            default:
                // Input is join of previous input and ouput and Output is new empty multiset
                if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                {
                    // Disjoint so do a Product
                    initialInput = context.InputMultiset.Product(context.OutputMultiset);
                }
                else
                {
                    // Normal Join
                    initialInput = context.InputMultiset.Join(context.OutputMultiset);
                }
                localOutput = new Multiset();
                break;
        }
        context.InputMultiset = initialInput;
        context.OutputMultiset = localOutput;

        // Get the Triple Pattern we're evaluating
        ITriplePattern temp = triplePatterns[pattern];
        var resultsFound = 0;

        if (temp.PatternType == TriplePatternType.Match)
        {
            // Find the first Triple which matches the Pattern
            var tp = (IMatchTriplePattern)temp;
            foreach (Triple t in context.GetTriples(tp))
            {
                // Remember to check for Timeout during lazy evaluation
                context.CheckTimeout();

                ISet result = tp.Evaluate(context, t);
                if (result != null)
                {
                    resultsFound++;
                    context.OutputMultiset.Add(result);

                    // Recurse unless we're the last pattern
                    if (pattern < triplePatterns.Count - 1)
                    {
                        results = EvaluateAskBgp(triplePatterns, context, pattern + 1, out halt);

                        // If recursion leads to a halt then we halt and return immediately
                        if (halt)
                        {
                            return results;
                        }

                        // Otherwise we need to keep going here
                        // So must reset our input and outputs before continuing
                        context.InputMultiset = initialInput;
                        context.OutputMultiset = new Multiset();
                        resultsFound--;
                    }
                    else
                    {
                        // If we're at the last pattern and we've found a match then we can halt
                        halt = true;

                        // Generate the final output and return it
                        if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                        {
                            // Disjoint so do a Product
                            context.OutputMultiset = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.QueryTimeout - context.QueryTime);
                        }
                        else
                        {
                            // Normal Join
                            context.OutputMultiset = context.InputMultiset.Join(context.OutputMultiset);
                        }
                        return context.OutputMultiset;
                    }
                }
            }
        }
        else if (temp.PatternType == TriplePatternType.Filter)
        {
            var fp = (IFilterPattern)temp;
            ISparqlFilter filter = fp.Filter;
            ISparqlExpression expr = filter.Expression;

            // Find the first result of those we've got so far that matches
            if (context.InputMultiset is IdentityMultiset || context.InputMultiset.IsEmpty)
            {
                try
                {
                    // If the Input is the Identity Multiset then the Output is either
                    // the Identity/Null Multiset depending on whether the Expression evaluates to true
                    if (expr.Accept(_expressionProcessor, context, 0).AsSafeBoolean())
                    {
                        context.OutputMultiset = new IdentityMultiset();
                    }
                    else
                    {
                        context.OutputMultiset = new NullMultiset();
                    }
                }
                catch
                {
                    // If Expression fails to evaluate then result is NullMultiset
                    context.OutputMultiset = new NullMultiset();
                }
            }
            else
            {
                foreach (var id in context.InputMultiset.SetIDs)
                {
                    // Remember to check for Timeout during lazy evaluation
                    context.CheckTimeout();

                    try
                    {
                        if (expr.Accept(_expressionProcessor, context, id).AsSafeBoolean())
                        {
                            resultsFound++;
                            context.OutputMultiset.Add(context.InputMultiset[id].Copy());

                            // Recurse unless we're the last pattern
                            if (pattern < triplePatterns.Count - 1)
                            {
                                results = StreamingEvaluate(triplePatterns, context, pattern + 1, out halt);

                                // If recursion leads to a halt then we halt and return immediately
                                if (halt) return results;

                                // Otherwise we need to keep going here
                                // So must reset our input and outputs before continuing
                                context.InputMultiset = initialInput;
                                context.OutputMultiset = new Multiset();
                                resultsFound--;
                            }
                            else
                            {
                                // If we're at the last pattern and we've found a match then we can halt
                                halt = true;

                                // Generate the final output and return it
                                if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                                {
                                    // Disjoint so do a Product
                                    context.OutputMultiset = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.RemainingTimeout);
                                }
                                else
                                {
                                    // Normal Join
                                    context.OutputMultiset = context.InputMultiset.Join(context.OutputMultiset);
                                }
                                return context.OutputMultiset;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore expression evaluation errors
                    }
                }
            }
        }

        // If we found no possibles we return the null multiset
        if (resultsFound == 0) return new NullMultiset();

        // We should never reach here so throw an error to that effect
        // The reason we'll never reach here is that this method should always return earlier
        throw new RdfQueryException("Unexpected control flow in evaluating a Streamed BGP for an ASK query");
    }

    private BaseMultiset StreamingEvaluate(IList<ITriplePattern> triplePatterns, SparqlEvaluationContext context, int pattern, out bool halt)
    {
        halt = false;

        // Handle Empty BGPs
        if (pattern == 0 && triplePatterns.Count == 0)
        {
            context.OutputMultiset = new IdentityMultiset();
            return context.OutputMultiset;
        }

        BaseMultiset initialInput, localOutput, results;

        // Set up the Input and Output Multiset appropriately
        switch (pattern)
        {
            case 0:
                // Input is as given and Output is new empty multiset
                initialInput = context.InputMultiset;
                localOutput = new Multiset();
                break;

            case 1:
                // Input becomes current Output and Output is new empty multiset
                initialInput = context.OutputMultiset;
                localOutput = new Multiset();
                break;

            default:
                // Input is join of previous input and ouput and Output is new empty multiset
                if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                {
                    // Disjoint so do a Product
                    initialInput = context.InputMultiset.Product(context.OutputMultiset);
                }
                else
                {
                    // Normal Join
                    initialInput = context.InputMultiset.Join(context.OutputMultiset);
                }
                localOutput = new Multiset();
                break;
        }
        context.InputMultiset = initialInput;
        context.OutputMultiset = localOutput;

        // Get the Triple Pattern we're evaluating
        ITriplePattern temp = triplePatterns[pattern];
        var resultsFound = 0;

        if (temp.PatternType == TriplePatternType.Match)
        {
            // Find the first Triple which matches the Pattern
            var tp = (IMatchTriplePattern)temp;
            foreach (Triple t in context.GetTriples(tp))
            {
                // Remember to check for Timeout during lazy evaluation
                context.CheckTimeout();

                ISet result = tp.Evaluate(context, t);
                if (result != null)
                {
                    resultsFound++;
                    context.OutputMultiset.Add(result);

                    // Recurse unless we're the last pattern
                    if (pattern < triplePatterns.Count - 1)
                    {
                        results = StreamingEvaluate(triplePatterns, context, pattern + 1, out halt);

                        // If recursion leads to a halt then we halt and return immediately
                        if (halt) return results;

                        // Otherwise we need to keep going here
                        // So must reset our input and outputs before continuing
                        context.InputMultiset = initialInput;
                        context.OutputMultiset = new Multiset();
                        resultsFound--;
                    }
                    else
                    {
                        // If we're at the last pattern and we've found a match then we can halt
                        halt = true;

                        // Generate the final output and return it
                        if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                        {
                            // Disjoint so do a Product
                            context.OutputMultiset = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.QueryTimeout - context.QueryTime);
                        }
                        else
                        {
                            // Normal Join
                            context.OutputMultiset = context.InputMultiset.Join(context.OutputMultiset);
                        }
                        return context.OutputMultiset;
                    }
                }
            }
        }
        else if (temp.PatternType == TriplePatternType.Filter)
        {
            var fp = (IFilterPattern)temp;
            ISparqlFilter filter = fp.Filter;
            ISparqlExpression expr = filter.Expression;

            // Find the first result of those we've got so far that matches
            if (context.InputMultiset is IdentityMultiset || context.InputMultiset.IsEmpty)
            {
                try
                {
                    // If the Input is the Identity Multiset then the Output is either
                    // the Identity/Null Multiset depending on whether the Expression evaluates to true
                    if (expr.Accept(_expressionProcessor, context, 0).AsSafeBoolean())
                    {
                        context.OutputMultiset = new IdentityMultiset();
                    }
                    else
                    {
                        context.OutputMultiset = new NullMultiset();
                    }
                }
                catch
                {
                    // If Expression fails to evaluate then result is NullMultiset
                    context.OutputMultiset = new NullMultiset();
                }
            }
            else
            {
                foreach (var id in context.InputMultiset.SetIDs)
                {
                    // Remember to check for Timeout during lazy evaluation
                    context.CheckTimeout();

                    try
                    {
                        if (expr.Accept(_expressionProcessor, context, id).AsSafeBoolean())
                        {
                            resultsFound++;
                            context.OutputMultiset.Add(context.InputMultiset[id].Copy());

                            // Recurse unless we're the last pattern
                            if (pattern < triplePatterns.Count - 1)
                            {
                                results = StreamingEvaluate(triplePatterns, context, pattern + 1, out halt);

                                // If recursion leads to a halt then we halt and return immediately
                                if (halt) return results;

                                // Otherwise we need to keep going here
                                // So must reset our input and outputs before continuing
                                context.InputMultiset = initialInput;
                                context.OutputMultiset = new Multiset();
                                resultsFound--;
                            }
                            else
                            {
                                // If we're at the last pattern and we've found a match then we can halt
                                halt = true;

                                // Generate the final output and return it
                                if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                                {
                                    // Disjoint so do a Product
                                    context.OutputMultiset = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.RemainingTimeout);
                                }
                                else
                                {
                                    // Normal Join
                                    context.OutputMultiset = context.InputMultiset.Join(context.OutputMultiset);
                                }
                                return context.OutputMultiset;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore expression evaluation errors
                    }
                }
            }
        }

        // If we found no possibles we return the null multiset
        if (resultsFound == 0) return new NullMultiset();

        // We should never reach here so throw an error to that effect
        // The reason we'll never reach here is that this method should always return earlier
        throw new RdfQueryException("Unexpected control flow in evaluating a Streamed BGP for an ASK query");
    }
    #endregion

    #region LazyBgp Evaluation
    /// <summary>
    /// Evaluates the BGP against the Evaluation Context.
    /// </summary>
    /// <param name="bgp">The BGP to be evaluated.</param>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    private BaseMultiset EvaluateLazyBgp(LazyBgp bgp, SparqlEvaluationContext context)
    {
        BaseMultiset results = null;
        var origRequired = bgp.RequiredResults;

        // May need to detect the actual amount of required results if not specified at instantiation
        if (bgp.RequiredResults < 0)
        {
            if (context.Query != null)
            {
                if (context.Query.HasDistinctModifier || (context.Query.OrderBy != null && !context.Query.IsOptimisableOrderBy) || context.Query.GroupBy != null || context.Query.Having != null || context.Query.Bindings != null)
                {
                    // If there's an DISTINCT/ORDER BY/GROUP BY/HAVING/BINDINGS present then can't do Lazy evaluation
                    bgp.RequiredResults = -1;
                }
                else
                {
                    var limit = context.Query.Limit;
                    var offset = context.Query.Offset;
                    if (limit >= 0 && offset >= 0)
                    {
                        // If there is a Limit and Offset specified then the required results is the LIMIT+OFFSET
                        bgp.RequiredResults = limit + offset;
                    }
                    else if (limit >= 0)
                    {
                        // If there is just a Limit specified then the required results is the LIMIT
                        bgp.RequiredResults = limit;
                    }
                    else
                    {
                        // In any other case required results is everything i.e. -1
                        bgp.RequiredResults = -1;
                    }
                }
                Debug.WriteLine("Lazy Evaluation - Number of required results is " + bgp.RequiredResults);
            }
        }

        switch (bgp.RequiredResults)
        {
            case -1:
                // If required results is everything just use normal evaluation as otherwise 
                // lazy evaluation will significantly impact performance and lead to an apparent infinite loop
                return EvaluateDefaultBgp(bgp, context);
            case 0:
                // Don't need any results
                results = new NullMultiset();
                break;
            default:
                // Do streaming evaluation
                if (bgp.RequiredResults != 0)
                {
                    results = StreamingEvaluate(bgp, context, 0, out var halt);
                    if (results is Multiset && results.IsEmpty) results = new NullMultiset();
                }
                break;
        }
        bgp.RequiredResults = origRequired;

        context.OutputMultiset = results;
        context.OutputMultiset?.Trim();
        return context.OutputMultiset;
    }

    private BaseMultiset StreamingEvaluate(LazyBgp bgp, SparqlEvaluationContext context, int pattern, out bool halt)
    {
        // Remember to check for Timeouts during Lazy Evaluation
        context.CheckTimeout();

        halt = false;

        // Handle Empty BGPs
        if (pattern == 0 && bgp.TriplePatterns.Count == 0)
        {
            context.OutputMultiset = new IdentityMultiset();
            return context.OutputMultiset;
        }

        BaseMultiset initialInput, localOutput, results = null;

        // Determine whether the Pattern modifies the existing Input rather than joining to it
        var modifies = (bgp.TriplePatterns[pattern].PatternType == TriplePatternType.Filter);
        var extended = (pattern > 0 && bgp.TriplePatterns[pattern - 1].PatternType == TriplePatternType.BindAssignment);
        var modified = (pattern > 0 && bgp.TriplePatterns[pattern - 1].PatternType == TriplePatternType.Filter);

        // Set up the Input and Output Multiset appropriately
        switch (pattern)
        {
            case 0:
                // Input is as given and Output is new empty multiset
                if (!modifies)
                {
                    initialInput = context.InputMultiset;
                }
                else
                {
                    // If the Pattern will modify the Input and is the first thing in the BGP then it actually modifies a new empty input
                    // This takes care of FILTERs being out of scope
                    initialInput = new Multiset();
                }
                localOutput = new Multiset();
                break;

            case 1:
                // Input becomes current Output and Output is new empty multiset
                initialInput = context.OutputMultiset;
                localOutput = new Multiset();
                break;

            default:
                if (!extended && !modified)
                {
                    // Input is join of previous input and output and Output is new empty multiset
                    if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                    {
                        // Disjoint so do a Product
                        initialInput = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.RemainingTimeout);
                    }
                    else
                    {
                        // Normal Join
                        initialInput = context.InputMultiset.Join(context.OutputMultiset);
                    }
                }
                else
                {
                    initialInput = context.OutputMultiset;
                }
                localOutput = new Multiset();
                break;
        }
        context.InputMultiset = initialInput;
        context.OutputMultiset = localOutput;

        // Get the Triple Pattern we're evaluating
        ITriplePattern temp = bgp.TriplePatterns[pattern];
        var resultsFound = 0;
        var prevResults = -1;

        if (temp.PatternType == TriplePatternType.Match)
        {
            // Find the first Triple which matches the Pattern
            var tp = (IMatchTriplePattern)temp;
            IEnumerable<Triple> ts = context.GetTriples(tp);

            // In the case that we're lazily evaluating an optimisable ORDER BY then
            // we need to apply OrderBy()'s to our enumeration
            // This only applies to the 1st pattern
            if (pattern == 0)
            {
                if (context.Query != null)
                {
                    if (context.Query.OrderBy != null && context.Query.IsOptimisableOrderBy)
                    {
                        IComparer<Triple> comparer = context.Query.OrderBy.GetComparer(tp, context.OrderingComparer);
                        if (comparer != null)
                        {
                            ts = ts.OrderBy(t => t, comparer);
                        }
                        else
                        {
                            // Can't get a comparer so can't optimise
                            // Thus required results is everything so just use normal evaluation as otherwise 
                            // lazy evaluation will significantly impact performance and lead to an apparent infinite loop
                            return EvaluateDefaultBgp(bgp, context);
                        }
                    }
                }
            }

            foreach (Triple t in ts)
            {
                ISet result = tp.Evaluate(context, t);
                if (result != null)
                {
                    resultsFound++;
                    if (tp.IndexType == TripleIndexType.NoVariables)
                    {
                        localOutput = new IdentityMultiset();
                        context.OutputMultiset = localOutput;
                    }
                    else
                    {
                        context.OutputMultiset.Add(result);
                    }
                }
            }
            // Recurse unless we're the last pattern
            if (pattern < bgp.TriplePatterns.Count - 1)
            {
                results = StreamingEvaluate(bgp, context, pattern + 1, out halt);

                // If recursion leads to a halt then we halt and return immediately
                if (halt && results.Count >= bgp.RequiredResults && bgp.RequiredResults != -1)
                {
                    return results;
                }
                else if (halt)
                {
                    if (results.Count == 0)
                    {
                        // If recursing leads to no results then eliminate all outputs
                        // Also reset to prevResults to -1
                        resultsFound = 0;
                        localOutput = new Multiset();
                        prevResults = -1;
                    }
                    else if (prevResults > -1)
                    {
                        if (results.Count == prevResults)
                        {
                            // If the amount of results found hasn't increased then this match does not
                            // generate any further solutions further down the recursion so we can eliminate
                            // this from the results
                            localOutput.Remove(localOutput.SetIDs.Max());
                        }
                    }
                    prevResults = results.Count;

                    // If we're supposed to halt but not reached the number of required results then continue
                    context.InputMultiset = initialInput;
                    context.OutputMultiset = localOutput;
                }
                else
                {
                    // Otherwise we need to keep going here
                    // So must reset our input and outputs before continuing
                    context.InputMultiset = initialInput;
                    context.OutputMultiset = new Multiset();
                    resultsFound--;
                }
            }
            else
            {
                // If we're at the last pattern and we've found a match then we can halt
                halt = true;

                // Generate the final output and return it
                if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                {
                    // Disjoint so do a Product
                    results = context.InputMultiset.ProductWithTimeout(context.OutputMultiset,
                        context.RemainingTimeout);
                }
                else
                {
                    // Normal Join
                    results = context.InputMultiset.Join(context.OutputMultiset);
                }

                // If not reached required number of results continue
                if (results.Count >= bgp.RequiredResults && bgp.RequiredResults != -1)
                {
                    context.OutputMultiset = results;
                    return context.OutputMultiset;
                }
            }
            context.InputMultiset = results;
        }
        else if (temp.PatternType == TriplePatternType.Filter)
        {
            var filter = (IFilterPattern)temp;
            ISparqlExpression filterExpr = filter.Filter.Expression;

            if (filter.Variables.IsDisjoint(context.InputMultiset.Variables))
            {
                // Filter is Disjoint so determine whether it has any affect or not
                if (filter.Variables.Any())
                {
                    // Has Variables but disjoint from input => not in scope so gets ignored

                    // Do we recurse or not?
                    if (pattern < bgp.TriplePatterns.Count - 1)
                    {
                        // Recurse and return
                        results = StreamingEvaluate(bgp, context, pattern + 1, out halt);
                        return results;
                    }
                    else
                    {
                        // We don't affect the input in any way so just return it
                        return context.InputMultiset;
                    }
                }
                else
                {
                    // No Variables so have to evaluate it to see if it gives true otherwise
                    try
                    {
                        if (filterExpr.Accept(_expressionProcessor, context, 0).AsSafeBoolean())
                        {
                            if (pattern < bgp.TriplePatterns.Count - 1)
                            {
                                // Recurse and return
                                results = StreamingEvaluate(bgp, context, pattern + 1, out halt);
                                return results;
                            }
                            else
                            {
                                // Last Pattern and we evaluate to true so can return the input as-is
                                halt = true;
                                return context.InputMultiset;
                            }
                        }
                    }
                    catch (RdfQueryException)
                    {
                        // Evaluates to false so eliminates all solutions (use an empty Multiset)
                        return new Multiset();
                    }
                }
            }
            else
            {
                // Test each solution found so far against the Filter and eliminate those that evalute to false/error
                foreach (var id in context.InputMultiset.SetIDs.ToList())
                {
                    try
                    {
                        if (filterExpr.Accept(_expressionProcessor, context, id).AsSafeBoolean())
                        {
                            // If evaluates to true then add to output
                            context.OutputMultiset.Add(context.InputMultiset[id].Copy());
                        }
                    }
                    catch (RdfQueryException)
                    {
                        // Error means we ignore the solution
                    }
                }

                // Remember to check for Timeouts during Lazy Evaluation
                context.CheckTimeout();

                // Decide whether to recurse or not
                resultsFound = context.OutputMultiset.Count;
                if (pattern < bgp.TriplePatterns.Count - 1)
                {
                    // Recurse then return
                    // We can never decide whether to recurse again at this point as we are not capable of deciding
                    // which solutions should be dumped (that is the job of an earlier pattern in the BGP)
                    results = StreamingEvaluate(bgp, context, pattern + 1, out halt);

                    return results;
                }
                else
                {
                    halt = true;

                    // However many results we need we'll halt - previous patterns can call us again if they find more potential solutions
                    // for us to filter
                    return context.OutputMultiset;
                }
            }
        }
        else if (temp is BindPattern)
        {
            var bind = (BindPattern)temp;
            ISparqlExpression bindExpr = bind.AssignExpression;
            var bindVar = bind.VariableName;

            if (context.InputMultiset.ContainsVariable(bindVar))
            {
                throw new RdfQueryException(
                    "Cannot use a BIND assigment to BIND to a variable that has previously been used in the Query");
            }
            else
            {
                // Compute the Binding for every value
                context.OutputMultiset.AddVariable(bindVar);
                foreach (ISet s in context.InputMultiset.Sets)
                {
                    ISet x = s.Copy();
                    try
                    {
                        INode val = bindExpr.Accept(_expressionProcessor, context, s.ID);
                        x.Add(bindVar, val);
                    }
                    catch (RdfQueryException)
                    {
                        // Equivalent to no assignment but the solution is preserved
                    }
                    context.OutputMultiset.Add(x.Copy());
                }

                // Remember to check for Timeouts during Lazy Evaluation
                context.CheckTimeout();

                // Decide whether to recurse or not
                resultsFound = context.OutputMultiset.Count;
                if (pattern < bgp.TriplePatterns.Count - 1)
                {
                    // Recurse then return
                    results = StreamingEvaluate(bgp, context, pattern + 1, out halt);
                    return results;
                }
                else
                {
                    halt = true;

                    // However many results we need we'll halt - previous patterns can call us again if they find more potential solutions
                    // for us to extend
                    return context.OutputMultiset;
                }
            }
        }
        else
        {
            throw new RdfQueryException("Encountered a " + temp.GetType().FullName +
                                        " which is not a lazily evaluable Pattern");
        }

        // If we found no possibles we return the null multiset
        if (resultsFound == 0)
        {
            return new NullMultiset();
        }
        else
        {
            // Remember to check for Timeouts during Lazy Evaluation
            context.CheckTimeout();

            // Generate the final output and return it
            if (!modifies)
            {
                if (context.InputMultiset.IsDisjointWith(context.OutputMultiset))
                {
                    // Disjoint so do a Product
                    results = context.InputMultiset.ProductWithTimeout(context.OutputMultiset, context.RemainingTimeout);
                }
                else
                {
                    // Normal Join
                    results = context.InputMultiset.Join(context.OutputMultiset);
                }
                context.OutputMultiset = results;
            }
            return context.OutputMultiset;
        }
    }
    #endregion

    #region FilteredProductEvaluation

    private BaseMultiset EvaluateFilteredProduct(FilteredProduct fp, SparqlEvaluationContext context)
    {
        BaseMultiset initialInput = context.InputMultiset;
        BaseMultiset lhsResults = fp.Lhs.Accept(this, context);

        if (lhsResults is NullMultiset || lhsResults.IsEmpty)
        {
            // If LHS Results are Null/Empty then end result will always be null so short circuit
            context.OutputMultiset = new NullMultiset();
        }
        else
        {

            context.InputMultiset = initialInput;
            BaseMultiset rhsResults = fp.Rhs.Accept(this, context);
            if (rhsResults is NullMultiset || rhsResults.IsEmpty)
            {
                // If RHS Results are Null/Empty then end results will always be null so short circuit
                context.OutputMultiset = new NullMultiset();
            }
            else if (rhsResults is IdentityMultiset)
            {
                // Apply Filter over LHS Results only - defer evaluation to filter implementation
                context.InputMultiset = lhsResults;
                var filter = new UnaryExpressionFilter(fp.FilterExpression);
                filter.Accept(this, context);
                context.OutputMultiset = lhsResults;
            }
            else
            {
                // Calculate the product applying the filter as we go
                if (context.Options.UsePLinqEvaluation && fp.FilterExpression.CanParallelise)
                {
                    PartitionedMultiset partitionedSet;
                    SparqlResultBinder binder = context.Binder;
                    if (lhsResults.Count >= rhsResults.Count)
                    {
                        partitionedSet = new PartitionedMultiset(lhsResults.Count, rhsResults.Count);
                        context.Binder = new LeviathanLeftJoinBinder(partitionedSet);
                        lhsResults.Sets.AsParallel().ForAll(x => EvalFilteredProduct(fp.FilterExpression, context, x, rhsResults, partitionedSet));
                    }
                    else
                    {
                        partitionedSet = new PartitionedMultiset(rhsResults.Count, lhsResults.Count);
                        context.Binder = new LeviathanLeftJoinBinder(partitionedSet);
                        rhsResults.Sets.AsParallel().ForAll(y => EvalFilteredProduct(fp.FilterExpression, context, y, lhsResults, partitionedSet));
                    }

                    context.Binder = binder;
                    context.OutputMultiset = partitionedSet;
                }
                else
                {
                    BaseMultiset productSet = new Multiset();
                    SparqlResultBinder binder = context.Binder;
                    context.Binder = new LeviathanLeftJoinBinder(productSet);
                    foreach (ISet x in lhsResults.Sets)
                    {
                        foreach (ISet y in rhsResults.Sets)
                        {
                            ISet z = x.Join(y);
                            productSet.Add(z);
                            try
                            {
                                if (!fp.FilterExpression.Accept(_expressionProcessor, context, z.ID).AsSafeBoolean())
                                {
                                    // Means the expression evaluates to false so we discard the solution
                                    productSet.Remove(z.ID);
                                }
                            }
                            catch
                            {
                                // Means this solution does not meet the FILTER and can be discarded
                                productSet.Remove(z.ID);
                            }
                        }
                        // Remember to check for timeouts occassionaly
                        context.CheckTimeout();
                    }
                    context.Binder = binder;
                    context.OutputMultiset = productSet;
                }
            }
        }
        return context.OutputMultiset;
    }

    private void EvalFilteredProduct(ISparqlExpression filterExpression, SparqlEvaluationContext context, ISet x, BaseMultiset other, PartitionedMultiset partitionedSet)
    {
        var id = partitionedSet.GetNextBaseID();
        foreach (ISet y in other.Sets)
        {
            id++;
            ISet z = x.Join(y);
            z.ID = id;
            partitionedSet.Add(z);
            try
            {
                if (!filterExpression.Accept(_expressionProcessor, context, z.ID).AsSafeBoolean())
                {
                    // Means the expression evaluates to false so we discard the solution
                    partitionedSet.Remove(z.ID);
                }
            }
            catch
            {
                // Means the solution does not meet the FILTER and can be discarded
                partitionedSet.Remove(z.ID);
            }
        }
        // Remember to check for timeouts occassionally
        context.CheckTimeout();
    }
    #endregion

    #region ParallelJoin Evaluation

    private BaseMultiset EvaluateParallelJoin(ParallelJoin join, SparqlEvaluationContext context)
    {
        // Create a copy of the evaluation context for the RHS
        var context2 =
            new SparqlEvaluationContext(context.Query, context.Data, context.Processor, context.Options);
        if (context.InputMultiset is not IdentityMultiset)
        {
            context2.InputMultiset = new Multiset();
            foreach (ISet s in context.InputMultiset.Sets)
            {
                context2.InputMultiset.Add(s.Copy());
            }
        }

        var activeGraphs = context.Data.ActiveGraphNames.ToList();
        var defaultGraphs = context.Data.DefaultGraphNames.ToList();

        // Start both executing asynchronously
        var cts = new CancellationTokenSource();
        CancellationToken cancellationToken = cts.Token;
        Task<BaseMultiset> lhsEvaluation =
            Task.Factory.StartNew(() => ParallelEvaluate(join.Lhs, context, activeGraphs, defaultGraphs),
                cancellationToken);
        Task<BaseMultiset> rhsEvaluation =
            Task.Factory.StartNew(() => ParallelEvaluate(join.Rhs, context2, activeGraphs, defaultGraphs),
                cancellationToken);
        var evaluationTasks = new Task[] { lhsEvaluation, rhsEvaluation };
        try
        {
            if (context.RemainingTimeout > 0)
            {
                Task.WaitAny(evaluationTasks, (int)context.RemainingTimeout, cancellationToken);
            }
            else
            {
                Task.WaitAny(evaluationTasks, cancellationToken);
            }

            BaseMultiset firstResult = lhsEvaluation.IsCompleted ? lhsEvaluation.Result : rhsEvaluation.Result;
            if (firstResult == null)
            {
                context.OutputMultiset = new NullMultiset();
                cts.Cancel();
            }
            else if (firstResult is NullMultiset)
            {
                context.OutputMultiset = new NullMultiset();
                cts.Cancel();
            }
            else
            {
                context.CheckTimeout();
                if (context.RemainingTimeout > 0)
                {
                    Task.WaitAll(evaluationTasks, (int)context.RemainingTimeout, cancellationToken);
                }
                else
                {
                    Task.WaitAll(evaluationTasks, cancellationToken);
                }

                BaseMultiset lhsResult = lhsEvaluation.Result;
                BaseMultiset rhsResult = rhsEvaluation.Result;
                if (lhsResult is NullMultiset)
                {
                    context.OutputMultiset = lhsResult;
                }
                else if (rhsResult is NullMultiset)
                {
                    context.OutputMultiset = rhsResult;
                }
                else if (lhsResult == null || rhsResult == null)
                {
                    context.OutputMultiset = new NullMultiset();
                }
                else
                {
                    context.OutputMultiset = lhsResult.Product(rhsResult);
                }
            }

            return context.OutputMultiset;
        }
        catch (OperationCanceledException)
        {
            throw new RdfQueryTimeoutException("Query Execution Time exceeded the Timeout of " +
                                               context.QueryTimeout + "ms, query aborted after " +
                                               context.QueryTime + "ms");
        }
        catch (AggregateException ex)
        {
            Exception firstCause = ex.InnerExceptions.FirstOrDefault();
            if (firstCause is RdfException) throw firstCause;
            throw new RdfQueryException("Error in parallel join evaluation.", ex);
        }
        catch (RdfException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RdfQueryException("Error in parallel join evaluation.", ex);
        }


    }

    private BaseMultiset ParallelEvaluate(ISparqlAlgebra algebra, SparqlEvaluationContext context, IList<IRefNode> activeGraphs, IList<IRefNode> defGraphs)
    {
        bool activeGraphOk = false, defaultGraphOk = false;
        try
        {
            // Set the Active Graph
            if (activeGraphs.Any())
            {
                context.Data.SetActiveGraph(activeGraphs);
                activeGraphOk = true;
            }
            // Set the Default Graph
            if (defGraphs.Any())
            {
                context.Data.SetDefaultGraph(defGraphs);
                defaultGraphOk = true;
            }

            // Evaluate the algebra and return the result
            return context.Evaluate(algebra);
        }
        finally
        {
            if (defaultGraphOk)
            {
                try
                {
                    context.Data.ResetDefaultGraph();
                }
                catch
                {
                    // Ignore reset exceptions
                }
            }
            if (activeGraphOk)
            {
                try
                {
                    context.Data.ResetActiveGraph();
                }
                catch
                {
                    // Ignore reset exceptions
                }
            }
        }
    }

    #endregion

    #region ParallelUnion Evaluation

    private BaseMultiset EvaluateParallelUnion(ParallelUnion union, SparqlEvaluationContext context)
    {
        // Create a copy of the evaluation context for the RHS
        var context2 = new SparqlEvaluationContext(context.Query, context.Data, context.Processor, context.Options);
        if (!(context.InputMultiset is IdentityMultiset))
        {
            context2.InputMultiset = new Multiset();
            foreach (ISet s in context.InputMultiset.Sets)
            {
                context2.InputMultiset.Add(s.Copy());
            }
        }

        var activeGraphs = context.Data.ActiveGraphNames.ToList();
        var defaultGraphs = context.Data.ActiveGraphNames.ToList();

        Task<BaseMultiset> lhsTask = Task.Factory.StartNew(() => ParallelEvaluate(union.Lhs, context, activeGraphs, defaultGraphs));
        Task<BaseMultiset> rhsTask = Task.Factory.StartNew(() => ParallelEvaluate(union.Rhs, context2, activeGraphs, defaultGraphs));
        Task[] evaluationTasks = { lhsTask, rhsTask };
        try
        {
            Task.WaitAll(evaluationTasks);
            context.CheckTimeout();
            BaseMultiset lhsResult = lhsTask.Result;
            BaseMultiset rhsResult = rhsTask.Result;
            context.OutputMultiset = lhsResult.Union(rhsResult);
            context.CheckTimeout();
            context.InputMultiset = context.OutputMultiset;
            return context.OutputMultiset;
        }
        catch (AggregateException ex)
        {
            if (ex.InnerExceptions.Any())
            {
                throw ex.InnerExceptions.First();
            }

            throw;
        }
    }
    #endregion
}
