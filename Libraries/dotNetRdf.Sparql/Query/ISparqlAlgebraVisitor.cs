using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Filters;

namespace VDS.RDF.Query
{
    public interface ISparqlAlgebraVisitor<out T>
    {
        T VisitAsk(Ask ask);

        T VisitAskAnyTriples(AskAnyTriples askAny);

        /// <summary>
        /// Visits a BGP.
        /// </summary>
        /// <param name="bgp">BGP.</param>
        T VisitBgp(Bgp bgp);

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
        /// Visits a subquery.
        /// </summary>
        /// <param name="subquery">Subquery.</param>
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
    }
}
