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
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Interface for SPARQL Query Processors
    /// </summary>
    /// <remarks>
    /// <para>
    /// A SPARQL Query Processor is a class that knows how to evaluate SPARQL queries against some data source to which the processor has access
    /// </para>
    /// <para>
    /// The point of this interface is to allow for end users to implement custom query processors or to extend and modify the behaviour of the default Leviathan engine as required.
    /// </para>
    /// </remarks>
    public interface ISparqlQueryProcessor
    {
        /// <summary>
        /// Processes a SPARQL Query returning a <see cref="IGraph">IGraph</see> instance or a <see cref="SparqlResultSet">SparqlResultSet</see> depending on the type of the query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns>
        /// Either an <see cref="IGraph">IGraph</see> instance of a <see cref="SparqlResultSet">SparqlResultSet</see> depending on the type of the query
        /// </returns>
        Object ProcessQuery(SparqlQuery query);

        /// <summary>
        /// Processes a SPARQL Query passing the results to the RDF or Results handler as appropriate
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query);

        /// <summary>
        /// Processes a SPARQL Query asynchronously invoking the relevant callback when the query completes
        /// </summary>
        /// <param name="query">SPARQL QUery</param>
        /// <param name="rdfCallback">Callback for queries that return a Graph</param>
        /// <param name="resultsCallback">Callback for queries that return a Result Set</param>
        /// <param name="state">State to pass to the callback</param>
        void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback, Object state);

        /// <summary>
        /// Processes a SPARQL Query asynchronously passing the results to the relevant handler and invoking the callback when the query completes
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query, QueryCallback callback, Object state);

    }

    /// <summary>
    /// Interface for SPARQL Query Algebra Processors
    /// </summary>
    /// <remarks>
    /// A SPARQL Query Algebra Processor is a class which knows how to evaluate the
    /// </remarks>
    /// <typeparam name="TResult">Type of intermediate results produced by processing an Algebra operator</typeparam>
    /// <typeparam name="TContext">Type of context object providing evaluation context</typeparam>
    public interface ISparqlQueryAlgebraProcessor<TResult, TContext>
    {
        /// <summary>
        /// Processes SPARQL Algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessAlgebra(ISparqlAlgebra algebra, TContext context);

        /// <summary>
        /// Processes an Ask
        /// </summary>
        /// <param name="ask">Ask</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessAsk(Ask ask, TContext context);

        /// <summary>
        /// Processes a BGP
        /// </summary>
        /// <param name="bgp">BGP</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessBgp(IBgp bgp, TContext context);

        /// <summary>
        /// Processes a Bindings modifier
        /// </summary>
        /// <param name="b">Bindings</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessBindings(Bindings b, TContext context);

        /// <summary>
        /// Processes a Distinct modifier
        /// </summary>
        /// <param name="distinct">Distinct modifier</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessDistinct(Distinct distinct, TContext context);

        /// <summary>
        /// Processes an Exists Join
        /// </summary>
        /// <param name="existsJoin">Exists Join</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessExistsJoin(IExistsJoin existsJoin, TContext context);

        /// <summary>
        /// Processes an Extend
        /// </summary>
        /// <param name="extend">Extend</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        TResult ProcessExtend(Extend extend, TContext context);

        /// <summary>
        /// Processes a Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessFilter(IFilter filter, TContext context);

        /// <summary>
        /// Processes a Graph
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessGraph(Algebra.Graph graph, TContext context);

        /// <summary>
        /// Processes a Group By
        /// </summary>
        /// <param name="groupBy">Group By</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessGroupBy(GroupBy groupBy, TContext context);

        /// <summary>
        /// Processes a Having
        /// </summary>
        /// <param name="having">Having</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessHaving(Having having, TContext context);

        /// <summary>
        /// Processes a Join
        /// </summary>
        /// <param name="join">Join</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessJoin(IJoin join, TContext context);

        /// <summary>
        /// Processes a LeftJoin
        /// </summary>
        /// <param name="leftJoin">Left Join</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessLeftJoin(ILeftJoin leftJoin, TContext context);

        /// <summary>
        /// Processes a Minus
        /// </summary>
        /// <param name="minus">Minus</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessMinus(IMinus minus, TContext context);

        /// <summary>
        /// Processes a Negated Property Set
        /// </summary>
        /// <param name="negPropSet">Negated Property Set</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        TResult ProcessNegatedPropertySet(NegatedPropertySet negPropSet, TContext context);

        /// <summary>
        /// Processes a Null Operator
        /// </summary>
        /// <param name="nullOp">Null Operator</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        TResult ProcessNullOperator(NullOperator nullOp, TContext context);

        /// <summary>
        /// Processes a One or More Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        TResult ProcessOneOrMorePath(OneOrMorePath path, TContext context);

        /// <summary>
        /// Processes an Order By
        /// </summary>
        /// <param name="orderBy"></param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessOrderBy(OrderBy orderBy, TContext context);

        /// <summary>
        /// Processes a Property Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        TResult ProcessPropertyPath(PropertyPath path, TContext context);

        /// <summary>
        /// Processes a Reduced modifier
        /// </summary>
        /// <param name="reduced">Reduced modifier</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessReduced(Reduced reduced, TContext context);

        /// <summary>
        /// Processes a Select
        /// </summary>
        /// <param name="select">Select</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessSelect(Select select, TContext context);

        /// <summary>
        /// Processes a Select Distinct Graphs
        /// </summary>
        /// <param name="selDistGraphs">Select Distinct Graphs</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs, TContext context);

        /// <summary>
        /// Processes a Service
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessService(Service service, TContext context);

        /// <summary>
        /// Processes a Slice modifier
        /// </summary>
        /// <param name="slice">Slice modifier</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessSlice(Slice slice, TContext context);
        
        /// <summary>
        /// Processes a subquery
        /// </summary>
        /// <param name="subquery">Subquery</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        TResult ProcessSubQuery(SubQuery subquery, TContext context);

        /// <summary>
        /// Processes a Union
        /// </summary>
        /// <param name="union">Union</param>
        /// <param name="context">Evaluation Context</param>
        TResult ProcessUnion(IUnion union, TContext context);

        /// <summary>
        /// Processes an Unknown Operator
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        TResult ProcessUnknownOperator(ISparqlAlgebra algebra, TContext context);

        /// <summary>
        /// Processes a Zero Length Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        TResult ProcessZeroLengthPath(ZeroLengthPath path, TContext context);

        /// <summary>
        /// Processes a Zero or More Path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        TResult ProcessZeroOrMorePath(ZeroOrMorePath path, TContext context);
    }

    // public interface ISparqlPatternProcessor<TResult, TContext> : ISparqlQueryAlgebraProcessor<TResult, TContext>
    // {
    //    void ProcessPattern(ITriplePattern
    // }
}
