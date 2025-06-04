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
/// The interface for a visitor object that can visit the elements of a SPARQL algebra tree.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISparqlAlgebraVisitor<out T>
{
    /// <summary>
    /// Visits an ASK.
    /// </summary>
    /// <param name="ask"></param>
    /// <returns></returns>
    T VisitAsk(Ask ask);

    /// <summary>
    /// Visits an ASK *.
    /// </summary>
    /// <param name="askAny"></param>
    /// <returns></returns>
    T VisitAskAnyTriples(AskAnyTriples askAny);

    /// <summary>
    /// Visits a BGP.
    /// </summary>
    /// <param name="bgp">BGP.</param>
    T VisitBgp(IBgp bgp);

    /// <summary>
    /// Visits a Bindings modifier.
    /// </summary>
    /// <param name="b">Bindings.</param>
    T VisitBindings(Bindings b);

    /// <summary>
    /// Visits a Distinct modifier.
    /// </summary>
    /// <param name="distinct">Distinct modifier.</param>
    T VisitDistinct(Distinct distinct);

    /// <summary>
    /// Visits an Exists Join.
    /// </summary>
    /// <param name="existsJoin">Exists Join.</param>
    T VisitExistsJoin(IExistsJoin existsJoin);

    /// <summary>
    /// Visits an Extend.
    /// </summary>
    /// <param name="extend">Extend.</param>
    /// <returns></returns>
    T VisitExtend(Extend extend);

    /// <summary>
    /// Visits a Filter.
    /// </summary>
    /// <param name="filter">Filter.</param>
    T VisitFilter(IFilter filter);

    /// <summary>
    /// Visits a Graph.
    /// </summary>
    /// <param name="graph">Graph.</param>
    T VisitGraph(Algebra.Graph graph);

    /// <summary>
    /// Visits a Group By.
    /// </summary>
    /// <param name="groupBy">Group By.</param>
    T VisitGroupBy(GroupBy groupBy);

    /// <summary>
    /// Visits a Having.
    /// </summary>
    /// <param name="having">Having.</param>
    T VisitHaving(Having having);

    /// <summary>
    /// Visits a Join.
    /// </summary>
    /// <param name="join">Join.</param>
    T VisitJoin(IJoin join);

    /// <summary>
    /// Visits a LeftJoin.
    /// </summary>
    /// <param name="leftJoin">Left Join.</param>
    T VisitLeftJoin(ILeftJoin leftJoin);

    /// <summary>
    /// Visits a Minus.
    /// </summary>
    /// <param name="minus">Minus.</param>
    T VisitMinus(IMinus minus);

    /// <summary>
    /// Visits a Negated Property Set.
    /// </summary>
    /// <param name="negPropSet">Negated Property Set.</param>
    /// <returns></returns>
    T VisitNegatedPropertySet(NegatedPropertySet negPropSet);

    /// <summary>
    /// Visits a Null Operator.
    /// </summary>
    /// <param name="nullOp">Null Operator.</param>
    /// <returns></returns>
    T VisitNullOperator(NullOperator nullOp);

    /// <summary>
    /// Visits a One or More Path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <returns></returns>
    T VisitOneOrMorePath(OneOrMorePath path);

    /// <summary>
    /// Visits an Order By.
    /// </summary>
    /// <param name="orderBy"></param>
    T VisitOrderBy(OrderBy orderBy);

    /// <summary>
    /// Visits a Property Path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <returns></returns>
    T VisitPropertyPath(PropertyPath path);

    /// <summary>
    /// Visits a Reduced modifier.
    /// </summary>
    /// <param name="reduced">Reduced modifier.</param>
    T VisitReduced(Reduced reduced);

    /// <summary>
    /// Visits a Select.
    /// </summary>
    /// <param name="select">Select.</param>
    T VisitSelect(Select select);

    /// <summary>
    /// Visits a Select Distinct Graphs.
    /// </summary>
    /// <param name="selDistGraphs">Select Distinct Graphs.</param>
    T VisitSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs);

    /// <summary>
    /// Visits a Service.
    /// </summary>
    /// <param name="service">Service.</param>
    T VisitService(Service service);

    /// <summary>
    /// Visits a Slice modifier.
    /// </summary>
    /// <param name="slice">Slice modifier.</param>
    T VisitSlice(Slice slice);

    /// <summary>
    /// Visits a sub-query.
    /// </summary>
    /// <param name="subquery">Sub-query.</param>
    /// <returns></returns>
    T VisitSubQuery(SubQuery subquery);

    /// <summary>
    /// Visits a Union.
    /// </summary>
    /// <param name="union">Union.</param>
    T VisitUnion(IUnion union);

    /// <summary>
    /// Visits an Unknown Operator.
    /// </summary>
    /// <param name="algebra">Algebra.</param>
    /// <returns></returns>
    T VisitUnknownOperator(ISparqlAlgebra algebra);

    /// <summary>
    /// Visits a Zero Length Path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <returns></returns>
    T VisitZeroLengthPath(ZeroLengthPath path);

    /// <summary>
    /// Visits a Zero or More Path.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <returns></returns>
    T VisitZeroOrMorePath(ZeroOrMorePath path);

    /// <summary>
    /// Visit a Bound Filter.
    /// </summary>
    /// <param name="filter">Filter.</param>
    /// <returns></returns>
    T VisitBoundFilter(BoundFilter filter);
    /// <summary>
    /// Visit a Unary Expression Filter.
    /// </summary>
    /// <param name="filter">Filter.</param>
    /// <returns></returns>
    T VisitUnaryExpressionFilter(UnaryExpressionFilter filter);
    /// <summary>
    /// Visit a Chain Filter.
    /// </summary>
    /// <param name="filter">Filter.</param>
    /// <returns></returns>
    T VisitChainFilter(ChainFilter filter);

    /// <summary>
    /// Visit a Single Value Restriction Filter.
    /// </summary>
    /// <param name="filter">filter.</param>
    /// <returns></returns>
    T VisitSingleValueRestrictionFilter(SingleValueRestrictionFilter filter);

    /// <summary>
    /// Visit a BIND pattern.
    /// </summary>
    /// <param name="bindPattern"></param>
    /// <returns></returns>
    T VisitBindPattern(BindPattern bindPattern);

    /// <summary>
    /// Visit a FILTER pattern.
    /// </summary>
    /// <param name="filterPattern"></param>
    /// <returns></returns>
    T VisitFilterPattern(FilterPattern filterPattern);

    /// <summary>
    /// Visit a LET pattern.
    /// </summary>
    /// <param name="letPattern"></param>
    /// <returns></returns>
    T VisitLetPattern(LetPattern letPattern);
    
    /// <summary>
    /// Visit a property function.
    /// </summary>
    /// <param name="propertyFunction"></param>
    /// <returns></returns>
    T VisitPropertyFunction(PropertyFunction propertyFunction);

    /// <summary>
    /// Visit a property path.
    /// </summary>
    /// <param name="propertyPathPattern"></param>
    /// <returns></returns>
    T VisitPropertyPathPattern(PropertyPathPattern propertyPathPattern);

    /// <summary>
    /// Visit a sub-query pattern.
    /// </summary>
    /// <param name="subQueryPattern"></param>
    /// <returns></returns>
    T VisitSubQueryPattern(SubQueryPattern subQueryPattern);

    /// <summary>
    /// Visit a property function pattern.
    /// </summary>
    /// <param name="propFunctionPattern"></param>
    /// <returns></returns>
    T VisitPropertyFunctionPattern(PropertyFunctionPattern propFunctionPattern);

    /// <summary>
    /// Visit a triple pattern.
    /// </summary>
    /// <param name="triplePattern"></param>
    /// <returns></returns>
    T VisitTriplePattern(TriplePattern triplePattern);
}
