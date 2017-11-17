/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Linq;
using System.Threading;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Describe;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Default SPARQL Query Processor provided by the library's Leviathan SPARQL Engine
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Leviathan Query Processor simply invokes the <see cref="ISparqlAlgebra">Evaluate</see> method of the SPARQL Algebra it is asked to process
    /// </para>
    /// <para>
    /// In future releases much of the Leviathan Query engine logic will be moved into this class to make it possible for implementors to override specific bits of the algebra processing but this is not possible at this time
    /// </para>
    /// </remarks>
    public class LeviathanQueryProcessor 
        : ISparqlQueryProcessor, ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext>
    {
        private ISparqlDataset _dataset;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Creates a new Leviathan Query Processor
        /// </summary>
        /// <param name="store">Triple Store</param>
        public LeviathanQueryProcessor(IInMemoryQueryableStore store)
            : this(new InMemoryDataset(store)) { }

        /// <summary>
        /// Creates a new Leviathan Query Processor
        /// </summary>
        /// <param name="data">SPARQL Dataset</param>
        public LeviathanQueryProcessor(ISparqlDataset data)
        {
            _dataset = data;

            if (!_dataset.UsesUnionDefaultGraph)
            {
                if (!_dataset.HasGraph(null))
                {
                    // Create the Default unnamed Graph if it doesn't exist and then Flush() the change
                    _dataset.AddGraph(new Graph());
                    _dataset.Flush();
                }
            }
        }

        /// <summary>
        /// Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public Object ProcessQuery(SparqlQuery query)
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
                    SparqlResultSet results = new SparqlResultSet();
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
        /// Processes a SPARQL Query sending the results to a RDF/SPARQL Results handler as appropriate
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
        {
            // Do Handler null checks before evaluating the query
            if (query == null) throw new ArgumentNullException("query", "Cannot evaluate a null query");
            if (rdfHandler == null && (query.QueryType == SparqlQueryType.Construct || query.QueryType == SparqlQueryType.Describe || query.QueryType == SparqlQueryType.DescribeAll)) throw new ArgumentNullException("rdfHandler", "Cannot use a null RDF Handler when the Query is a CONSTRUCT/DESCRIBE");
            if (resultsHandler == null && (query.QueryType == SparqlQueryType.Ask || SparqlSpecsHelper.IsSelectQuery(query.QueryType))) throw new ArgumentNullException("resultsHandler", "Cannot use a null resultsHandler when the Query is an ASK/SELECT");

            // Handle the Thread Safety of the Query Evaluation
            ReaderWriterLockSlim currLock = (_dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)_dataset).Lock : _lock;
            try
            {
                currLock.EnterReadLock();
                // Reset Query Timers
                query.QueryExecutionTime = null;

                bool datasetOk = false, defGraphOk = false;

                try
                {
                    // Set up the Default and Active Graphs
                    if (query.DefaultGraphs.Any())
                    {
                        // Call HasGraph() on each Default Graph but ignore the results, we just do this
                        // in case a dataset has any kind of load on demand behaviour
                        foreach (Uri defGraphUri in query.DefaultGraphs)
                        {
                            _dataset.HasGraph(defGraphUri);
                        }
                        _dataset.SetDefaultGraph(query.DefaultGraphs);
                        defGraphOk = true;
                    }
                    else if (query.NamedGraphs.Any())
                    {
                        // No FROM Clauses but one/more FROM NAMED means the Default Graph is the empty graph
                        _dataset.SetDefaultGraph(Enumerable.Empty<Uri>());
                    }
                    _dataset.SetActiveGraph(_dataset.DefaultGraphUris);
                    datasetOk = true;

                    // Convert to Algebra and execute the Query
                    SparqlEvaluationContext context = GetContext(query);
                    BaseMultiset result;
                    try
                    {
                        context.StartExecution();
                        ISparqlAlgebra algebra = query.ToAlgebra();
                        result = context.Evaluate(algebra);

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

                                foreach (String prefix in query.NamespaceMap.Prefixes)
                                {
                                    if (!rdfHandler.HandleNamespace(prefix, query.NamespaceMap.GetNamespaceUri(prefix))) ParserHelper.Stop();
                                }

                                // Construct the Triples for each Solution
                                if (context.OutputMultiset is IdentityMultiset) context.OutputMultiset = new SingletonMultiset();
                                foreach (ISet s in context.OutputMultiset.Sets)
                                {
                                    // List<Triple> constructedTriples = new List<Triple>();
                                    try
                                    {
                                        ConstructContext constructContext = new ConstructContext(rdfHandler, s, false);
                                        foreach (IConstructTriplePattern p in query.ConstructTemplate.TriplePatterns.OfType<IConstructTriplePattern>())
                                        {
                                            try

                                            {
                                                if (!rdfHandler.HandleTriple(p.Construct(constructContext))) ParserHelper.Stop();
                                                // constructedTriples.Add(((IConstructTriplePattern)p).Construct(constructContext));
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
                                    // h.Assert(constructedTriples);
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
                            ISparqlDescribe describer = query.Describer;
                            describer.Describe(rdfHandler, context);
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
        /// Delegate used for asychronous execution
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        private delegate void ProcessQueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query);

        /// <summary>
        /// Processes a SPARQL Query asynchronously invoking the relevant callback when the query completes
        /// </summary>
        /// <param name="query">SPARQL QUery</param>
        /// <param name="rdfCallback">Callback for queries that return a Graph</param>
        /// <param name="resultsCallback">Callback for queries that return a Result Set</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// In the event of a success the appropriate callback will be invoked, if there is an error both callbacks will be invoked and passed an instance of <see cref="AsyncError"/> which contains details of the error and the original state information passed in.
        /// </remarks>
        public void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback, Object state)
        {
            Graph g = new Graph();
            SparqlResultSet rset = new SparqlResultSet();
            ProcessQueryAsync d = new ProcessQueryAsync(ProcessQuery);
            d.BeginInvoke(new GraphHandler(g), new ResultSetHandler(rset), query, r =>
            {
                try
                {
                    d.EndInvoke(r);
                    if (rset.ResultsType != SparqlResultsType.Unknown)
                    {
                        resultsCallback(rset, state);
                    }
                    else
                    {
                        rdfCallback(g, state);
                    }
                }
                catch (RdfQueryException queryEx)
                {
                    if (rdfCallback != null) rdfCallback(null, new AsyncError(queryEx, state));
                    if (resultsCallback != null) resultsCallback(null, new AsyncError(queryEx, state));
                }
                catch (Exception ex)
                {
                    RdfQueryException queryEx = new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex);
                    if (rdfCallback != null) rdfCallback(null, new AsyncError(queryEx, state));
                    if (resultsCallback != null) resultsCallback(null, new AsyncError(queryEx, state));
                }
            }, state);
        }

        /// <summary>
        /// Processes a SPARQL Query asynchronously passing the results to the relevant handler and invoking the callback when the query completes
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// In the event of a success the callback will be invoked, if there is an error the callback will be invoked and passed an instance of <see cref="AsyncError"/> which contains details of the error and the original state information passed in.
        /// </remarks>
        public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query, QueryCallback callback, Object state)
        {
            ProcessQueryAsync d = new ProcessQueryAsync(ProcessQuery);
            d.BeginInvoke(rdfHandler, resultsHandler, query, r =>
            {
                try
                {
                    d.EndInvoke(r);
                    callback(rdfHandler, resultsHandler, state);
                }
                catch (RdfQueryException queryEx)
                {
                    callback(rdfHandler, resultsHandler, new AsyncError(queryEx, state));
                }
                catch (Exception ex)
                {
                    callback(rdfHandler, resultsHandler, new AsyncError(new RdfQueryException("Unexpected error making an asynchronous query", ex), state));
                }
            }, state);
        }

        /// <summary>
        /// Creates a new Evaluation Context
        /// </summary>
        /// <returns></returns>
        protected SparqlEvaluationContext GetContext()
        {
            return GetContext(null);
        }

        /// <summary>
        /// Creates a new Evaluation Context for the given Query
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        private SparqlEvaluationContext GetContext(SparqlQuery q)
        {
            return new SparqlEvaluationContext(q, _dataset, GetProcessorForContext());
        }

        /// <summary>
        /// Gets the Query Processor for a Context
        /// </summary>
        /// <returns></returns>
        private ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> GetProcessorForContext()
        {
            if (GetType().Equals(typeof(LeviathanQueryProcessor)))
            {
                return null;
            }
            else
            {
                return this;
            }
        }

        #region Algebra Processor Implementation

        /// <summary>
        /// Processes SPARQL Algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessAlgebra(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            if (algebra is Ask)
            {
                return ProcessAsk((Ask)algebra, context);
            }
            else if (algebra is IBgp)
            {
                return ProcessBgp((IBgp)algebra, context);
            }
            else if (algebra is Bindings)
            {
                return ProcessBindings((Bindings)algebra, context);
            }
            else if (algebra is Distinct)
            {
                return ProcessDistinct((Distinct)algebra, context);
            }
            else if (algebra is Extend)
            {
                return ProcessExtend((Extend)algebra, context);
            }
            else if (algebra is IExistsJoin)
            {
                return ProcessExistsJoin((IExistsJoin)algebra, context);
            }
            else if (algebra is IFilter)
            {
                return ProcessFilter((IFilter)algebra, context);
            }
            else if (algebra is Algebra.Graph)
            {
                return ProcessGraph((Algebra.Graph)algebra, context);
            }
            else if (algebra is GroupBy)
            {
                return ProcessGroupBy((GroupBy)algebra, context);
            }
            else if (algebra is Having)
            {
                return ProcessHaving((Having)algebra, context);
            }
            else if (algebra is IJoin)
            {
                return ProcessJoin((IJoin)algebra, context);
            }
            else if (algebra is ILeftJoin)
            {
                return ProcessLeftJoin((ILeftJoin)algebra, context);
            }
            else if (algebra is IMinus)
            {
                return ProcessMinus((IMinus)algebra, context);
            }
            else if (algebra is NegatedPropertySet)
            {
                return ProcessNegatedPropertySet((NegatedPropertySet)algebra, context);
            }
            else if (algebra is NullOperator)
            {
                return ProcessNullOperator((NullOperator)algebra, context);
            }
            else if (algebra is OneOrMorePath)
            {
                return ProcessOneOrMorePath((OneOrMorePath)algebra, context);
            }
            else if (algebra is OrderBy)
            {
                return ProcessOrderBy((OrderBy)algebra, context);
            }
            else if (algebra is PropertyPath)
            {
                return ProcessPropertyPath((PropertyPath)algebra, context);
            }
            else if (algebra is Reduced)
            {
                return ProcessReduced((Reduced)algebra, context);
            }
            else if (algebra is Select)
            {
                return ProcessSelect((Select)algebra, context);
            }
            else if (algebra is SelectDistinctGraphs)
            {
                return ProcessSelectDistinctGraphs((SelectDistinctGraphs)algebra, context);
            }
            else if (algebra is Service)
            {
                return ProcessService((Service)algebra, context);
            }
            else if (algebra is Slice)
            {
                return ProcessSlice((Slice)algebra, context);
            }
            else if (algebra is SubQuery)
            {
                return ProcessSubQuery((SubQuery)algebra, context);
            }
            else if (algebra is IUnion)
            {
                return ProcessUnion((IUnion)algebra, context);
            }
            else if (algebra is ZeroLengthPath)
            {
                return ProcessZeroLengthPath((ZeroLengthPath)algebra, context);
            }
            else if (algebra is ZeroOrMorePath)
            {
                return ProcessZeroOrMorePath((ZeroOrMorePath)algebra, context);
            }
            else
            {
                // Unknown Algebra
                return ProcessUnknownOperator(algebra, context);
            }
        }

        /// <summary>
        /// Processes an Ask
        /// </summary>
        /// <param name="ask">Ask</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessAsk(Ask ask, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return ask.Evaluate(context);
        }

        /// <summary>
        /// Processes a BGP
        /// </summary>
        /// <param name="bgp">BGP</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessBgp(IBgp bgp, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return bgp.Evaluate(context);
        }

        /// <summary>
        /// Processes a Bindings modifier
        /// </summary>
        /// <param name="b">Bindings</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessBindings(Bindings b, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return b.Evaluate(context);
        }

        /// <summary>
        /// Processes a Distinct modifier
        /// </summary>
        /// <param name="distinct">Distinct modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessDistinct(Distinct distinct, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return distinct.Evaluate(context);
        }

        /// <summary>
        /// Processes an Extend
        /// </summary>
        /// <param name="extend">Extend</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessExtend(Extend extend, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return extend.Evaluate(context);
        }

        /// <summary>
        /// Processes an Exists Join
        /// </summary>
        /// <param name="existsJoin">Exists Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessExistsJoin(IExistsJoin existsJoin, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return existsJoin.Evaluate(context);
        }

        /// <summary>
        /// Processes a Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessFilter(IFilter filter, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return filter.Evaluate(context);
        }

        /// <summary>
        /// Processes a Graph
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessGraph(Algebra.Graph graph, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return graph.Evaluate(context);
        }

        /// <summary>
        /// Processes a Group By
        /// </summary>
        /// <param name="groupBy">Group By</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessGroupBy(GroupBy groupBy, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return groupBy.Evaluate(context);
        }

        /// <summary>
        /// Processes a Having
        /// </summary>
        /// <param name="having">Having</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessHaving(Having having, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return having.Evaluate(context);
        }

        /// <summary>
        /// Processes a Join
        /// </summary>
        /// <param name="join">Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessJoin(IJoin join, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return join.Evaluate(context);
        }

        /// <summary>
        /// Processes a LeftJoin
        /// </summary>
        /// <param name="leftJoin">Left Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessLeftJoin(ILeftJoin leftJoin, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return leftJoin.Evaluate(context);
        }

        /// <summary>
        /// Processes a Minus
        /// </summary>
        /// <param name="minus">Minus</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessMinus(IMinus minus, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return minus.Evaluate(context);
        }

        /// <summary>
        /// Processes a Negated Property Set
        /// </summary>
        /// <param name="negPropSet">Negated Property Set</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public virtual BaseMultiset ProcessNegatedPropertySet(NegatedPropertySet negPropSet, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return negPropSet.Evaluate(context);
        }

        /// <summary>
        /// Processes a Null Operator
        /// </summary>
        /// <param name="nullOp">Null Operator</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public virtual BaseMultiset ProcessNullOperator(NullOperator nullOp, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return nullOp.Evaluate(context);
        }

        /// <summary>
        /// Processes a One or More Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public virtual BaseMultiset ProcessOneOrMorePath(OneOrMorePath path, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return path.Evaluate(context);
        }

        /// <summary>
        /// Processes an Order By
        /// </summary>
        /// <param name="orderBy"></param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessOrderBy(OrderBy orderBy, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return orderBy.Evaluate(context);
        }

        /// <summary>
        /// Processes a Property Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public virtual BaseMultiset ProcessPropertyPath(PropertyPath path, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return path.Evaluate(context);
        }

        /// <summary>
        /// Processes a Reduced modifier
        /// </summary>
        /// <param name="reduced">Reduced modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessReduced(Reduced reduced, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return reduced.Evaluate(context);
        }

        /// <summary>
        /// Processes a Select
        /// </summary>
        /// <param name="select">Select</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessSelect(Select select, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return select.Evaluate(context);
        }

        /// <summary>
        /// Processes a Select Distinct Graphs
        /// </summary>
        /// <param name="selDistGraphs">Select Distinct Graphs</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return selDistGraphs.Evaluate(context);
        }

        /// <summary>
        /// Processes a Service
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessService(Service service, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return service.Evaluate(context);
        }

        /// <summary>
        /// Processes a Slice modifier
        /// </summary>
        /// <param name="slice">Slice modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessSlice(Slice slice, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return slice.Evaluate(context);
        }

        /// <summary>
        /// Processes a Subquery
        /// </summary>
        /// <param name="subquery">Subquery</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public virtual BaseMultiset ProcessSubQuery(SubQuery subquery, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return subquery.Evaluate(context);
        }

        /// <summary>
        /// Processes a Union
        /// </summary>
        /// <param name="union">Union</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessUnion(IUnion union, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return union.Evaluate(context);
        }

        /// <summary>
        /// Processes a Unknown Operator
        /// </summary>
        /// <param name="algebra">Unknown Operator</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessUnknownOperator(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return algebra.Evaluate(context);
        }

        /// <summary>
        /// Processes a Zero Length Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public virtual BaseMultiset ProcessZeroLengthPath(ZeroLengthPath path, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return path.Evaluate(context);
        }

        /// <summary>
        /// Processes a Zero or More Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public virtual BaseMultiset ProcessZeroOrMorePath(ZeroOrMorePath path, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return path.Evaluate(context);
        }

        #endregion
    }
}
