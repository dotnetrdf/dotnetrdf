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

using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query;

/// <summary>
/// Interface for SPARQL Query Algebra Processors.
/// </summary>
/// <remarks>
/// A SPARQL Query Algebra Processor is a class which knows how to evaluate the.
/// </remarks>
/// <typeparam name="TResult">Type of intermediate results produced by processing an Algebra operator.</typeparam>
/// <typeparam name="TContext">Type of context object providing evaluation context.</typeparam>
public interface ISparqlQueryAlgebraProcessor<out TResult, in TContext>
{
    /// <summary>
    /// Processes SPARQL Algebra.
    /// </summary>
    /// <param name="algebra">Algebra.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessAlgebra(ISparqlAlgebra algebra, TContext context);

    /// <summary>
    /// Processes an Ask.
    /// </summary>
    /// <param name="ask">Ask.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessAsk(Ask ask, TContext context);

    /// <summary>
    /// Processes an optimised ASK of the form ASK WHERE { ?s ?p ?o }.
    /// </summary>
    /// <param name="askAny"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessAskAnyTriples(AskAnyTriples askAny, TContext context);

    /// <summary>
    /// Processes a BGP.
    /// </summary>
    /// <param name="bgp">BGP.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessBgp(IBgp bgp, TContext context);

    /// <summary>
    /// Processes a Bindings modifier.
    /// </summary>
    /// <param name="b">Bindings.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessBindings(Bindings b, TContext context);

    /// <summary>
    /// Processes a Distinct modifier.
    /// </summary>
    /// <param name="distinct">Distinct modifier.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessDistinct(Distinct distinct, TContext context);

    /// <summary>
    /// Processes an Exists Join.
    /// </summary>
    /// <param name="existsJoin">Exists Join.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessExistsJoin(IExistsJoin existsJoin, TContext context);

    /// <summary>
    /// Processes an Extend.
    /// </summary>
    /// <param name="extend">Extend.</param>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    TResult ProcessExtend(Extend extend, TContext context);

    /// <summary>
    /// Processes a Filter.
    /// </summary>
    /// <param name="filter">Filter.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessFilter(IFilter filter, TContext context);

    /// <summary>
    /// Processes a Graph.
    /// </summary>
    /// <param name="graph">Graph.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessGraph(Algebra.Graph graph, TContext context);

    /// <summary>
    /// Processes a Group By.
    /// </summary>
    /// <param name="groupBy">Group By.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessGroupBy(GroupBy groupBy, TContext context);

    /// <summary>
    /// Processes a Having.
    /// </summary>
    /// <param name="having">Having.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessHaving(Having having, TContext context);

    /// <summary>
    /// Processes a Join.
    /// </summary>
    /// <param name="join">Join.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessJoin(IJoin join, TContext context);

    /// <summary>
    /// Processes a LeftJoin.
    /// </summary>
    /// <param name="leftJoin">Left Join.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessLeftJoin(ILeftJoin leftJoin, TContext context);

    /// <summary>
    /// Processes a Minus.
    /// </summary>
    /// <param name="minus">Minus.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessMinus(IMinus minus, TContext context);

    /// <summary>
    /// Processes a Negated Property Set.
    /// </summary>
    /// <param name="negPropSet">Negated Property Set.</param>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    TResult ProcessNegatedPropertySet(NegatedPropertySet negPropSet, TContext context);

    /// <summary>
    /// Processes a Null Operator.
    /// </summary>
    /// <param name="nullOp">Null Operator.</param>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    TResult ProcessNullOperator(NullOperator nullOp, TContext context);

    /// <summary>
    /// Processes a One or More Path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    TResult ProcessOneOrMorePath(OneOrMorePath path, TContext context);

    /// <summary>
    /// Processes an Order By.
    /// </summary>
    /// <param name="orderBy"></param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessOrderBy(OrderBy orderBy, TContext context);

    /// <summary>
    /// Processes a Property Path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    TResult ProcessPropertyPath(PropertyPath path, TContext context);

    /// <summary>
    /// Processes a Reduced modifier.
    /// </summary>
    /// <param name="reduced">Reduced modifier.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessReduced(Reduced reduced, TContext context);

    /// <summary>
    /// Processes a Select.
    /// </summary>
    /// <param name="select">Select.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessSelect(Select select, TContext context);

    /// <summary>
    /// Processes a Select Distinct Graphs.
    /// </summary>
    /// <param name="selDistGraphs">Select Distinct Graphs.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs, TContext context);

    /// <summary>
    /// Processes a Service.
    /// </summary>
    /// <param name="service">Service.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessService(Service service, TContext context);

    /// <summary>
    /// Processes a Slice modifier.
    /// </summary>
    /// <param name="slice">Slice modifier.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessSlice(Slice slice, TContext context);
    
    /// <summary>
    /// Processes a sub-query.
    /// </summary>
    /// <param name="subQuery">Sub-query.</param>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    TResult ProcessSubQuery(SubQuery subQuery, TContext context);

    /// <summary>
    /// Processes a Union.
    /// </summary>
    /// <param name="union">Union.</param>
    /// <param name="context">Evaluation Context.</param>
    TResult ProcessUnion(IUnion union, TContext context);

    /// <summary>
    /// Processes a Zero Length Path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    TResult ProcessZeroLengthPath(ZeroLengthPath path, TContext context);

    /// <summary>
    /// Processes a Zero or More Path.
    /// </summary>
    /// <param name="zeroOrMorePath">Path.</param>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    TResult ProcessZeroOrMorePath(ZeroOrMorePath zeroOrMorePath, TContext context);

    /// <summary>
    /// Process a <see cref="BoundFilter"/>.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessBoundFilter(BoundFilter filter, TContext context);

    /// <summary>
    /// Process a <see cref="UnaryExpressionFilter"/>.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessUnaryExpressionFilter(UnaryExpressionFilter filter, TContext context);

    /// <summary>
    /// Process a <see cref="ChainFilter"/>.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessChainFilter(ChainFilter filter, TContext context);

    /// <summary>
    /// Process a <see cref="SingleValueRestrictionFilter"/>.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessSingleValueRestrictionFilter(SingleValueRestrictionFilter filter, TContext context);

    /// <summary>
    /// Process a <see cref="BindPattern"/>.
    /// </summary>
    /// <param name="bindPattern"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessBindPattern(BindPattern bindPattern, TContext context);

    /// <summary>
    /// Process a <see cref="FilterPattern"/>.
    /// </summary>
    /// <param name="filterPattern"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessFilterPattern(FilterPattern filterPattern, TContext context);

    /// <summary>
    /// Process a <see cref="LetPattern"/>.
    /// </summary>
    /// <param name="letPattern"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessLetPattern(LetPattern letPattern, TContext context);

    /// <summary>
    /// Process a <see cref="PropertyFunction"/>.
    /// </summary>
    /// <param name="propertyFunction"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessPropertyFunction(PropertyFunction propertyFunction, TContext context);

    /// <summary>
    /// Process a <see cref="PropertyPathPattern"/>.
    /// </summary>
    /// <param name="propertyPathPattern"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessPropertyPathPattern(PropertyPathPattern propertyPathPattern, TContext context);

    /// <summary>
    /// Process a <see cref="SubQueryPattern"/>.
    /// </summary>
    /// <param name="subQueryPattern"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessSubQueryPattern(SubQueryPattern subQueryPattern, TContext context);

    /// <summary>
    /// Process a <see cref="PropertyFunctionPattern"/>.
    /// </summary>
    /// <param name="propFunctionPattern"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessPropertyFunctionPattern(PropertyFunctionPattern propFunctionPattern, TContext context);

    /// <summary>
    /// Process a <see cref="TriplePattern"/>.
    /// </summary>
    /// <param name="triplePattern"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessTriplePattern(TriplePattern triplePattern, TContext context);

    /// <summary>
    /// Invoked to process any other algebra class not covered above. In particular
    /// engine-specific optimised algebra classes.
    /// </summary>
    /// <param name="op"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    TResult ProcessUnknownOperator(ISparqlAlgebra op, TContext context);
}